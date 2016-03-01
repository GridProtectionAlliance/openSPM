//******************************************************************************************************
//  DataHub.cs - Gbtc
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
//  01/14/2016 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GSF;
using GSF.Collections;
using GSF.Identity;
using GSF.Reflection;
using Microsoft.AspNet.SignalR;
using openSPM.Attributes;
using openSPM.Models;

namespace openSPM
{
    [AuthorizeHubRole]
    public class DataHub : Hub
    {
        #region [ Members ]

        // Fields
        private readonly DataContext m_dataContext;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        public DataHub()
        {
            m_dataContext = new DataContext();
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DataHub"/> object and optionally releases the managed resources.
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

        public override Task OnConnected()
        {
            // Store the current connection ID for this thread
            s_connectionID.Value = Context.ConnectionId;
            s_connectCount++;

            //MvcApplication.LogStatusMessage($"DataHub connect by {Context.User?.Identity?.Name ?? "Undefined User"} [{Context.ConnectionId}] - count = {s_connectCount}");
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            if (stopCalled)
            {
                s_connectCount--;
                //MvcApplication.LogStatusMessage($"DataHub disconnect by {Context.User?.Identity?.Name ?? "Undefined User"} [{Context.ConnectionId}] - count = {s_connectCount}");
            }

            return base.OnDisconnected(stopCalled);
        }

        #endregion

        #region [ Static ]

        // Static Properties

        /// <summary>
        /// Gets the hub connection ID for the current thread.
        /// </summary>
        public static string CurrentConnectionID => s_connectionID.Value;

        // Static Fields
        private static volatile int s_connectCount;
        private static readonly ThreadLocal<string> s_connectionID = new ThreadLocal<string>();
        private static readonly Dictionary<Type, Tuple<string, string>[]> s_recordOperations = new Dictionary<Type, Tuple<string, string>[]>();

        // Static Constructor
        static DataHub()
        {
            int recordOperations = Enum.GetValues(typeof(RecordOperation)).Length;

            // Analyze and cache data hub methods that are targeted for record operations
            foreach (MethodInfo method in typeof(DataHub).GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                AuthorizeHubRoleAttribute authorizeHubRoleAttribute;
                RecordOperationAttribute recordOperationAttribute;

                method.TryGetAttribute(out authorizeHubRoleAttribute);

                if (method.TryGetAttribute(out recordOperationAttribute))
                {
                    // Cache method name and any defined authorized roles for current record operation
                    s_recordOperations.GetOrAdd(recordOperationAttribute.ModelType,
                        type => new Tuple<string, string>[recordOperations])[(int)recordOperationAttribute.Operation] =
                        new Tuple<string, string>(method.Name, authorizeHubRoleAttribute?.AllowedRoles?.ToDelimitedString(','));
                }
            }
        }

        // Static Methods

        /// <summary>
        /// Gets record operation methods for specified modeled table.
        /// </summary>
        /// <typeparam name="T">Modeled table.</typeparam>
        /// <returns>record operation methods for specified modeled table.</returns>
        public static Tuple<string, string>[] GetRecordOperations<T>() where T : class, new()
        {
            return s_recordOperations[typeof(T)];
        }

        #endregion

        // Client-side script functionality

        #region [ Patch Table Operations ]

        [RecordOperation(typeof(Patch), RecordOperation.QueryRecordCount)]
        public int QueryPatchCount(bool showDeleted)
        {
            if (showDeleted)
                return m_dataContext.Table<Patch>().QueryRecordCount();

            return m_dataContext.Table<Patch>().QueryRecordCount(new RecordRestriction
            {
                FilterExpression = "IsDeleted = 0"
            });
        }

        [RecordOperation(typeof(Patch), RecordOperation.QueryRecords)]
        public IEnumerable<Patch> QueryPatches(bool showDeleted, string sortField, bool ascending, int page, int pageSize)
        {
            if (showDeleted)
                return m_dataContext.Table<Patch>().QueryRecords(sortField, ascending, page, pageSize);

            return m_dataContext.Table<Patch>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction
            {
                FilterExpression = "IsDeleted = 0"
            });
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(Patch), RecordOperation.DeleteRecord)]
        public void DeletePatch(int id)
        {
            // For Patches, we only "mark" a record as deleted
            m_dataContext.Connection.ExecuteNonQuery("UPDATE Patch SET IsDeleted=1 WHERE ID={0}", id);
        }

