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
using GSF.Security;
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
                        new Tuple<string, string>(method.Name, authorizeHubRoleAttribute?.Roles);
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
        public void AddNewPatch(Patch record)
        {
            record.CreatedByID = GetCurrentUserID();
            record.CreatedOn = DateTime.UtcNow;
            record.UpdatedByID = record.CreatedByID;
            record.UpdatedOn = record.CreatedOn;
            m_dataContext.Table<Patch>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC")]
        [RecordOperation(typeof(Patch), RecordOperation.UpdateRecord)]
        public void UpdatePatch(Patch record)
        {
            record.UpdatedByID = GetCurrentUserID();
            record.UpdatedOn = DateTime.UtcNow;
            m_dataContext.Table<Patch>().UpdateRecord(record);
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
        public void AddNewVendor(Vendor record)
        {
            record.CreatedByID = GetCurrentUserID();
            record.CreatedOn = DateTime.UtcNow;
            record.UpdatedByID = record.CreatedByID;
            record.UpdatedOn = record.CreatedOn;
            m_dataContext.Table<Vendor>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(Vendor), RecordOperation.UpdateRecord)]
        public void UpdateVendor(Vendor record)
        {
            record.UpdatedByID = GetCurrentUserID();
            record.UpdatedOn = DateTime.UtcNow;
            m_dataContext.Table<Vendor>().UpdateRecord(record);
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
        public void AddNewPlatform(Platform record)
        {
            record.CreatedByID = GetCurrentUserID();
            record.CreatedOn = DateTime.UtcNow;
            record.UpdatedByID = record.CreatedByID;
            record.UpdatedOn = record.CreatedOn;
            m_dataContext.Table<Platform>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(Platform), RecordOperation.UpdateRecord)]
        public void UpdatePlatform(Platform record)
        {
            record.UpdatedByID = GetCurrentUserID();
            record.UpdatedOn = DateTime.UtcNow;
            m_dataContext.Table<Platform>().UpdateRecord(record);
        }

        #endregion

        #region [ BusinessUnitGroup Table Operations ]

        [RecordOperation(typeof(BusinessUnitGroup), RecordOperation.QueryRecordCount)]
        public int QueryBusinessUnitGroupCount(bool showDeleted)
        {
            if (showDeleted)
                return m_dataContext.Table<BusinessUnitGroup>().QueryRecordCount();

            return m_dataContext.Table<BusinessUnitGroup>().QueryRecordCount(new RecordRestriction
            {
                FilterExpression = "IsDeleted = 0"
            });
        }

        [RecordOperation(typeof(BusinessUnitGroup), RecordOperation.QueryRecords)]
        public IEnumerable<BusinessUnitGroup> QueryBusinessUnitGroups(bool showDeleted, string sortField, bool ascending, int page, int pageSize)
        {
            if (showDeleted)
                return m_dataContext.Table<BusinessUnitGroup>().QueryRecords(sortField, ascending, page, pageSize);

            return m_dataContext.Table<BusinessUnitGroup>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction
            {
                FilterExpression = "IsDeleted = 0"
            });
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(BusinessUnitGroup), RecordOperation.DeleteRecord)]
        public void DeleteBusinessUnitGroup(int id)
        {
            // For BusinessUnitGroups, we only "mark" a record as deleted
            m_dataContext.Connection.ExecuteNonQuery("UPDATE BusinessUnitGroup SET IsDeleted=1 WHERE ID={0}", id);
        }

        [RecordOperation(typeof(BusinessUnitGroup), RecordOperation.CreateNewRecord)]
        public BusinessUnitGroup NewBusinessUnitGroup()
        {
            return new BusinessUnitGroup();
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(BusinessUnitGroup), RecordOperation.AddNewRecord)]
        public void AddNewBusinessUnitGroup(BusinessUnitGroup record)
        {
            record.CreatedByID = GetCurrentUserID();
            record.CreatedOn = DateTime.UtcNow;
            record.UpdatedByID = record.CreatedByID;
            record.UpdatedOn = record.CreatedOn;
            m_dataContext.Table<BusinessUnitGroup>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(BusinessUnitGroup), RecordOperation.UpdateRecord)]
        public void UpdateBusinessUnitGroup(BusinessUnitGroup record)
        {
            record.UpdatedByID = GetCurrentUserID();
            record.UpdatedOn = DateTime.UtcNow;
            m_dataContext.Table<BusinessUnitGroup>().UpdateRecord(record);
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
        public void AddNewUserAccount(UserAccount record)
        {
            if (!record.UseADAuthentication && !string.IsNullOrWhiteSpace(record.Password))
                record.Password = SecurityProviderUtility.EncryptPassword(record.Password);

            record.DefaultNodeID = MvcApplication.DefaultModel.Global.NodeID;
            record.CreatedBy = UserInfo.CurrentUserID;
            record.CreatedOn = DateTime.UtcNow;
            record.UpdatedBy = record.CreatedBy;
            record.UpdatedOn = record.CreatedOn;
            m_dataContext.Table<UserAccount>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(UserAccount), RecordOperation.UpdateRecord)]
        public void UpdateUserAccount(UserAccount record)
        {
            if (!record.UseADAuthentication && !string.IsNullOrWhiteSpace(record.Password))
                record.Password = SecurityProviderUtility.EncryptPassword(record.Password);

            record.DefaultNodeID = MvcApplication.DefaultModel.Global.NodeID;
            record.UpdatedBy = UserInfo.CurrentUserID;
            record.UpdatedOn = DateTime.UtcNow;
            m_dataContext.Table<UserAccount>().UpdateRecord(record);
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
        public void AddNewPage(Page record)
        {
            record.CreatedOn = DateTime.UtcNow;
            m_dataContext.Table<Page>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Page), RecordOperation.UpdateRecord)]
        public void UpdatePage(Page record)
        {
            m_dataContext.Table<Page>().UpdateRecord(record);
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
        public void AddNewMenu(Menu record)
        {
            record.CreatedOn = DateTime.UtcNow;
            m_dataContext.Table<Menu>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Menu), RecordOperation.UpdateRecord)]
        public void UpdateMenu(Menu record)
        {
            m_dataContext.Table<Menu>().UpdateRecord(record);
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
        public void AddNewMenuItem(MenuItem record)
        {
            // TODO: MenuItem.Text is currently required in database, but empty should be allowed for spacer items
            if (string.IsNullOrEmpty(record.Text))
                record.Text = " ";

            m_dataContext.Table<MenuItem>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(MenuItem), RecordOperation.UpdateRecord)]
        public void UpdateMenuItem(MenuItem record)
        {
            // TODO: MenuItem.Text is currently required in database, but empty should be allowed for spacer items
            if (string.IsNullOrEmpty(record.Text))
                record.Text = " ";

            m_dataContext.Table<MenuItem>().UpdateRecord(record);
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
        public void AddNewValueListGroup(ValueListGroup record)
        {
            record.CreatedOn = DateTime.UtcNow;
            m_dataContext.Table<ValueListGroup>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueListGroup), RecordOperation.UpdateRecord)]
        public void UpdateValueListGroup(ValueListGroup record)
        {
            m_dataContext.Table<ValueListGroup>().UpdateRecord(record);
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
        public void AddNewValueList(ValueList record)
        {
            record.CreatedOn = DateTime.UtcNow;
            m_dataContext.Table<ValueList>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueList), RecordOperation.UpdateRecord)]
        public void UpdateValueList(ValueList record)
        {
            m_dataContext.Table<ValueList>().UpdateRecord(record);
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
            Page page = m_dataContext.Table<Page>().LoadRecord(pageID);
            Dictionary<string, string> pageSettings = (page?.ServerConfiguration ?? "").ParseKeyValuePairs();
            AppModel model = MvcApplication.DefaultModel;
            return model.GetPageSetting(pageSettings, model.Global.PageDefaults, key, defaultValue);
        }

        /// <summary>
        /// Gets the specified user account record.
        /// </summary>
        /// <param name="id">ID of requested user.</param>
        /// <returns>Specified user account record.</returns>
        public UserAccount QueryUserAccount(Guid id)
        {
            return m_dataContext.Table<UserAccount>().LoadRecord(id);
        }

        /// <summary>
        /// Gets the current application role records.
        /// </summary>
        /// <returns>Current application role records.</returns>
        public IEnumerable<ApplicationRole> QueryApplicationRoles()
        {
            return m_dataContext.Table<ApplicationRole>().QueryRecords("SELECT ID FROM ApplicationRole WHERE NodeID={0} ORDER BY Name", MvcApplication.DefaultModel.Global.NodeID);
        }

        /// <summary>
        /// Determines if user is in role based on database ID values.
        /// </summary>
        /// <param name="userID">User ID value.</param>
        /// <param name="roleID">Role ID value.</param>
        /// <returns><c>true</c> if user is in role; otherwise, <c>false</c>.</returns>
        public bool UserIsInRole(Guid userID, Guid roleID)
        {
            return m_dataContext.Table<ApplicationRoleUserAccount>().QueryRecordCount(new RecordRestriction
            {
                FilterExpression = "UserAccountID={0} AND ApplicationRoleID={1}",
                Parameters = new object[] { userID, roleID }
            }) > 0;
        }

        /// <summary>
        /// Adds user to a role.
        /// </summary>
        /// <param name="userID">User ID value.</param>
        /// <param name="roleID">Role ID value.</param>
        /// <returns><c>true</c> if user was added; otherwise <c>false</c>.</returns>
        [AuthorizeHubRole("Administrator")]
        public bool AddUserToRole(Guid userID, Guid roleID)
        {
            // Nothing to do if user is already in role
            if (UserIsInRole(userID, roleID))
                return false;

            return m_dataContext.Table<ApplicationRoleUserAccount>().AddNewRecord(new ApplicationRoleUserAccount
            {
                ApplicationRoleID = roleID,
                UserAccountID = userID
            }) > 0;
        }

        /// <summary>
        /// Removes user from a role.
        /// </summary>
        /// <param name="userID">User ID value.</param>
        /// <param name="roleID">Role ID value.</param>
        /// <returns><c>true</c> if user was removed; otherwise <c>false</c>.</returns>
        [AuthorizeHubRole("Administrator")]
        public bool RemoveUserFromRole(Guid userID, Guid roleID)
        {
            // Nothing to do if user is not currently in role
            if (!UserIsInRole(userID, roleID))
                return false;

            return m_dataContext.Table<ApplicationRoleUserAccount>().DeleteRecord(new RecordRestriction
            {
                FilterExpression = "UserAccountID={0} AND ApplicationRoleID={1}",
                Parameters = new object[] { userID, roleID }
            }) > 0;
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
