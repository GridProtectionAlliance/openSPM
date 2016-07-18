//******************************************************************************************************
//  ServiceHost.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/25/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using GSF;
using GSF.Configuration;
using GSF.Data;
using GSF.Data.Model;
using GSF.Identity;
using GSF.IO;
using GSF.Net.Smtp;
using GSF.Security.Model;
using GSF.ServiceProcess;
using GSF.Threading;
using GSF.Units;
using openSPM.Model;

namespace EmailService
{
    public partial class ServiceHost : ServiceBase
    {
        #region [ Members ]

        // Fields
        private RunTimeLog m_runTimeLog;
        private string m_defaultFromEmailAddress;
        private string m_smtpServer;
        private string m_smtpUserName;
        private string m_smtpPassword;
        private readonly ConcurrentDictionary<Guid, string> m_emailAddressCache;
        private readonly LongSynchronizedOperation m_emailOperation;
        private readonly LongSynchronizedOperation m_timeStampUpdate;
        private readonly LongSynchronizedOperation m_emailNewItems;
        private string userEmail;

        #endregion

        #region [ Constructors ]

        public ServiceHost()
        {
            InitializeComponent();

            // Register event handlers.
            m_serviceHelper.ServiceStarting += ServiceHelper_ServiceStarting;
            m_serviceHelper.ServiceStarted += ServiceHelper_ServiceStarted;
            m_serviceHelper.ServiceStopping += ServiceHelper_ServiceStopping;

            m_serviceHelper.MonitorServiceHealth = true;

            if (m_serviceHelper.StatusLog != null)
                m_serviceHelper.StatusLog.LogException += LogExceptionHandler;

            if (m_serviceHelper.ErrorLogger?.ErrorLog != null)
                m_serviceHelper.ErrorLogger.ErrorLog.LogException += LogExceptionHandler;

            m_emailAddressCache = new ConcurrentDictionary<Guid, string>();
            m_emailOperation = new LongSynchronizedOperation(ProcessEmails, LogException);
            m_timeStampUpdate = new LongSynchronizedOperation(TimeStampUpdate, LogException);
            m_emailNewItems = new LongSynchronizedOperation(EmailNewItems, LogException);
        }

        public ServiceHost(IContainer container) : this()
        {
            container?.Add(this);
        }

        #endregion

        #region [ Methods ]

        private void PrimaryProcess(string processName, object[] processArguments)
        {
            // The primary process runs once per minute

            int dailyEmailTime = int.Parse(ConfigurationFile.Current.Settings["systemSettings"]["DailyEmailTime"].Value);

            m_timeStampUpdate.TryRunOnce();
            m_emailNewItems.TryRunOnce();
           
            //    if (DateTime.UtcNow.Minute % 5 == 0)
            //    {
            //        // This task will run every five minutes
            //    }

            //    if (DateTime.UtcNow.Minute == 0)
            //    {
            //        // This task will run every hour
            //    }

            if (DateTime.Now.Hour == dailyEmailTime && DateTime.Now.Minute == 0)
                {
                // This task will run once per day
                m_emailOperation.TryRunOnce();

            }
        }

        private void ProcessEmails()
        {
            ProcessOpenSPMEmails();
            ProcessMiPlanEmails();
        }

        private void TimeStampUpdate()
        {
            using (AdoDataConnection connection = new AdoDataConnection("openSPM"))
            {
                connection.ExecuteNonQuery("UPDATE EmailService SET TimeStamp = {0}", DateTime.Now);
                bool flag = connection.ExecuteScalar<bool>("Select TOP 1 Push FROM EmailService");
                if (flag)
                {
                    m_emailOperation.TryRunOnce();
                    connection.ExecuteNonQuery("UPDATE EmailService SET Push = 'False'");
                }
            }
        }

