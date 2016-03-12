//******************************************************************************************************
//  RazorView.cs - Gbtc
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
//  01/13/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using GSF;
using GSF.Configuration;
using GSF.IO;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;

// TODO: Move into GSF

// ReSharper disable StaticMemberInGenericType
namespace openSPM.Models
{
    public abstract class LanguageContraint
    {
        public abstract Language TargetLanguage
        {
            get;
        }
    }

    public class CSharp : LanguageContraint
    {
        public override Language TargetLanguage => Language.CSharp;
    }

    public class VisualBasic : LanguageContraint
    {
        public override Language TargetLanguage => Language.VisualBasic;
    }

    public class RazorView<T> where T : LanguageContraint, new()
    {
        #region [ Members ]

        // Fields
        private readonly DynamicViewBag m_viewBag = new DynamicViewBag();
        private Dictionary<string, string> m_parameters;

        #endregion

        #region [ Constructors ]

        public RazorView()
        {
        }

        public RazorView(string templateName, Action<Exception> exceptionHandler = null)
        {
            TemplateName = templateName;
            ExceptionHandler = exceptionHandler;
        }

        public RazorView(string templateName, object model, Type modelType, Action<Exception> exceptionHandler = null) : this(templateName, exceptionHandler)
        {
            Model = model;
            ModelType = modelType;
        }

        #endregion

        #region [ Properties ]

        public string TemplateName
        {
            get; set;
        }

        public object Model
        {
            get; set;
        }

        public Type ModelType
        {
            get; set;
        }

        public Action<Exception> ExceptionHandler
        {
            get; set;
        }

        public dynamic ViewBag => m_viewBag;

        public string this[string key] => Parameters[key];

        public Dictionary<string, string> Parameters
        {
            get
            {
                if ((object)m_parameters == null)
                {
                    HttpRequestMessage request = ViewBag.Request;
                    m_parameters = HttpUtility.ParseQueryString(request.RequestUri.Query).ToDictionary();
                }

                return m_parameters;
            }
        }

        #endregion

        #region [ Methods ]

        public string Execute()
        {
            using (DataContext dataContext = new DataContext(exceptionHandler: ExceptionHandler))
            {
                m_viewBag.AddValue("DataContext", dataContext);
                return s_engineService.RunCompile(TemplateName, ModelType, Model, m_viewBag);
            }
        }

        public string Execute(HttpRequestMessage requestMessage, dynamic postData)
        {
            using (DataContext dataContext = new DataContext(exceptionHandler: ExceptionHandler))
            {
                m_viewBag.AddValue("DataContext", dataContext);
                m_viewBag.AddValue("Request", requestMessage);

                if ((object)postData == null)
                {
                    m_viewBag.AddValue("IsPost", false);
                }
                else
                {
                    m_viewBag.AddValue("IsPost", true);
                    m_viewBag.AddValue("PostData", postData);
                }

                return s_engineService.RunCompile(TemplateName, ModelType, Model, m_viewBag);
            }
        }

        public Task ExecuteAsync(HttpRequestMessage requestMessage, dynamic postData)
        {
            return Task.Run(() => Execute(requestMessage, postData));
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly IRazorEngineService s_engineService;

        /// <summary>
        /// Defines default settings path for template files.
        /// </summary>
        public static readonly string TemplatePath;

        // Static Constructor
        static RazorView()
        {
            T languageType = new T();

            // Get configured template path
            CategorizedSettingsElementCollection systemSettings = ConfigurationFile.Current.Settings["systemSettings"];
            systemSettings.Add("TemplatePath", "~/Views/Shared/Templates/", "Path for data context based razor field templates.");

            if (Common.GetApplicationType() == ApplicationType.Web)
                TemplatePath = HostingEnvironment.MapPath(systemSettings["TemplatePath"].Value).EnsureEnd('/');
            else
                TemplatePath = FilePath.GetAbsolutePath(systemSettings["TemplatePath"].Value);
#if DEBUG
            // The watching resolve path template manager should not be used in production since
            // assemblies cannot be unloaded from an AppDomain. Every time a change to a .cshtml
            // file is picked up by the watcher it is compiled and loaded into the AppDomain and
            // the old one cannot be removed (.NET restriction), the net result is a memory leak
            InvalidatingCachingProvider cachingProvider = new InvalidatingCachingProvider(fileName => MvcApplication.LogStatusMessage($"Cache invalidated for {fileName}..."));

            s_engineService = RazorEngineService.Create(new TemplateServiceConfiguration
            {
                Language = languageType.TargetLanguage,
                CachingProvider = cachingProvider,
                TemplateManager = new WatchingResolvePathTemplateManager(new[] { TemplatePath }, cachingProvider),
                Debug = true
            });
#else
            s_engineService = RazorEngineService.Create(new TemplateServiceConfiguration
            {
                Language  = languageType.TargetLanguage,
                TemplateManager = new ResolvePathTemplateManager(new[] { TemplatePath }),
                Debug = false
            });
#endif
            Task.Run(() =>
            {
                string webRootFolder = FilePath.AddPathSuffix(TemplatePath);
                string[] razorFiles = FilePath.GetFileList($"{webRootFolder}*.{(languageType.TargetLanguage == Language.CSharp ? "cs" : "vb")}html");

                foreach (string fileName in razorFiles)
                {
                    try
                    {
                        MvcApplication.LogStatusMessage($"Pre-compiling razor template \"{fileName}\"...");
                        s_engineService.Compile(FilePath.GetFileName(fileName), typeof(AppModel));
                    }
                    catch (Exception ex)
                    {
                        MvcApplication.LogException(new InvalidOperationException($"Failed to pre-compile razor template \"{fileName}\": {ex.Message}", ex));
                    }
                }
            });
        }

        #endregion
    }
}