        [RecordOperation(typeof(Patch), RecordOperation.CreateNewRecord)]
        public Patch NewPatch()
        {
            return new Patch();
        }

        [AuthorizeHubRole("Administrator, Owner, PIC")]
        [RecordOperation(typeof(Patch), RecordOperation.AddNewRecord)]
        public void AddNewPatch(Patch patch)
        {
            patch.CreatedByID = GetCurrentUserID();
            patch.CreatedOn = DateTime.UtcNow;
            patch.UpdatedByID = patch.CreatedByID;
            patch.UpdatedOn = patch.CreatedOn;
            m_dataContext.Table<Patch>().AddNewRecord(patch);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC")]
        [RecordOperation(typeof(Patch), RecordOperation.UpdateRecord)]
        public void UpdatePatch(Patch patch)
        {
            patch.UpdatedByID = GetCurrentUserID();
            patch.UpdatedOn = DateTime.UtcNow;
            m_dataContext.Table<Patch>().UpdateRecord(patch);
        }

        #endregion

        #region [ Vendor Table Operations ]

        [RecordOperation(typeof(Vendor), RecordOperation.QueryRecordCount)]
        public int QueryVendorCount(bool showDeleted)
        {
            if (showDeleted)
                return m_dataContext.Table<Vendor>().QueryRecordCount();

            return m_dataContext.Table<Vendor>().QueryRecordCount(new RecordRestriction
            {
                FilterExpression = "IsDeleted = 0"
            });
        }

        [RecordOperation(typeof(Vendor), RecordOperation.QueryRecords)]
        public IEnumerable<Vendor> QueryVendors(bool showDeleted, string sortField, bool ascending, int page, int pageSize)
        {
            if (showDeleted)
                return m_dataContext.Table<Vendor>().QueryRecords(sortField, ascending, page, pageSize);

            return m_dataContext.Table<Vendor>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction
            {
                FilterExpression = "IsDeleted = 0"
            });
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(Vendor), RecordOperation.DeleteRecord)]
        public void DeleteVendor(int id)
        {
            // For Vendors, we only "mark" a record as deleted
            m_dataContext.Connection.ExecuteNonQuery("UPDATE Vendor SET IsDeleted=1 WHERE ID={0}", id);
        }

        [RecordOperation(typeof(Vendor), RecordOperation.CreateNewRecord)]
        public Vendor NewVendor()
        {
            return new Vendor();
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(Vendor), RecordOperation.AddNewRecord)]
        public void AddNewVendor(Vendor vendor)
        {
            vendor.CreatedByID = GetCurrentUserID();
            vendor.CreatedOn = DateTime.UtcNow;
            vendor.UpdatedByID = vendor.CreatedByID;
            vendor.UpdatedOn = vendor.CreatedOn;
            m_dataContext.Table<Vendor>().AddNewRecord(vendor);
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(Vendor), RecordOperation.UpdateRecord)]
        public void UpdateVendor(Vendor vendor)
        {
            vendor.UpdatedByID = GetCurrentUserID();
            vendor.UpdatedOn = DateTime.UtcNow;
            m_dataContext.Table<Vendor>().UpdateRecord(vendor);
        }

        #endregion

        #region [ Platform Table Operations ]

        [RecordOperation(typeof(Platform), RecordOperation.QueryRecordCount)]
        public int QueryPlatformCount(bool showDeleted)
        {
            if (showDeleted)
                return m_dataContext.Table<Platform>().QueryRecordCount();

            return m_dataContext.Table<Platform>().QueryRecordCount(new RecordRestriction
            {
                FilterExpression = "IsDeleted = 0"
            });
        }

        [RecordOperation(typeof(Platform), RecordOperation.QueryRecords)]
        public IEnumerable<Platform> QueryPlatforms(bool showDeleted, string sortField, bool ascending, int page, int pageSize)
        {
            if (showDeleted)
                return m_dataContext.Table<Platform>().QueryRecords(sortField, ascending, page, pageSize);

            return m_dataContext.Table<Platform>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction
            {
                FilterExpression = "IsDeleted = 0"
            });
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(Platform), RecordOperation.DeleteRecord)]
        public void DeletePlatform(int id)
        {
            // For Platforms, we only "mark" a record as deleted
            m_dataContext.Connection.ExecuteNonQuery("UPDATE Platform SET IsDeleted=1 WHERE ID={0}", id);
        }

        [RecordOperation(typeof(Platform), RecordOperation.CreateNewRecord)]
        public Platform NewPlatform()
        {
            return new Platform();
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(Platform), RecordOperation.AddNewRecord)]
        public void AddNewPlatform(Platform vendor)
        {
            vendor.CreatedByID = GetCurrentUserID();
            vendor.CreatedOn = DateTime.UtcNow;
            vendor.UpdatedByID = vendor.CreatedByID;
            vendor.UpdatedOn = vendor.CreatedOn;
            m_dataContext.Table<Platform>().AddNewRecord(vendor);
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(Platform), RecordOperation.UpdateRecord)]
        public void UpdatePlatform(Platform vendor)
        {
            vendor.UpdatedByID = GetCurrentUserID();
            vendor.UpdatedOn = DateTime.UtcNow;
            m_dataContext.Table<Platform>().UpdateRecord(vendor);
        }

        #endregion

        #region [ UserAccount Table Operations ]

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(UserAccount), RecordOperation.QueryRecordCount)]
        public int QueryUserAccountCount()
        {
            return m_dataContext.Table<UserAccount>().QueryRecordCount();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(UserAccount), RecordOperation.QueryRecords)]
        public IEnumerable<UserAccount> QueryUserAccounts(string sortField, bool ascending, int page, int pageSize)
        {
            return m_dataContext.Table<UserAccount>().QueryRecords(sortField, ascending, page, pageSize);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(UserAccount), RecordOperation.DeleteRecord)]
        public void DeleteUserAccount(Guid id)
        {
            m_dataContext.Table<UserAccount>().DeleteRecord(id);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(UserAccount), RecordOperation.CreateNewRecord)]
        public UserAccount NewUserAccount()
        {
            return new UserAccount();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(UserAccount), RecordOperation.AddNewRecord)]
        public void AddNewUserAccount(UserAccount user)
        {
            user.DefaultNodeID = MvcApplication.DefaultModel.Global.NodeID;
            user.CreatedBy = UserInfo.CurrentUserID;
            user.CreatedOn = DateTime.UtcNow;
            user.UpdatedBy = user.CreatedBy;
            user.UpdatedOn = user.CreatedOn;
            m_dataContext.Table<UserAccount>().AddNewRecord(user);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(UserAccount), RecordOperation.UpdateRecord)]
        public void UpdateUserAccount(UserAccount user)
        {
            user.DefaultNodeID = MvcApplication.DefaultModel.Global.NodeID;
            user.UpdatedBy = UserInfo.CurrentUserID;
            user.UpdatedOn = DateTime.UtcNow;
            m_dataContext.Table<UserAccount>().UpdateRecord(user);
        }

        #endregion

        #region [ Page Table Operations ]

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Page), RecordOperation.QueryRecordCount)]
        public int QueryPageCount()
        {
            return m_dataContext.Table<Page>().QueryRecordCount();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Page), RecordOperation.QueryRecords)]
        public IEnumerable<Page> QueryPages(string sortField, bool ascending, int page, int pageSize)
        {
            return m_dataContext.Table<Page>().QueryRecords(sortField, ascending, page, pageSize);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Page), RecordOperation.DeleteRecord)]
        public void DeletePage(int id)
        {
            m_dataContext.Table<Page>().DeleteRecord(id);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Page), RecordOperation.CreateNewRecord)]
        public Page NewPage()
        {
            return new Page();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Page), RecordOperation.AddNewRecord)]
        public void AddNewPage(Page page)
        {
            page.CreatedOn = DateTime.UtcNow;
            m_dataContext.Table<Page>().AddNewRecord(page);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Page), RecordOperation.UpdateRecord)]
        public void UpdatePage(Page page)
        {
            m_dataContext.Table<Page>().UpdateRecord(page);
        }

        #endregion

        #region [ Menu Table Operations ]

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Menu), RecordOperation.QueryRecordCount)]
        public int QueryMenuCount()
        {
            return m_dataContext.Table<Menu>().QueryRecordCount();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Menu), RecordOperation.QueryRecords)]
        public IEnumerable<Menu> QueryMenus(string sortField, bool ascending, int page, int pageSize)
        {
            return m_dataContext.Table<Menu>().QueryRecords(sortField, ascending, page, pageSize);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Menu), RecordOperation.DeleteRecord)]
        public void DeleteMenu(int id)
        {
            m_dataContext.Table<Menu>().DeleteRecord(id);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Menu), RecordOperation.CreateNewRecord)]
        public Menu NewMenu()
        {
            return new Menu();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Menu), RecordOperation.AddNewRecord)]
        public void AddNewMenu(Menu menu)
        {
            menu.CreatedOn = DateTime.UtcNow;
            m_dataContext.Table<Menu>().AddNewRecord(menu);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Menu), RecordOperation.UpdateRecord)]
        public void UpdateMenu(Menu menu)
        {
            m_dataContext.Table<Menu>().UpdateRecord(menu);
        }

        #endregion

        #region [ MenuItem Table Operations ]

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(MenuItem), RecordOperation.QueryRecordCount)]
        public int QueryMenuItemCount(int parentID)
        {
            return m_dataContext.Table<MenuItem>().QueryRecordCount(new RecordRestriction
            {
                FilterExpression = "MenuID = {0}",
                Parameters = new object[] { parentID }
            });
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(MenuItem), RecordOperation.QueryRecords)]
        public IEnumerable<MenuItem> QueryMenuItems(int parentID, string sortField, bool ascending, int page, int pageSize)
        {
            return m_dataContext.Table<MenuItem>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction
            {
                FilterExpression = "MenuID = {0}",
                Parameters = new object[] { parentID }
            });
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(MenuItem), RecordOperation.DeleteRecord)]
        public void DeleteMenuItem(int id)
        {
            m_dataContext.Table<MenuItem>().DeleteRecord(id);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(MenuItem), RecordOperation.CreateNewRecord)]
        public MenuItem NewMenuItem()
        {
            return new MenuItem();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(MenuItem), RecordOperation.AddNewRecord)]
        public void AddNewMenuItem(MenuItem menuItem)
        {
            m_dataContext.Table<MenuItem>().AddNewRecord(menuItem);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(MenuItem), RecordOperation.UpdateRecord)]
        public void UpdateMenuItem(MenuItem menuItem)
        {
            m_dataContext.Table<MenuItem>().UpdateRecord(menuItem);
        }

        #endregion

        #region [ ValueListGroup Table Operations ]

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueListGroup), RecordOperation.QueryRecordCount)]
        public int QueryValueListGroupCount()
        {
            return m_dataContext.Table<ValueListGroup>().QueryRecordCount();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueListGroup), RecordOperation.QueryRecords)]
        public IEnumerable<ValueListGroup> QueryValueListGroups(string sortField, bool ascending, int page, int pageSize)
        {
            return m_dataContext.Table<ValueListGroup>().QueryRecords(sortField, ascending, page, pageSize);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueListGroup), RecordOperation.DeleteRecord)]
        public void DeleteValueListGroup(int id)
        {
            m_dataContext.Table<ValueListGroup>().DeleteRecord(id);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueListGroup), RecordOperation.CreateNewRecord)]
        public ValueListGroup NewValueListGroup()
        {
            return new ValueListGroup();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueListGroup), RecordOperation.AddNewRecord)]
        public void AddNewValueListGroup(ValueListGroup valueListGroup)
        {
            valueListGroup.CreatedOn = DateTime.UtcNow;
            m_dataContext.Table<ValueListGroup>().AddNewRecord(valueListGroup);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueListGroup), RecordOperation.UpdateRecord)]
        public void UpdateValueListGroup(ValueListGroup valueListGroup)
        {
            m_dataContext.Table<ValueListGroup>().UpdateRecord(valueListGroup);
        }

        #endregion

        #region [ ValueList Table Operations ]


        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueList), RecordOperation.QueryRecordCount)]
        public int QueryValueListCount(int parentID)
        {
            return m_dataContext.Table<ValueList>().QueryRecordCount(new RecordRestriction
            {
                FilterExpression = "GroupID = {0}",
                Parameters = new object[] { parentID }
            });
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueList), RecordOperation.QueryRecords)]
        public IEnumerable<ValueList> QueryValueListItems(int parentID, string sortField, bool ascending, int page, int pageSize)
        {
            return m_dataContext.Table<ValueList>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction
            {
                FilterExpression = "GroupID = {0}",
                Parameters = new object[] { parentID }
            });
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueList), RecordOperation.DeleteRecord)]
        public void DeleteValueList(int id)
        {
            m_dataContext.Table<ValueList>().DeleteRecord(id);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueList), RecordOperation.CreateNewRecord)]
        public ValueList NewValueList()
        {
            return new ValueList();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueList), RecordOperation.AddNewRecord)]
        public void AddNewValueList(ValueList valueList)
        {
            valueList.CreatedOn = DateTime.UtcNow;
            m_dataContext.Table<ValueList>().AddNewRecord(valueList);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueList), RecordOperation.UpdateRecord)]
        public void UpdateValueList(ValueList valueList)
        {
            m_dataContext.Table<ValueList>().UpdateRecord(valueList);
        }

        #endregion

        #region [ Miscellaneous Hub Operations ]

        /// <summary>
        /// Gets page setting for specified page.
        /// </summary>
        /// <param name="pageID">ID of page record.</param>
        /// <param name="key">Setting key name.</param>
        /// <param name="defaultValue">Setting default value.</param>
        /// <returns>Page setting for specified page.</returns>
        public string GetPageSetting(int pageID, string key, string defaultValue)
        {
            Page page = m_dataContext.QueryRecord<Page>(pageID);
            Dictionary<string, string> pageSettings = (page?.ServerConfiguration ?? "").ParseKeyValuePairs();
            AppModel model = MvcApplication.DefaultModel;
            return model.GetPageSetting(pageSettings, model.Global.PageDefaults, key, defaultValue);
        }

        /// <summary>
        /// Gets SID for a given user name.
        /// </summary>
        /// <param name="userName">User name to convert to SID.</param>
        /// <returns>SID for a given user name.</returns>
        public string UserNameToSID(string userName)
        {
            return UserInfo.UserNameToSID(userName);
        }

        /// <summary>
        /// Gets SID for a given group name.
        /// </summary>
        /// <param name="groupName">Group name to convert to SID.</param>
        /// <returns>SID for a given group name.</returns>
        public string GroupNameToSID(string groupName)
        {
            return UserInfo.GroupNameToSID(groupName);
        }

        /// <summary>
        /// Gets account name for a given SID.
        /// </summary>
        /// <param name="sid">SID to convert to a account name.</param>
        /// <returns>Account name for a given SID.</returns>
        public string SIDToAccountName(string sid)
        {
            return UserInfo.SIDToAccountName(sid);
        }

        /// <summary>
        /// Gets UserAccount table ID for current user.
        /// </summary>
        /// <returns>UserAccount.ID for current user.</returns>
        public Guid GetCurrentUserID()
        {
            Guid userID;
            MvcApplication.UserIDCache.TryGetValue(UserInfo.CurrentUserID, out userID);
            return userID;
        }

        #endregion
    }
}