        private void EmailNewItems()
        {
            using (AdoDataConnection connection = new AdoDataConnection("openSPM"))
            {
                TableOperations<NewPatchesView> newPatches = new TableOperations<NewPatchesView>(connection);
                TableOperations<NoticeLog> logs = new TableOperations<NoticeLog>(connection);

                IEnumerable<NewPatchesView> patches = newPatches.QueryRecords();

                foreach (NewPatchesView patch in patches)
                {
                    string emailBody = "NOTIFICATION:" + "<br/>" +
                    "The following patch was just entered..." + "<br/>" +
                     "Patch: " + patch.VendorPatchName + "<br/>" +
                    "Business Unit: " + patch.BUName + "<br/>" +
                    "Platform: " + patch.PlatformName + "<br/>" +
                    "Deadline: " + patch.EvaluationDeadline;
                    string emailSubject = "New Patch: " + patch.VendorPatchName;

                    try
                    {
                        SendEmail(patch.SME, emailSubject, emailBody, "openSPM@tva.gov", "openSPM");
                        NoticeLog log = new NoticeLog();
                        log.CreatedOn = DateTime.UtcNow;
                        log.SentOn = DateTime.UtcNow;
                        log.PatchID = patch.PatchStatusID;
                        log.NoticeMethodKey = 1;
                        log.NoticeLevelKey = 2;
                        log.Text = emailBody;
                        log.ToUsers = userEmail;
                        logs.AddNewRecord(log);

                    }
                    catch (Exception ex)
                    {
                        LogException(ex);
                    }


                }


            }

            using (AdoDataConnection connection = new AdoDataConnection("miPlan"))
            {
                TableOperations<NewPlanView> newPlans = new TableOperations<NewPlanView>(connection);
                TableOperations<NoticeLog> logs = new TableOperations<NoticeLog>(connection);

                IEnumerable<NewPlanView> plans = newPlans.QueryRecords();

                foreach (NewPlanView plan in plans)
                {
                    string emailBody = "NOTIFICATION: <br/>" +
                                        "The following plan was just created...<br/>" +
                                         "Plan: " + plan.Title + "<br/>" +
                                         "Business Unit: " + plan.Name + "<br/>";

                    string emailSubject = "New plan submitted: " + plan.Title;

                    try
                    {
                        SendEmail(plan.UserAccountID, emailSubject, emailBody, "MiPlan@tva.gov", "miPlan");
                        NoticeLog log = new NoticeLog();
                        log.CreatedOn = DateTime.UtcNow;
                        log.SentOn = DateTime.UtcNow;
                        log.PatchID = plan.ID;
                        log.NoticeMethodKey = 1;
                        log.NoticeLevelKey = 1;
                        log.Text = emailBody;
                        log.ToUsers = userEmail;
                        logs.AddNewRecord(log);

                    }
                    catch (Exception ex)
                    {
                        LogException(ex);
                    }


                }

            }

        }

