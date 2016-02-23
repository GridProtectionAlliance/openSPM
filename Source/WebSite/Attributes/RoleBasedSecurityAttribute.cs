//******************************************************************************************************
//  RoleBasedSecurityAttribute.cs - Gbtc
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
//  02/23/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Security;
using System.Threading;
using System.Web.Mvc;
using GSF.Security;

namespace openSPM.Attributes
{
    /// <summary>
    /// Defines an MVC filter attribute to handle the GSF role based security model.
    /// </summary>
    public class RoleBasedSecurityAttribute : FilterAttribute, IAuthorizationFilter, IExceptionFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            // Initialize the security principal from caller's windows identity if uninitialized.
            if (SecurityProviderCache.CurrentProvider == null)
                SecurityProviderCache.CurrentProvider = SecurityProviderUtility.CreateProvider(string.Empty);

            // Setup the principal
            filterContext.HttpContext.User = Thread.CurrentPrincipal;

            // Verify that the current thread principal has been authenticated.
            if (!Thread.CurrentPrincipal.Identity.IsAuthenticated)
                throw new SecurityException($"Authentication failed for user '{Thread.CurrentPrincipal.Identity.Name}': {SecurityProviderCache.CurrentProvider.AuthenticationFailureReason}");
        }

        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext.Exception is SecurityException)
            {
                filterContext.Result = new ViewResult
                {
                    ViewName = "SecurityError",
                    ViewData = new ViewDataDictionary
                    {
                        ["Exception"] = filterContext.Exception
                    }
                };
                filterContext.ExceptionHandled = true;
            }
        }
    }
}
