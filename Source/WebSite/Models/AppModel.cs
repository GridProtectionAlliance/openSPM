//******************************************************************************************************
//  AppModel.cs - Gbtc
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
using System.Linq;
using GSF.Security;
using openSPM.Attributes;

namespace openSPM.Models
{
    /// <summary>
    /// Defines a base application model with convenient global settings and functions.
    /// </summary>
    /// <remarks>
    /// Custom view models should inherit from AppModel because the "Global" property is used by _Layout.cshtml.
    /// </remarks>
    public class AppModel
    {
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="AppModel"/>.
        /// </summary>
        public AppModel()
        {
            Global = MvcApplication.DefaultModel != null ? MvcApplication.DefaultModel.Global : new GlobalSettings();
        }

        /// <summary>
        /// Creates a new <see cref="AppModel"/> with the specified <paramref name="dataContext"/>.
        /// </summary>
        /// <param name="dataContext">Data context to provide to model.</param>
        public AppModel(DataContext dataContext) : this()
        {
            DataContext = dataContext;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets global settings for application.
        /// </summary>
        public GlobalSettings Global
        {
            get;
        }

        /// <summary>
        /// Gets reference to user specific security provider instance.
        /// </summary>
        public AdoSecurityProvider SecurityProvider => SecurityProviderCache.CurrentProvider as AdoSecurityProvider;

        /// <summary>
        /// Gets or sets data context for model.
        /// </summary>
        public DataContext DataContext
        {
            get;
            private set;
        }

        #endregion

        #region [ Methods ]

        //public string RenderValueListLookup<T>()

        /// <summary>
        /// Generates template based select field based on reflected modeled table field attributes with values derived from ValueList table.
        /// </summary>
        /// <typeparam name="T">Modeled table for select field.</typeparam>
        /// <param name="groupName">Value list group name as defined in ValueListGroup table.</param>
        /// <param name="fieldName">Field name for value of select field.</param>
        /// <param name="optionLabelFieldName">Field name for label of option data, defaults to "Text"</param>
        /// <param name="optionValueFieldName">Field name for ID of option data, defaults to "Key".</param>
        /// <param name="optionSortFieldName">Field name for sort order of option data, defaults to "SortOrder"</param>
        /// <param name="fieldLabel">Label name for select field, pulls from <see cref="LabelAttribute"/> if defined, otherwise defaults to <paramref name="fieldName"/>.</param>
        /// <param name="fieldID">ID to use for select field; defaults to select + <paramref name="fieldName"/>.</param>
        /// <param name="groupDataBinding">Data-bind operations to apply to outer form-group div, if any.</param>
        /// <param name="labelDataBinding">Data-bind operations to apply to label, if any.</param>
        /// <param name="customDataBinding">Extra custom data-binding operations to apply to field, if any.</param>
        /// <param name="dependencyFieldName">Defines default "enabled" subordinate data-bindings based a single boolean field, e.g., a check-box.</param>
        /// <param name="toolTip">Tool tip text to apply to field, if any.</param>
        /// <returns>Generated HTML for new text field based on modeled table field attributes.</returns>
        public string AddValueListSelectField<T>(string fieldName, string groupName, string optionLabelFieldName = "Text", string optionValueFieldName = "Key", string optionSortFieldName = "SortOrder", string fieldLabel = null, string fieldID = null, string groupDataBinding = null, string labelDataBinding = null, string customDataBinding = null, string dependencyFieldName = null, string toolTip = null) where T : class, new()
        {
            int key = DataContext.Connection.ExecuteScalar<int?>("SELECT ID FROM ValueListGroup WHERE Name={0} AND Enabled <> 0", groupName) ?? 0;

            RecordRestriction restriction = new RecordRestriction
            {
                FilterExpression = "GroupID = {0} AND Enabled <> 0 AND Hidden = 0",
                Parameters = new object[] { key }
            };

            return DataContext.AddSelectField<T, ValueList>(fieldName, optionValueFieldName, optionLabelFieldName, optionSortFieldName, fieldLabel, fieldID, groupDataBinding, labelDataBinding, customDataBinding, dependencyFieldName, toolTip, restriction);
        }

        /// <summary>
        /// Determines if user is in a specific role or list of roles (comma separated).
        /// </summary>
        /// <param name="role">Role or comma separated list of roles.</param>
        /// <returns><c>true</c> if user is in <paramref name="role"/>(s); otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Set to * for any role.
        /// </remarks>
        public bool UserIsInRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return false;

            role = role.Trim();

            string[] roles = role.Split(',').Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();

            if (roles.Length > 1)
                return UserIsInRole(roles);

            List<string> userRoles = SecurityProvider?.UserData?.Roles ?? new List<string>();

            if (role.Equals("*") && userRoles.Count > 0)
                return true;

            foreach (string userRole in userRoles)
                if (userRole.Equals(role, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        /// <summary>
        /// Determines if user is in one of the provided of roles.
        /// </summary>
        /// <param name="roles">List of role names.</param>
        /// <returns><c>true</c> if user is in one of the <paramref name="roles"/>; otherwise, <c>false</c>.</returns>
        public bool UserIsInRole(string[] roles)
        {
            return roles.Any(UserIsInRole);
        }

        /// <summary>
        /// Determines if user is in a specific group or list of groups (comma separated).
        /// </summary>
        /// <param name="group">Group or comma separated list of groups.</param>
        /// <returns><c>true</c> if user is in <paramref name="group"/>(s); otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Set to * for any group.
        /// </remarks>
        public bool UserIsInGroup(string group)
        {
            if (string.IsNullOrWhiteSpace(group))
                return false;

            group = group.Trim();

            string[] groups = group.Split(',').Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();

            if (groups.Length > 1)
                return UserIsInGroup(groups);

            List<string> userGroups = SecurityProvider?.UserData?.Groups ?? new List<string>();

            if (group.Equals("*") && userGroups.Count > 0)
                return true;

            foreach (string userGroup in userGroups)
                if (userGroup.Equals(group, StringComparison.OrdinalIgnoreCase))
                    return true;

            return false;
        }

        /// <summary>
        /// Determines if user is in one of the provided of groups.
        /// </summary>
        /// <param name="groups">List of group names.</param>
        /// <returns><c>true</c> if user is in one of the <paramref name="groups"/>; otherwise, <c>false</c>.</returns>
        public bool UserIsInGroup(string[] groups)
        {
            return groups.Any(UserIsInGroup);
        }

        /// <summary>
        /// Looks up page info based on defined page name.
        /// </summary>
        /// <param name="pageName">Page name as defined in Page table.</param>
        /// <param name="viewBag">Current view bag.</param>
        /// <remarks>
        /// This is normally called from controller before returning view action result.
        /// </remarks>
        public void LookupPageDetail(string pageName, dynamic viewBag)
        {
            int pageID = DataContext.Connection.ExecuteScalar<int?>("SELECT ID FROM Page WHERE Name={0} AND Enabled <> 0", pageName ?? "") ?? 0;
            Page page = DataContext.QueryRecord<Page>(pageID);

            viewBag.Title = page?.Title ?? (pageName == null ? "<pageName is undefined>" : $"<Page record for \"{pageName}\" does not exist>");
            viewBag.PageName = pageName;
            viewBag.PageID = pageID;
            viewBag.Page = page;
        }

        #endregion
    }
}