        private void ProcessOpenSPMEmails()
        {
            using (AdoDataConnection connection = new AdoDataConnection("openSPM"))
            {
                TableOperations<PendingAssessmentViolations> pavs = new TableOperations<PendingAssessmentViolations>(connection);
                TableOperations<PendingInstallationViolations> pivs = new TableOperations<PendingInstallationViolations>(connection);
                TableOperations<NoticeLog> logs = new TableOperations<NoticeLog>(connection);
                int groupID = connection.ExecuteScalar<int?>("SELECT ID FROM ValueListGroup WHERE Name = 'dayLimits'") ?? 0;
                TableOperations<ValueList> valueList = new TableOperations<ValueList>(connection);
                int criticalAlarm, warning, alarm, violation;
                ValueList[] alarms = valueList.QueryRecords("[Key]", restriction: new RecordRestriction("GroupID = {0}", groupID)).ToArray();
                if(groupID == 0)
                {
                    violation = 0;
                    alarm = 14;
                    warning = 21;
                    criticalAlarm = 3;   
                }
                else
                {
                    criticalAlarm = alarms[0].Value;
                    warning = alarms[3].Value - alarms[1].Value;
                    alarm = alarms[3].Value - alarms[2].Value;
                    violation = alarms[3].Value - alarms[3].Value;
                }

                IEnumerable<PendingAssessmentViolations> tr = pavs.QueryRecords();
          
                string emailSubject = "";

                foreach (PendingAssessmentViolations pav in tr)
                {
                    Debug.WriteLine(pav.VendorPatchName + " SME:" + pav.SME + " days left:" + pav.DaysTilViolation);
               
                    string emailBody = "NOTIFICATION:" + "<br/>" + 
                                        "The following patch is nearing the evalutation deadline..." + "<br/>" +
                                         "Patch: " + pav.VendorPatchName + "<br/>" +
                                        "Business Unit: " + pav.BUName + "<br/>" +
                                        "Platform: " + pav.PlatformName + "<br/>" +
                                        "Deadline: " + pav.EvaluationDeadline;
                    if ((DateTime.Now - pav.CreatedOn).Days < 1)
                    {
                        emailSubject = "New Patch: " + pav.VendorPatchName;
                        try
                        {
                            SendEmail(pav.SME, emailSubject, emailBody, "openSPM@tva.gov", "openSPM");
                            NoticeLog log = new NoticeLog();
                            log.CreatedOn = DateTime.UtcNow;
                            log.SentOn = DateTime.UtcNow;
                            log.PatchID = pav.ID;
                            log.NoticeMethodKey = 1;
                            log.NoticeLevelKey = 1;
                            log.Text = emailBody;
                            log.ToUsers = userEmail;
                            logs.AddNewRecord(log);
                        }
                        catch (Exception ex)
                        {
                            LogException(ex);
                        }

                    }
                    else if (pav.DaysTilViolation <= warning && pav.DaysTilViolation > alarm)
                    {
                        emailSubject = "Warning: " + pav.VendorPatchName + " approaching Evaluation Deadline";
                        try
                        {
                            SendEmail(pav.SME, emailSubject, emailBody, "openSPM@tva.gov", "openSPM");
                            NoticeLog log = new NoticeLog();
                            log.CreatedOn = DateTime.UtcNow;
                            log.SentOn = DateTime.UtcNow;
                            log.PatchID = pav.PatchStatusID;
                            log.NoticeMethodKey = 1;
                            log.NoticeLevelKey = 2;
                            log.Text = emailBody;
                            log.ToUsers = userEmail;
                            logs.AddNewRecord(log);

                        }
                        catch (Exception ex)
                        {
                            LogException(ex);
                        }
                    }
                    else if (pav.DaysTilViolation <= alarm && pav.DaysTilViolation > criticalAlarm)
                    {
                        emailSubject = "Alarm: " + pav.VendorPatchName + " approaching Evaluation Deadline";
                        try
                        {
                            SendEmail(pav.SME, emailSubject, emailBody, "openSPM@tva.gov", "openSPM");
                            NoticeLog log = new NoticeLog();
                            log.CreatedOn = DateTime.UtcNow;
                            log.SentOn = DateTime.UtcNow;
                            log.PatchID = pav.ID;
                            log.NoticeMethodKey = 1;
                            log.NoticeLevelKey = 3;
                            log.Text = emailBody;
                            log.ToUsers = userEmail;
                            logs.AddNewRecord(log);

                        }
                        catch (Exception ex)
                        {
                            LogException(ex);
                        }
                    }
                    else if (pav.DaysTilViolation <= criticalAlarm && pav.DaysTilViolation > violation)
                    {
                        emailSubject = "Critical Alarm: " + pav.VendorPatchName + " approaching Evaluation Deadline";
                        try
                        {
                            SendEmail(pav.SME, emailSubject, emailBody, "openSPM@tva.gov", "openSPM");
                            NoticeLog log = new NoticeLog();
                            log.CreatedOn = DateTime.UtcNow;
                            log.SentOn = DateTime.UtcNow;
                            log.PatchID = pav.ID;
                            log.NoticeMethodKey = 1;
                            log.NoticeLevelKey = 4;
                            log.Text = emailBody;
                            log.ToUsers = userEmail;
                            logs.AddNewRecord(log);

                        }
                        catch (Exception ex)
                        {
                            LogException(ex);
                        }
                    }
                    else if (pav.DaysTilViolation <= violation)
                    {
                        emailSubject = "Violation: " + pav.VendorPatchName + " passed Evaluation Deadline";
                        try
                        {
                            SendEmail(pav.SME, emailSubject, emailBody, "openSPM@tva.gov", "openSPM");
                            NoticeLog log = new NoticeLog();
                            log.CreatedOn = DateTime.UtcNow;
                            log.SentOn = DateTime.UtcNow;
                            log.PatchID = pav.ID;
                            log.NoticeMethodKey = 1;
                            log.NoticeLevelKey = 5;
                            log.Text = emailBody;
                            log.ToUsers = userEmail;
                            logs.AddNewRecord(log);

                        }
                        catch (Exception ex)
                        {
                            LogException(ex);
                        }
                    }



                }

                IEnumerable<PendingInstallationViolations> installTable = pivs.QueryRecords();

                foreach (PendingInstallationViolations rows in installTable)
                {
                    Debug.WriteLine(rows.VendorPatchName + " SME:" + rows.SME + " days left:" + rows.DaysTilViolation);

                    string emailBody = "NOTIFICATION:" + "<br/>" + "<br/>" +
                                        "The following patch is nearing the deadline..." + "<br/>" +
                                         "Patch: " + rows.VendorPatchName + "<br/>" +
                                        "Business Unit: " + rows.BUName + "<br/>" +
                                        "Platform: " + rows.PlatformName + "<br/>" +
                                        "Deadline: " + rows.DueDate;
                    if ((DateTime.Now - rows.CreatedOn).Days < 1)
                    {
                        emailSubject = "Assessment Complete: " + rows.VendorPatchName;
                        try
                        {
                            SendEmail(rows.SME, emailSubject, emailBody, "openSPM@tva.gov", "openSPM");
                            NoticeLog log = new NoticeLog();
                            log.CreatedOn = DateTime.UtcNow;
                            log.SentOn = DateTime.UtcNow;
                            log.PatchID = rows.ID;
                            log.NoticeMethodKey = 1;
                            log.NoticeLevelKey = 1;
                            log.Text = emailBody;
                            log.ToUsers = userEmail;
                            logs.AddNewRecord(log);

                        }
                        catch (Exception ex)
                        {
                            LogException(ex);
                        }

                    }

                    else if (rows.DaysTilViolation <= warning && rows.DaysTilViolation > alarm)
                    {
                        emailSubject = "Warning: " + rows.VendorPatchName + " approaching Deadline";
                        try
                        {
                            SendEmail(rows.SME, emailSubject, emailBody, "openSPM@tva.gov", "openSPM");
                            NoticeLog log = new NoticeLog();
                            log.CreatedOn = DateTime.UtcNow;
                            log.SentOn = DateTime.UtcNow;
                            log.PatchID = rows.ID;
                            log.NoticeMethodKey = 1;
                            log.NoticeLevelKey = 2;
                            log.Text = emailBody;
                            log.ToUsers = userEmail;
                            logs.AddNewRecord(log);


                        }
                        catch (Exception ex)
                        {
                            LogException(ex);
                        }
                    }
                    else if (rows.DaysTilViolation <= alarm && rows.DaysTilViolation > criticalAlarm)
                    {
                        emailSubject = "Alarm: " + rows.VendorPatchName + " approaching Deadline";
                        try
                        {
                            SendEmail(rows.SME, emailSubject, emailBody, "openSPM@tva.gov", "openSPM");
                            NoticeLog log = new NoticeLog();
                            log.CreatedOn = DateTime.UtcNow;
                            log.SentOn = DateTime.UtcNow;
                            log.PatchID = rows.ID;
                            log.NoticeMethodKey = 1;
                            log.NoticeLevelKey = 3;
                            log.Text = emailBody;
                            log.ToUsers = userEmail;
                            logs.AddNewRecord(log);


                        }
                        catch (Exception ex)
                        {
                            LogException(ex);
                        }
                    }
                    else if (rows.DaysTilViolation <= criticalAlarm && rows.DaysTilViolation > violation)
                    {
                        emailSubject = "Critical Alarm: " + rows.VendorPatchName + " approaching Deadline";
                        try
                        {
                            SendEmail(rows.SME, emailSubject, emailBody, "openSPM@tva.gov", "openSPM");
                            NoticeLog log = new NoticeLog();
                            log.CreatedOn = DateTime.UtcNow;
                            log.SentOn = DateTime.UtcNow;
                            log.PatchID = rows.ID;
                            log.NoticeMethodKey = 1;
                            log.NoticeLevelKey = 4;
                            log.Text = emailBody;
                            log.ToUsers = userEmail;
                            logs.AddNewRecord(log);


                        }
                        catch (Exception ex)
                        {
                            LogException(ex);
                        }
                    }
                    else if (rows.DaysTilViolation <= violation)
                    {
                        emailSubject = "Violation: " + rows.VendorPatchName + " passed Deadline";
                        try
                        {
                            SendEmail(rows.SME, emailSubject, emailBody, "openSPM@tva.gov", "openSPM");
                            NoticeLog log = new NoticeLog();
                            log.CreatedOn = DateTime.UtcNow;
                            log.SentOn = DateTime.UtcNow;
                            log.PatchID = rows.ID;
                            log.NoticeMethodKey = 1;
                            log.NoticeLevelKey = 5;
                            log.Text = emailBody;
                            log.ToUsers = userEmail;
                            logs.AddNewRecord(log);


                        }
                        catch (Exception ex)
                        {
                            LogException(ex);
                        }
                    }

                }

                // Handle daily e-mails
                if (DateTime.UtcNow.Hour == 0 && DateTime.UtcNow.Minute == 0)
                {
                    // ...     Guid

                    try
                    {
                        SendEmail("userID", "MySubject", "<hr><b>My E-Mail</b>", "openSPM@tva.gov", "openSPM");
                    }
                    catch (Exception ex)
                    {
                        LogException(ex);
                    }
                }
            }
        }

