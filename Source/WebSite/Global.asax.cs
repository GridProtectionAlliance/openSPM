//******************************************************************************************************
//  Global.asax.cs - Gbtc
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
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using GSF;
using GSF.Configuration;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using openSPM.Models;

namespace openSPM
{
    public class MvcApplication : System.Web.HttpApplication
    {
        /// <summary>
        /// Gets the default model used for the application.
        /// </summary>
        public static readonly AppModel DefaultModel = new AppModel();

        /// <summary>
        /// Gets the list of currently connected hub clients.
        /// </summary>
        public static IHubConnectionContext<dynamic> HubClients => s_clients.Value;

        /// <summary>
        /// Gets the loaded global settings.
        /// </summary>
        public static readonly Dictionary<string, string> GlobalSettings = new Dictionary<string, string>();

        private static readonly Lazy<IHubConnectionContext<dynamic>> s_clients = new Lazy<IHubConnectionContext<dynamic>>(() => GlobalHost.ConnectionManager.GetHubContext<DataHub>().Clients);

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);


            // Make sure openSPM specific default service settings exist
            CategorizedSettingsElementCollection systemSettings = ConfigurationFile.Current.Settings["systemSettings"];

            systemSettings.Add("CompanyName", "Grid Protection Alliance", "The name of the company who owns this instance of the openMIC.");
            systemSettings.Add("CompanyAcronym", "GPA", "The acronym representing the company who owns this instance of the openMIC.");
            systemSettings.Add("DateTimeFormat", "yyyy-MM-dd HH:mm.ss.fff", "The date/time format to use when rendering timestamps.");
            systemSettings.Add("BootstrapTheme", "/Content/bootstrap.min.css", "Path to Bootstrap CSS to use for rendering styles.");

            using (DataContext dataContext = new DataContext())
            {
                // Load global web settings
                Dictionary<string, string> globalSettings = dataContext.LoadDatabaseSettings("web.global");

                foreach (KeyValuePair<string, string> item in globalSettings)
                    GlobalSettings.Add(item.Key, item.Value);
            }

            // Load default model settings
            DefaultModel.CompanyName = systemSettings["CompanyName"].Value;
            DefaultModel.CompanyAcronym = systemSettings["CompanyAcronym"].Value;
            DefaultModel.ApplicationName = "openSPM";
            DefaultModel.ApplicationDescription = "open Security Patch Management";
            DefaultModel.ApplicationKeywords = "open source, utility, software, patch, management";
            DefaultModel.DateTimeFormat = systemSettings["DateTimeFormat"].Value;
            DefaultModel.BootstrapTheme = systemSettings["BootstrapTheme"].Value;
        }

        /// <summary>
        /// Logs a status message.
        /// </summary>
        /// <param name="message">Message to log.</param>
        /// <param name="type">Type of message to log.</param>
        public static void LogStatusMessage(string message, UpdateType type = UpdateType.Information)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                string connectionID = state as string;

                if (!string.IsNullOrEmpty(connectionID))
                {
                    if (type == UpdateType.Information)
                        HubClients.Client(connectionID).sendInfoMessage(message, 3000);
                    else
                        HubClients.Client(connectionID).sendErrorMessage(message, type == UpdateType.Alarm ? -1 : 3000);
                }
#if DEBUG
                else
                {
                    Thread.Sleep(1500);
                    if (type == UpdateType.Information)
                        HubClients.All.sendInfoMessage(message, 3000);
                    else
                        HubClients.All(connectionID).sendErrorMessage(message, type == UpdateType.Alarm ? -1 : 3000);
                }
#endif
            }, DataHub.CurrentConnectionID);
        }

        /// <summary>
        /// Logs an exception.
        /// </summary>
        /// <param name="ex">Exception to log.</param>
        public static void LogException(Exception ex)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                string connectionID = state as string;

                if (!string.IsNullOrEmpty(connectionID))
                {
                    HubClients.Client(connectionID).sendErrorMessage(ex.Message, -1);
                }
#if DEBUG
                else
                {
                    Thread.Sleep(1500);
                    HubClients.All.sendErrorMessage(ex.Message, -1);
                }
#endif
            }, DataHub.CurrentConnectionID);
        }
    }
}
