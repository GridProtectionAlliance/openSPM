//******************************************************************************************************
//  HomeController.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  02/17/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using System.Text;
using GSF;
using GSF.Data;
using GSF.Data.Model;
using GSF.Web;
using GSF.Web.Model;
using GSF.Web.Security;
using openSPM.Model;

namespace openSPM.Controllers
{
    /// <summary>
    /// Represents a MVC controller for the site's main pages.
    /// </summary>
    [AuthorizeControllerRole]
    public class MainController : Controller
    {
        #region [ Members ]

        // Fields
        private readonly DataContext m_dataContext;
        private readonly AppModel m_appModel;
        private bool m_disposed;

    

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="MainController"/>.
        /// </summary>
        public MainController()
        {
            // Establish data context for the view
            m_dataContext = new DataContext(exceptionHandler: MvcApplication.LogException);
            ViewData.Add("DataContext", m_dataContext);

            // Set default model for pages used by layout
            m_appModel = new AppModel(m_dataContext);
            ViewData.Model = m_appModel;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="MainController"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    if (disposing)
                        m_dataContext?.Dispose();
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                    base.Dispose(disposing);    // Call base class Dispose().
                }
            }
        }

        public ActionResult Home()
        {
            m_appModel.ConfigureView(Url.RequestContext, "Home", ViewBag);
            DateTime today = DateTime.Today.AddDays(1).AddMinutes(-1);
            DateTime yesterday = DateTime.Today.AddDays(1).AddMinutes(-1).AddDays(-1);
            DateTime lastWeek = DateTime.Today.AddDays(-7);
            DateTime lastTwoWeeks = DateTime.Today.AddDays(-14);
            DateTime begYear = new DateTime(DateTime.Today.Year, 1, 1);
            Guid userID = new DataHub().GetCurrentUserID();

            ViewBag.todaysTotalPatchCount = m_dataContext.Table<Patch>().QueryRecordCount(new RecordRestriction("DATEDIFF(day, {0}, VendorReleaseDate) = 0", today));
            ViewBag.lastWeeksTotalPatchCount = m_dataContext.Table<Patch>().QueryRecordCount(new RecordRestriction("DATEDIFF(day, {0}, VendorReleaseDate) < 7", today));
            ViewBag.lastTwoWeeksTotalPatchCount = m_dataContext.Table<Patch>().QueryRecordCount(new RecordRestriction("DATEDIFF(day, {0}, VendorReleaseDate) < 14", today));

            ViewBag.todaysWaitingAssessmentPatchCount = m_dataContext.Table<Assessment>().QueryRecordCount(new RecordRestriction("AssessmentResultKey < 3 AND DATEDIFF(day, {0}, CreatedOn) = 0", today));
            ViewBag.lastWeeksWaitingAssessmentPatchCount = m_dataContext.Table<Assessment>().QueryRecordCount(new RecordRestriction("AssessmentResultKey < 3 AND DATEDIFF(day, {0}, CreatedOn) < 7", today));
            ViewBag.lastTwoWeeksWaitingAssessmentPatchCount = m_dataContext.Table<Assessment>().QueryRecordCount(new RecordRestriction("AssessmentResultKey < 3 AND DATEDIFF(day, {0}, CreatedOn) < 14", today));

            ViewBag.todaysRequiringPatchCount = m_dataContext.Table<Assessment>().QueryRecordCount(new RecordRestriction("AssessmentResultKey = 1 AND DATEDIFF(day, {0}, CreatedOn) = 0", today));
            ViewBag.lastWeeksRequiringPatchCount = m_dataContext.Table<Assessment>().QueryRecordCount(new RecordRestriction("AssessmentResultKey = 1 AND DATEDIFF(day, {0}, CreatedOn) < 7", today));
            ViewBag.lastTwoWeeksRequiringPatchCount = m_dataContext.Table<Assessment>().QueryRecordCount(new RecordRestriction("AssessmentResultKey = 1 AND DATEDIFF(day, {0}, CreatedOn) < 14", today));

            ViewBag.todaysRequiringMitigationCount = m_dataContext.Table<Assessment>().QueryRecordCount(new RecordRestriction("AssessmentResultKey = 2 AND DATEDIFF(day, {0}, CreatedOn) = 0", today));
            ViewBag.lastWeeksRequiringMitigationCount = m_dataContext.Table<Assessment>().QueryRecordCount(new RecordRestriction("AssessmentResultKey = 2 AND DATEDIFF(day, {0}, CreatedOn) < 7", today));
            ViewBag.lastTwoWeeksRequiringMitigationCount = m_dataContext.Table<Assessment>().QueryRecordCount(new RecordRestriction("AssessmentResultKey =2 AND DATEDIFF(day, {0}, CreatedOn) < 14", today));

            ViewBag.YTDPatchesAssesed = m_dataContext.Table<Assessment>().QueryRecordCount(new RecordRestriction("CreatedOn BETWEEN {0} AND {1} ",begYear, today));
            ViewBag.YTDPatchesNotApplicaple = m_dataContext.Table<Assessment>().QueryRecordCount(new RecordRestriction("AssessmentResultKey = 4 AND CreatedOn BETWEEN {0} AND {1} ", begYear, today));
            ViewBag.YTDPatchesApplied = m_dataContext.Table<Install>().QueryRecordCount(new RecordRestriction("CreatedOn BETWEEN {0} AND {1} ", begYear, today));
            ViewBag.YTDPatchesMitigationPlans = m_dataContext.Table<MitigationPlan>().QueryRecordCount(new RecordRestriction("isDeleted = 0 AND CreatedOn BETWEEN {0} AND {1} ", begYear, today));

            ViewBag.YTDCriticalAlarmsDiscovery = m_dataContext.Table<DiscoveryResult>().QueryRecordCount(new RecordRestriction("DATEDIFF(day, ReviewDate, CreatedOn) >= 21"));
            ViewBag.YTDCriticalAlarmsAssessment = m_dataContext.Table<Assessment>().QueryRecordCount(new RecordRestriction("DATEDIFF(day, UpdatedOn, CreatedOn) >= 21"));
            ViewBag.YTDCriticalAlarmsInstallation = m_dataContext.Table<Install>().QueryRecordCount(new RecordRestriction("DATEDIFF(day, CompletedOn, CreatedOn) >= 21"));
            ViewBag.YTDCriticalAlarmsMitigationPlans = m_dataContext.Table<MitigationPlan>().QueryRecordCount(new RecordRestriction("IsDeleted = 0 AND DATEDIFF(day, ApprovedOn, CreatedOn ) >= 21"));

            ViewBag.YTDViolationsDiscovery = m_dataContext.Table<DiscoveryResult>().QueryRecordCount(new RecordRestriction("DATEDIFF(day, ReviewDate, CreatedOn) >= 35"));
            ViewBag.YTDViolationsAssessment = m_dataContext.Table<Assessment>().QueryRecordCount(new RecordRestriction("DATEDIFF(day, UpdatedOn, CreatedOn) >= 35"));
            ViewBag.YTDViolationsInstallation = m_dataContext.Table<Install>().QueryRecordCount(new RecordRestriction("DATEDIFF(day, CompletedOn, CreatedOn) >= 35"));
            ViewBag.YTDViolationsMitigationPlans = m_dataContext.Table<MitigationPlan>().QueryRecordCount(new RecordRestriction("IsDeleted = 0 AND DATEDIFF(day, ApprovedOn, CreatedOn) >= 35"));

            m_appModel.ConfigureView(Url.RequestContext, "Home", ViewBag);            
            return View();
        }

        public ActionResult Patches()
        {
            m_appModel.ConfigureView<Patch>(Url.RequestContext, "Patches", ViewBag);
            return View();
        }

        public ActionResult DiscoverPatches()
        {
            m_appModel.ConfigureView<LatestVendorDiscoveryResult>(Url.RequestContext, "Check", ViewBag);
            return View();
        }

        public ActionResult Help()
        {
            m_appModel.ConfigureView(Url.RequestContext, "Help", ViewBag);
            return View();
        }

        public ActionResult Contact()
        {
            m_appModel.ConfigureView(Url.RequestContext, "Contact", ViewBag);
            ViewBag.Message = "Contacting the Grid Protection Alliance";
            return View();
        }

        public ActionResult DisplayPDF()
        {
            // Using route ID, i.e., /Main/DisplayPDF/{id}, as page name of PDF load
            string routeID = Url.RequestContext.RouteData.Values["id"] as string ?? "UndefinedPageName";
            m_appModel.ConfigureView(Url.RequestContext, routeID, ViewBag);

            return View();
        }

        public ActionResult PageTemplate1()
        {
            m_appModel.ConfigureView(Url.RequestContext, "PageTemplate1", ViewBag);
            return View();
        }

        public ActionResult Install()
        {
            m_appModel.ConfigureView<Install>(Url.RequestContext, "Install", ViewBag);
            return View();
        }

        public ActionResult InstallDocument()
        {
            m_appModel.ConfigureView<InstallDocument>(Url.RequestContext, "InstallDocument", ViewBag);
            return View();
        }

        public ActionResult Assess()
        {
            m_appModel.ConfigureView(Url.RequestContext, "Assess", ViewBag);
            return View();
        }

        public ActionResult History()
        {
            m_appModel.ConfigureView(Url.RequestContext, "History", ViewBag);
            return View();
        }

        public ActionResult Plan()
        {
            m_appModel.ConfigureView(Url.RequestContext, "Plan", ViewBag);
            return View();
        }

        public ActionResult MitigationPlan()
        {
            m_appModel.ConfigureView<MitigationPlan>(Url.RequestContext, "MitigationPlan", ViewBag);
            return View();
        }

        public ActionResult Notification()
        {
            m_appModel.ConfigureView(Url.RequestContext, "Notification", ViewBag);
            return View();
        }


        public ActionResult Done()
        {
            m_appModel.ConfigureView(Url.RequestContext, "Done", ViewBag);
            return View();
        }

        public ActionResult Assessments()
        {
            m_appModel.ConfigureView<Assessment>(Url.RequestContext, "Assessments", ViewBag);
            ViewBag.psag = m_dataContext.Table<PatchStatusAssessmentDetail>().QueryRecords();
            return View();
        }

        

       

        #endregion
    }
}