        private void ProcessMiPlanEmails()
        {
            using (AdoDataConnection connection = new AdoDataConnection("miPlan"))
            {
                TableOperations<MitigationPlanActionsDue> mpad = new TableOperations<MitigationPlanActionsDue>(connection);
                int groupID = connection.ExecuteScalar<int?>("SELECT ID FROM ValueListGroup WHERE Name = 'alarmLimits'") ?? 0;
                TableOperations<ValueList> valueList = new TableOperations<ValueList>(connection);
                TableOperations<NoticeLog> logs = new TableOperations<NoticeLog>(connection);
                int due, pastDue;
                ValueList[] alarms = valueList.QueryRecords("[Key]", restriction: new RecordRestriction("GroupID = {0}", groupID)).ToArray();
                if (groupID == 0)
                {
                    pastDue = 0;
                    due = 7;

                }
                else
                {
                    pastDue = alarms[0].Value;
                    due = alarms[1].Value;
                }

                IEnumerable<MitigationPlanActionsDue> table = mpad.QueryRecords();
                string emailSubject = "";

                foreach (MitigationPlanActionsDue row in table)
                {
                    Debug.WriteLine(row.Title + " SME:" + row.UserAccountID + " days left:" + row.DaysLeft);

                    string emailBody = "NOTIFICATION: <br/>" +
                                        "The following plan has actions nearing the deadline...<br/>" +
                                         "Plan: " + row.Title + "<br/>" +
                                         "Business Unit: " + row.Name + "<br/>" +
                                         "Deadline: " + row.ScheduledEndDate;

                    if ((DateTime.Now - row.CreatedOn).Days < 1)
                    {
                        emailSubject = "New plan submitted: " + row.Title;
                        try
                        {
                            SendEmail(row.UserAccountID, emailSubject, emailBody, "MiPlan@tva.gov", "miPlan");
                            NoticeLog log = new NoticeLog();
                            log.CreatedOn = DateTime.UtcNow;
                            log.SentOn = DateTime.UtcNow;
                            log.PatchID = row.ID;
                            log.NoticeMethodKey = 1;
                            log.NoticeLevelKey = 1;
                            log.Text = emailBody;
                            log.ToUsers = userEmail;
                            logs.AddNewRecord(log);

                        }
                        catch (Exception ex)
                        {
                            LogException(ex);
                        }

                    }

                    else if (row.DaysLeft <= due && row.DaysLeft >= pastDue)
                    {
                        emailSubject = "Due: " + row.Title + " approaching Deadline";
                        try
                        {
                            SendEmail(row.UserAccountID, emailSubject, emailBody, "MiPlan@tva.gov", "miPlan");
                            NoticeLog log = new NoticeLog();
                            log.CreatedOn = DateTime.UtcNow;
                            log.SentOn = DateTime.UtcNow;
                            log.PatchID = row.ID;
                            log.NoticeMethodKey = 1;
                            log.NoticeLevelKey = 2;
                            log.Text = emailBody;
                            log.ToUsers = userEmail;
                            logs.AddNewRecord(log);

                        }
                        catch (Exception ex)
                        {
                            LogException(ex);
                        }
                    }
                    else if (row.DaysLeft <= pastDue)
                    {
                        emailSubject = "Past Due: " + row.Title + " approaching Deadline";
                        try
                        {
                            SendEmail(row.UserAccountID, emailSubject, emailBody, "MiPlan@tva.gov", "miPlan");
                            NoticeLog log = new NoticeLog();
                            log.CreatedOn = DateTime.UtcNow;
                            log.SentOn = DateTime.UtcNow;
                            log.PatchID = row.ID;
                            log.NoticeMethodKey = 1;
                            log.NoticeLevelKey = 3;
                            log.Text = emailBody;
                            log.ToUsers = userEmail;
                            logs.AddNewRecord(log);

                        }
                        catch (Exception ex)
                        {
                            LogException(ex);
                        }
                    }

                }

            }
        }

