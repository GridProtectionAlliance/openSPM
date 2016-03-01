//******************************************************************************************************
//  AuthorizeHubRole.cs - Gbtc
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
//  02/25/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Principal;
using System.Threading;
using GSF.Collections;
using GSF.Identity;
using GSF.Security;
using Microsoft.AspNet.SignalR;
using openSPM.Models;

namespace openSPM.Attributes
{
    /// <summary>
    /// Defines a SignalR authorization attribute to handle the GSF role based security model.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class AuthorizeHubRoleAttribute : AuthorizeAttribute
    {
        public readonly string[] AllowedRoles;

        public AuthorizeHubRoleAttribute()
        {
            AllowedRoles = new string[0];
        }

        public AuthorizeHubRoleAttribute(string allowedRoles)
        {
            Roles = allowedRoles;

            AllowedRoles = allowedRoles?.Split(',').Select(role => role.Trim()).
                Where(role => !string.IsNullOrEmpty(role)).ToArray() ?? new string[0];
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override bool UserAuthorized(IPrincipal user)
        {
            // Get current user name
            string userName = user.Identity.Name;

            // Initialize the security principal from caller's windows identity if uninitialized.
            if (SecurityProviderCache.CurrentProvider == null)
                SecurityProviderCache.CurrentProvider = SecurityProviderUtility.CreateProvider(user.Identity.Name);

            // Setup the principal
            user = Thread.CurrentPrincipal;

            // Verify that the current thread principal has been authenticated.
            if (!Thread.CurrentPrincipal.Identity.IsAuthenticated)
                throw new SecurityException($"Authentication failed for user '{userName}': {SecurityProviderCache.CurrentProvider.AuthenticationFailureReason}");

            if (AllowedRoles.Length > 0 && !AllowedRoles.Any(role => user.IsInRole(role)))
                throw new SecurityException($"Access is denied for user '{userName}' defined: minimum required roles = {AllowedRoles.ToDelimitedString(", ")}.");

            // Make sure current user ID is cached
            if (!MvcApplication.UserIDCache.ContainsKey(userName))
            {
                using (DataContext dataContext = new DataContext())
                {
                    MvcApplication.UserIDCache.TryAdd(userName, dataContext.Connection.ExecuteScalar<Guid?>("SELECT ID FROM UserAccount WHERE Name={0}", UserInfo.UserNameToSID(userName)) ?? Guid.Empty);
                }
            }

            return true;
        }
    }
}
