//******************************************************************************************************
//  BundleConfig.cs - Gbtc
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
//  02/17/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.Generic;
using System.Web.Optimization;

namespace openSPM
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/js.bundle/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/js.bundle/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/js.bundle/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/js.bundle/bootstrap").Include(
                        "~/Scripts/bootstrap.min.js",
                        "~/Scripts/ie10-viewport-bug-workaround.js",
                        "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/js.bundle/signalR").Include(
                        "~/Scripts/jquery.signalR-2.2.0.min.js"));

            bundles.Add(new ScriptBundle("~/js.bundle/gsf.web.client").Include(
                        "~/Scripts/gsf.web.client.js"));

            bundles.Add(new ScriptBundle("~/js.bundle/site").Include(
                        "~/Scripts/Site.js"));

            bundles.Add(new ScriptBundle("~/js.bundle/pagedViewModel").Include(
                        "~/Scripts/bootstrap-toolkit.min.js",
                        "~/Scripts/knockout-3.4.0.js",
                        "~/Scripts/knockout.mapping-latest.js",
                        "~/Scripts/knockout.validation.min.js",
                        "~/Scripts/ko-reactor.min.js",
                        "~/Scripts/ko.observableDictionary.js",
                        "~/Scripts/js.cookie.js",
                        "~/Scripts/pagedViewModel.js"));

            bundles.Add(new StyleBundle("~/css.bundle/bootstrap").Include(
                        "~/Content/bootstrap-sidebar.css"));

            bundles.Add(new StyleBundle("~/css.bundle/site").Include(
                        "~/Content/Site.css"));
        }
    }
}