        private void HeartbeatProcess(string processName, object[] processArguments)
        {
            const string RequestCommand = "Health";
            ClientRequestHandler requestHandler = m_serviceHelper.FindClientRequestHandler(RequestCommand);
            requestHandler?.HandlerMethod(ClientHelper.PretendRequest(RequestCommand));
        }

        private void ServiceHelper_ServiceStarting(object sender, EventArgs<string[]> e)
        {
            // Define a run-time log
            m_runTimeLog = new RunTimeLog();
            m_runTimeLog.FileName = "RunTimeLog.txt";
            m_runTimeLog.ProcessException += ProcessExceptionHandler;
            m_runTimeLog.Initialize();

            // Create a handler for unobserved task exceptions
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            // Make sure default service settings exist
            ConfigurationFile configFile = ConfigurationFile.Current;

            // System settings
            CategorizedSettingsElementCollection systemSettings = configFile.Settings["systemSettings"];
            systemSettings.Add("ConnectionString", "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=IaonHost.mdb", "Configuration database connection string");
            systemSettings.Add("DataProviderString", "AssemblyName={System.Data, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089};ConnectionType=System.Data.OleDb.OleDbConnection;AdapterType=System.Data.OleDb.OleDbDataAdapter", "Configuration database ADO.NET data provider assembly type creation string");
            systemSettings.Add("FromEmailAddress", "from@address.com", "Default from address for e-mails");
            systemSettings.Add("SmtpServer", "localhost", "DNS or IP address of SMTP server for relaying e-mails");
            systemSettings.Add("SmtpUserName", "", "User name for SMTP server, if any");
            systemSettings.Add("SmtpPassword", "", "Password for SMTP server, if any");

            m_defaultFromEmailAddress = systemSettings["FromEmailAddress"].Value;
            m_smtpServer = systemSettings["SmtpServer"].Value;
            m_smtpUserName = systemSettings["SmtpUserName"].Value;
            m_smtpPassword = systemSettings["SmtpPassword"].Value;
        }

        private void ServiceHelper_ServiceStarted(object sender, EventArgs e)
        {
            // Define a line of asterisks for emphasis
            string stars = new string('*', 79);

            // Get current process memory usage
            long processMemory = Common.GetProcessMemory();

            // Log startup information
            m_serviceHelper.UpdateStatus(
                UpdateType.Information,
                "\r\n\r\n{0}\r\n\r\n" +
                "     System Time: {1} UTC\r\n\r\n" +
                "    Current Path: {2}\r\n\r\n" +
                "    Machine Name: {3}\r\n\r\n" +
                "      OS Version: {4}\r\n\r\n" +
                "    Product Name: {5}\r\n\r\n" +
                "  Working Memory: {6}\r\n\r\n" +
                "  Execution Mode: {7}-bit\r\n\r\n" +
                "      Processors: {8}\r\n\r\n" +
                "  GC Server Mode: {9}\r\n\r\n" +
                " GC Latency Mode: {10}\r\n\r\n" +
                " Process Account: {11}\\{12}\r\n\r\n" +
                "{13}\r\n",
                stars,
                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                FilePath.TrimFileName(FilePath.RemovePathSuffix(FilePath.GetAbsolutePath("")), 61),
                Environment.MachineName,
                Environment.OSVersion.VersionString,
                Common.GetOSProductName(),
                processMemory > 0 ? SI2.ToScaledString(processMemory, 4, "B", SI2.IECSymbols) : "Undetermined",
                IntPtr.Size * 8,
                Environment.ProcessorCount,
                GCSettings.IsServerGC,
                GCSettings.LatencyMode,
                Environment.UserDomainName,
                Environment.UserName,
                stars);

            // Add run-time log as a service component
            m_serviceHelper.ServiceComponents.Add(m_runTimeLog);

            // Add service commands
            m_serviceHelper.ClientRequestHandlers.Add(new ClientRequestHandler("SendEmail", "Sends an email to a recipient", SendEmailHandler));

            m_serviceHelper.AddScheduledProcess(PrimaryProcess, "PrimaryProcess", "* * * * *");
            m_serviceHelper.AddScheduledProcess(HeartbeatProcess, "HeartbeatProcess", "* * * * *");
        }

        private void ServiceHelper_ServiceStopping(object sender, EventArgs e)
        {
            // Dispose of run-time log
            if ((object)m_runTimeLog != null)
            {
                m_serviceHelper.ServiceComponents.Remove(m_runTimeLog);
                m_runTimeLog.ProcessException -= ProcessExceptionHandler;
                m_runTimeLog.Dispose();
                m_runTimeLog = null;
            }

            m_serviceHelper.ServiceStarting -= ServiceHelper_ServiceStarting;
            m_serviceHelper.ServiceStarted -= ServiceHelper_ServiceStarted;
            m_serviceHelper.ServiceStopping -= ServiceHelper_ServiceStopping;

            if ((object)m_serviceHelper.StatusLog != null)
            {
                m_serviceHelper.StatusLog.Flush();
                m_serviceHelper.StatusLog.LogException -= LogExceptionHandler;
            }

            if ((object)m_serviceHelper.ErrorLogger != null && (object)m_serviceHelper.ErrorLogger.ErrorLog != null)
            {
                m_serviceHelper.ErrorLogger.ErrorLog.Flush();
                m_serviceHelper.ErrorLogger.ErrorLog.LogException -= LogExceptionHandler;
            }

            // Detach from handler for unobserved task exceptions
            TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;

        }

        /// <summary>
        /// Displays a broadcast message to all subscribed clients.
        /// </summary>
        /// <param name="status">Status message to send to all clients.</param>
        /// <param name="type"><see cref="UpdateType"/> of message to send.</param>
        private void DisplayStatusMessage(string status, UpdateType type)
        {
            try
            {
                status = status.Replace("{", "{{").Replace("}", "}}");
                m_serviceHelper.UpdateStatus(type, $"{status}\r\n\r\n");
            }
            catch (Exception ex)
            {
                LogException(ex);
                m_serviceHelper.UpdateStatus(UpdateType.Alarm, "Failed to update client status \"" + status.ToNonNullString() + "\" due to an exception: " + ex.Message + "\r\n\r\n");
            }
        }

        /// <summary>
        /// Sends an actionable response to client.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        /// <param name="success">Flag that determines if this response to client request was a success.</param>
        protected virtual void SendResponse(ClientRequestInfo requestInfo, bool success)
        {
            SendResponseWithAttachment(requestInfo, success, null, null);
        }

        /// <summary>
        /// Sends an actionable response to client with a formatted message.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        /// <param name="success">Flag that determines if this response to client request was a success.</param>
        /// <param name="status">Formatted status message to send with response.</param>
        /// <param name="args">Arguments of the formatted status message.</param>
        protected virtual void SendResponse(ClientRequestInfo requestInfo, bool success, string status, params object[] args)
        {
            SendResponseWithAttachment(requestInfo, success, null, status, args);
        }

        /// <summary>
        /// Sends an actionable response to client with a formatted message and attachment.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        /// <param name="success">Flag that determines if this response to client request was a success.</param>
        /// <param name="attachment">Attachment to send with response.</param>
        /// <param name="status">Formatted status message to send with response.</param>
        /// <param name="args">Arguments of the formatted status message.</param>
        protected virtual void SendResponseWithAttachment(ClientRequestInfo requestInfo, bool success, object attachment, string status, params object[] args)
        {
            try
            {
                // Send actionable response
                m_serviceHelper.SendActionableResponse(requestInfo, success, attachment, status, args);

                // Log details of client request as well as response
                if (m_serviceHelper.LogStatusUpdates && m_serviceHelper.StatusLog.IsOpen)
                {
                    string responseType = requestInfo.Request.Command + (success ? ":Success" : ":Failure");
                    string arguments = requestInfo.Request.Arguments.ToString();
                    string message = responseType + (string.IsNullOrWhiteSpace(arguments) ? "" : "(" + arguments + ")");

                    if (status != null)
                    {
                        if (args.Length == 0)
                            message += " - " + status;
                        else
                            message += " - " + string.Format(status, args);
                    }

                    m_serviceHelper.StatusLog.WriteTimestampedLine(message);
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                m_serviceHelper.UpdateStatus(UpdateType.Alarm, "Failed to send client response due to an exception: " + ex.Message + "\r\n\r\n");
            }
        }

        /// <summary>
        /// Displays a response message to client requester.
        /// </summary>
        /// <param name="requestInfo"><see cref="ClientRequestInfo"/> instance containing the client request.</param>
        /// <param name="status">Formatted status message to send to client.</param>
        /// <param name="args">Arguments of the formatted status message.</param>
        private void DisplayResponseMessage(ClientRequestInfo requestInfo, string status, params object[] args)
        {
            try
            {
                m_serviceHelper.UpdateStatus(requestInfo.Sender.ClientID, UpdateType.Information, $"{status}\r\n\r\n", args);
            }
            catch (Exception ex)
            {
                LogException(ex);
                m_serviceHelper.UpdateStatus(UpdateType.Alarm, "Failed to update client status \"" + status.ToNonNullString() + "\" due to an exception: " + ex.Message + "\r\n\r\n");
            }
        }

        /// <summary>
        /// Event handler for processing exceptions encountered while writing entries to a log file.
        /// </summary>
        /// <param name="sender">Event source of the exception.</param>
        /// <param name="e">Event arguments containing the exception to report.</param>
        private void LogExceptionHandler(object sender, EventArgs<Exception> e)
        {
            DisplayStatusMessage("Log file exception: " + e.Argument.Message, UpdateType.Alarm);
        }

        /// <summary>
        /// Event handler for processing reported exceptions.
        /// </summary>
        /// <param name="sender">Event source of the exception.</param>
        /// <param name="e">Event arguments containing the exception to report.</param>
        private void ProcessExceptionHandler(object sender, EventArgs<Exception> e)
        {
            LogException(e.Argument);
        }

        /// <summary>
        /// Logs an exception to the service helper <see cref="GSF.ErrorManagement.ErrorLogger"/>.
        /// </summary>
        /// <param name="ex"><see cref="Exception"/> to log.</param>
        private void LogException(Exception ex)
        {
            m_serviceHelper.LogException(ex);
        }

        // Handle task scheduler exceptions
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            foreach (Exception ex in e.Exception.Flatten().InnerExceptions)
            {
                LogException(ex);
            }

            e.SetObserved();
        }

        private void SendEmailHandler(ClientRequestInfo requestInfo)
        {
            if (requestInfo.Request.Arguments.ContainsHelpRequest)
            {
                StringBuilder helpMessage = new StringBuilder();

                helpMessage.Append("Sends an e-mail to a recipient.");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   Usage:");
                helpMessage.AppendLine();
                helpMessage.Append("       SendEmail UserID \"Subject\" \"Body\" [FromEmail] [SettingsCategory]");
                helpMessage.AppendLine();
                helpMessage.AppendLine();
                helpMessage.Append("   UserID:".PadRight(20));
                helpMessage.Append("Guid based user ID for e-mail recipient");
                helpMessage.AppendLine();
                helpMessage.Append("   Subject:".PadRight(20));
                helpMessage.Append("Subject of e-mail to send");
                helpMessage.AppendLine();
                helpMessage.Append("   Body:".PadRight(20));
                helpMessage.Append("Body of e-mail to send (assumed HTML based)");
                helpMessage.AppendLine();
                helpMessage.Append("   FromEmail:".PadRight(20));
                helpMessage.Append("Optional from e-mail address");
                helpMessage.AppendLine();
                helpMessage.Append("   SettingsCategory:".PadRight(20));
                helpMessage.Append("Optional settings category for e-mail lookup");
                helpMessage.AppendLine();
                helpMessage.Append("   Options:");
                helpMessage.AppendLine();
                helpMessage.Append("       -?".PadRight(20));
                helpMessage.Append("Displays this help message");
                helpMessage.AppendLine();

                DisplayResponseMessage(requestInfo, helpMessage.ToString());
            }
            else
            {
                if (requestInfo.Request.Arguments.Count != 3)
                {
                    SendResponse(requestInfo, false, "Expected three arguments for SendEmail method, received {0}", requestInfo.Request.Arguments.Count);
                    return;
                }

                try
                {
                    string userID = requestInfo.Request.Arguments["OrderedArg1"];
                    string subject = requestInfo.Request.Arguments["OrderedArg2"];
                    string body = requestInfo.Request.Arguments["OrderedArg3"];
                    string fromEmail = requestInfo.Request.Arguments.Exists("OrderedArg4") ? requestInfo.Request.Arguments["OrderedArg4"] : m_defaultFromEmailAddress;

                    SendEmail(userID, subject, body, fromEmail);
                }
                catch (Exception ex)
                {
                    SendResponse(requestInfo, false, "Unable to send e-mail due to exception: {0}", ex.Message);
                }
            }
        }

        private void SendEmail(string userID, string subject, string body, string fromEmail = null, string settingsCategory = "systemSettings")
        {
            SendEmail(Guid.Parse(userID), subject, body, fromEmail, settingsCategory);
        }

        private void SendEmail(Guid userID, string subject, string body, string fromEmail = null, string settingsCategory = "systemSettings")
        {
            SendEmail(new[] {userID}, subject, body, fromEmail, settingsCategory);
        }

        private void SendEmail(IEnumerable<Guid> userIDs, string subject, string body, string fromEmail = null, string settingsCategory = "systemSettings")
        {
            if (!string.IsNullOrWhiteSpace(m_smtpUserName) && !string.IsNullOrWhiteSpace(m_smtpPassword))
                Mail.Send(fromEmail ?? m_defaultFromEmailAddress, string.Join(", ", userIDs.Select(id => LookupActiveDirectoryEmail(id, settingsCategory))), subject, body, true, m_smtpServer, m_smtpUserName, m_smtpPassword);
            else
                Mail.Send(fromEmail ?? m_defaultFromEmailAddress, string.Join(", ", userIDs.Select(id => LookupActiveDirectoryEmail(id, settingsCategory))), subject, body, true, m_smtpServer);
        }

        private string LookupActiveDirectoryEmail(Guid userID, string settingsCategory = "systemSettings")
        {
            return m_emailAddressCache.GetOrAdd(userID, id =>
            {
                using (AdoDataConnection connection = new AdoDataConnection(settingsCategory))
                {
                    TableOperations<UserAccount> userAccountOperations = new TableOperations<UserAccount>(connection, LogException);
                    UserAccount userAccount = userAccountOperations.LoadRecord(id);
                    string userAccountName = UserInfo.SIDToAccountName(userAccount.Name);

                    using (UserInfo userInfo = new UserInfo(userAccountName))
                    {
                        Debug.WriteLine($"User = {userAccountName} - e-mail = {userInfo.Email}");
                        userEmail = userInfo.Email;
                        return userInfo.Email;
                    }
                }
            });
        }

        #endregion
    }
}
