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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using GSF;
using GSF.Data.Model;
using GSF.Identity;
using GSF.Web.Model;
using GSF.Web.Security;
using Microsoft.AspNet.SignalR;
using openSPM.Model;

namespace openSPM
{
    [AuthorizeHubRole]
    public class DataHub : Hub, IRecordOperationsHub
    {
        #region [ Members ]

        // Fields
        private readonly DataContext m_dataContext;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        public DataHub()
        {
            m_dataContext = new DataContext(exceptionHandler: MvcApplication.LogException);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets <see cref="IRecordOperationsHub.RecordOperationsCache"/> for SignalR hub.
        /// </summary>
        public RecordOperationsCache RecordOperationsCache => s_recordOperationsCache;

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
        private static readonly RecordOperationsCache s_recordOperationsCache;

        // Static Constructor
        static DataHub()
        {
            // Analyze and cache record operations of security hub
            s_recordOperationsCache = new RecordOperationsCache(typeof(DataHub));
        }

        #endregion

        // Client-side script functionality

        #region [ Patch Table Operations ]

        [RecordOperation(typeof(Patch), RecordOperation.QueryRecordCount)]
        public int QueryPatchCount(bool showDeleted)
        {
            if (showDeleted)
                return m_dataContext.Table<Patch>().QueryRecordCount();

            return m_dataContext.Table<Patch>().QueryRecordCount(new RecordRestriction("IsDeleted = 0"));
        }

        [RecordOperation(typeof(Patch), RecordOperation.QueryRecords)]
        public IEnumerable<Patch> QueryPatches(bool showDeleted, string sortField, bool ascending, int page, int pageSize)
        {
            if (showDeleted)
                return m_dataContext.Table<Patch>().QueryRecords(sortField, ascending, page, pageSize);

            return m_dataContext.Table<Patch>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("IsDeleted = 0"));
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

            return m_dataContext.Table<Vendor>().QueryRecordCount(new RecordRestriction("IsDeleted = 0"));
        }

        [RecordOperation(typeof(Vendor), RecordOperation.QueryRecords)]
        public IEnumerable<Vendor> QueryVendors(bool showDeleted, string sortField, bool ascending, int page, int pageSize)
        {
            if (showDeleted)
                return m_dataContext.Table<Vendor>().QueryRecords(sortField, ascending, page, pageSize);

            return m_dataContext.Table<Vendor>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("IsDeleted = 0"));
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

            return m_dataContext.Table<Platform>().QueryRecordCount(new RecordRestriction("IsDeleted = 0"));
        }

        [RecordOperation(typeof(Platform), RecordOperation.QueryRecords)]
        public IEnumerable<Platform> QueryPlatforms(bool showDeleted, string sortField, bool ascending, int page, int pageSize)
        {
            if (showDeleted)
                return m_dataContext.Table<Platform>().QueryRecords(sortField, ascending, page, pageSize);

            return m_dataContext.Table<Platform>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("IsDeleted = 0"));
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

        /// <summary>
        /// Searches platforms by name.
        /// </summary>
        /// <param name="searchText">Search text to lookup.</param>
        /// <returns>Search results as "IDLabel" values - serialized as JSON [{ id: "value", label : "name" }, ...]; useful for dynamic lookup lists.</returns>
        public IEnumerable<IDLabel> SearchPlatforms(string searchText)
        {
            return m_dataContext
                .Table<Platform>()
                .QueryRecords()
                .Where(record => record?.Name?.StartsWith(searchText, StringComparison.InvariantCultureIgnoreCase) ?? false)
                .Select(record => IDLabel.Create(record.ID.ToString(), record.Name));
        }

        #endregion

        #region [ UserAccountPlatform Table Operations ]

        public int QueryUserAccountPlatformCount(Guid userAccountID)
        {
            return m_dataContext.Table<UserAccountPlatform>().QueryRecordCount(new RecordRestriction("UserAccountID = {0}", userAccountID));
        }

        public IEnumerable<UserAccountPlatformDetail> QueryUserAccountPlatforms(Guid userAccountID, string sortField, bool ascending, int page, int pageSize)
        {
            return m_dataContext.Table<UserAccountPlatformDetail>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("UserAccountID = {0}", userAccountID));
        }

        [AuthorizeHubRole("Administrator, Owner")]
        public void DeleteUserAccountPlatform(Guid userAccountID, int platformID)
        {
            m_dataContext.Table<UserAccountPlatform>().DeleteRecord(userAccountID, platformID);
        }

        public UserAccountPlatform NewUserAccountPlatform()
        {
            return new UserAccountPlatform();
        }

        [AuthorizeHubRole("Administrator, Owner")]
        public void AddNewUserAccountPlatform(UserAccountPlatform record)
        {
            if (m_dataContext.Table<UserAccountPlatform>().QueryRecordCount(new RecordRestriction("UserAccountID = {0} AND PlatformID = {1}", record.UserAccountID, record.PlatformID)) == 0)
                m_dataContext.Table<UserAccountPlatform>().AddNewRecord(record);
        }

        #endregion

        #region [ BusinessUnit Table Operations ]

        [RecordOperation(typeof(BusinessUnit), RecordOperation.QueryRecordCount)]
        public int QueryBusinessUnitCount(bool showDeleted)
        {
            if (showDeleted)
                return m_dataContext.Table<BusinessUnit>().QueryRecordCount();

            return m_dataContext.Table<BusinessUnit>().QueryRecordCount(new RecordRestriction("IsDeleted = 0"));
        }

        [RecordOperation(typeof(BusinessUnit), RecordOperation.QueryRecords)]
        public IEnumerable<BusinessUnit> QueryBusinessUnits(bool showDeleted, string sortField, bool ascending, int page, int pageSize)
        {
            if (showDeleted)
                return m_dataContext.Table<BusinessUnit>().QueryRecords(sortField, ascending, page, pageSize);

            return m_dataContext.Table<BusinessUnit>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("IsDeleted = 0"));
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(BusinessUnit), RecordOperation.DeleteRecord)]
        public void DeleteBusinessUnit(int id)
        {
            // For BusinessUnits, we only "mark" a record as deleted
            m_dataContext.Connection.ExecuteNonQuery("UPDATE BusinessUnit SET IsDeleted=1 WHERE ID={0}", id);
        }

        [RecordOperation(typeof(BusinessUnit), RecordOperation.CreateNewRecord)]
        public BusinessUnit NewBusinessUnit()
        {
            return new BusinessUnit();
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(BusinessUnit), RecordOperation.AddNewRecord)]
        public void AddNewBusinessUnit(BusinessUnit record)
        {
            record.CreatedByID = GetCurrentUserID();
            record.CreatedOn = DateTime.UtcNow;
            record.UpdatedByID = record.CreatedByID;
            record.UpdatedOn = record.CreatedOn;
            m_dataContext.Table<BusinessUnit>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(BusinessUnit), RecordOperation.UpdateRecord)]
        public void UpdateBusinessUnit(BusinessUnit record)
        {
            record.UpdatedByID = GetCurrentUserID();
            record.UpdatedOn = DateTime.UtcNow;
            m_dataContext.Table<BusinessUnit>().UpdateRecord(record);
        }

        #endregion

        #region [ BusinessUnitUserAccount Table Operations ]

        public int QueryBusinessUnitUserAccountCount(int businessUnitID)
        {
            return m_dataContext.Table<BusinessUnitUserAccount>().QueryRecordCount(new RecordRestriction("BusinessUnitID = {0}", businessUnitID));
        }

        public IEnumerable<BusinessUnitUserAccountDetail> QueryBusinessUnitUserAccounts(int businessUnitID, bool ascending, int page, int pageSize)
        {
            IEnumerable<BusinessUnitUserAccountDetail> resolvedAccountRecords = m_dataContext.Table<BusinessUnitUserAccountDetail>().
                QueryRecords("UserAccountName", ascending, page, pageSize, new RecordRestriction("BusinessUnitID = {0}", businessUnitID)).
                Select(account =>
                {
                    account.UserAccountName = UserInfo.SIDToAccountName(account.UserAccountName);
                    return account;
                });

            if (ascending)
                return resolvedAccountRecords.OrderBy(key => key.UserAccountName);

            return resolvedAccountRecords.OrderByDescending(key => key.UserAccountName);
        }

        [AuthorizeHubRole("Administrator, Owner")]
        public void DeleteBusinessUnitUserAccount(int businessUnitID, Guid userAccountID)
        {
            m_dataContext.Table<BusinessUnitUserAccount>().DeleteRecord(businessUnitID, userAccountID);
        }

        public BusinessUnitUserAccount NewBusinessUnitUserAccount()
        {
            return new BusinessUnitUserAccount();
        }

        [AuthorizeHubRole("Administrator, Owner")]
        public void AddNewBusinessUnitUserAccount(BusinessUnitUserAccount record)
        {
            if (m_dataContext.Table<BusinessUnitUserAccount>().QueryRecordCount(new RecordRestriction("BusinessUnitID = {0} AND UserAccountID = {1}", record.BusinessUnitID, record.UserAccountID)) == 0)
                m_dataContext.Table<BusinessUnitUserAccount>().AddNewRecord(record);
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
            return m_dataContext.Table<MenuItem>().QueryRecordCount(new RecordRestriction("MenuID = {0}", parentID));
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(MenuItem), RecordOperation.QueryRecords)]
        public IEnumerable<MenuItem> QueryMenuItems(int parentID, string sortField, bool ascending, int page, int pageSize)
        {
            return m_dataContext.Table<MenuItem>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("MenuID = {0}", parentID));
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
            return m_dataContext.Table<ValueList>().QueryRecordCount(new RecordRestriction("GroupID = {0}", parentID));
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueList), RecordOperation.QueryRecords)]
        public IEnumerable<ValueList> QueryValueListItems(int parentID, string sortField, bool ascending, int page, int pageSize)
        {
            return m_dataContext.Table<ValueList>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("GroupID = {0}", parentID));
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

        #region [Assessment Table Operations]

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Assessment), RecordOperation.QueryRecordCount)]
        public int QueryAssessmentCount()
        {
            return m_dataContext.Table<Assessment>().QueryRecordCount();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Assessment), RecordOperation.QueryRecords)]
        public IEnumerable<Assessment> QueryAssessments(string sortField, bool ascending, int page, int pageSize)
        {
            return m_dataContext.Table<Assessment>().QueryRecords(sortField, ascending, page, pageSize);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Assessment), RecordOperation.DeleteRecord)]
        public void DeleteAssessment(int id)
        {
            m_dataContext.Table<Assessment>().DeleteRecord(id);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Assessment), RecordOperation.CreateNewRecord)]
        public Assessment NewAssessment()
        {
            return new Assessment();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Assessment), RecordOperation.AddNewRecord)]
        public void AddNewAssessment(Assessment record)
        {

            record.CreatedByID = GetCurrentUserID();
            record.CreatedOn = DateTime.UtcNow;
            record.UpdatedByID = GetCurrentUserID();
            record.UpdatedOn = DateTime.UtcNow;
            m_dataContext.Table<Assessment>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Assessment), RecordOperation.UpdateRecord)]
        public void UpdateAssessment(Assessment record)
        {
            record.UpdatedByID = GetCurrentUserID();
            record.UpdatedOn = DateTime.UtcNow;
            m_dataContext.Table<Assessment>().UpdateRecord(record);
        }

        #endregion

        #region [PatchStatusAssessmentDetail Table Operations]

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(PatchStatusAssessmentDetail), RecordOperation.QueryRecordCount)]
        public int QueryPatchStatusAssessmentDetailCount()
        {
            return m_dataContext.Table<PatchStatusAssessmentDetail>().QueryRecordCount();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(PatchStatusAssessmentDetail), RecordOperation.QueryRecords)]
        public IEnumerable<PatchStatusAssessmentDetail> QueryPatchStatusAssessmentDetails(string sortField, bool ascending, int page, int pageSize)
        {
            return m_dataContext.Table<PatchStatusAssessmentDetail>().QueryRecords(sortField, ascending, page, pageSize);
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(PatchStatusAssessmentDetail), RecordOperation.CreateNewRecord)]
        public PatchStatusAssessmentDetail NewPatchStatusAssessmentDetail()
        {
            return new PatchStatusAssessmentDetail();
        }
        

        #endregion

        #region [ LatestVendorDiscoveryResult View Operations ]

        [RecordOperation(typeof(LatestVendorDiscoveryResult), RecordOperation.QueryRecordCount)]
        public int QueryLatestVendorDiscoveryResultCount(bool showDeleted)
        {
            if (showDeleted)
                return m_dataContext.Table<LatestVendorDiscoveryResult>().QueryRecordCount();

            return m_dataContext.Table<LatestVendorDiscoveryResult>().QueryRecordCount(new RecordRestriction("IsDeleted = 0"));
        }

        [RecordOperation(typeof(LatestVendorDiscoveryResult), RecordOperation.QueryRecords)]
        public IEnumerable<LatestVendorDiscoveryResult> QueryLatestVendorDiscoveryResults(bool showDeleted, string sortField, bool ascending, int page, int pageSize)
        {
            if (showDeleted)
                return m_dataContext.Table<LatestVendorDiscoveryResult>().QueryRecords(sortField, ascending, page, pageSize);

            return m_dataContext.Table<LatestVendorDiscoveryResult>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("IsDeleted = 0"));
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(LatestVendorDiscoveryResult), RecordOperation.DeleteRecord)]
        public void DeleteLatestVendorDiscoveryResult(int id)
        {
            // Delete associated DiscoveryResult record
            m_dataContext.Table<DiscoveryResult>().DeleteRecord(id);
        }

        [RecordOperation(typeof(LatestVendorDiscoveryResult), RecordOperation.CreateNewRecord)]
        public LatestVendorDiscoveryResult NewLatestVendorDiscoveryResult()
        {
            return new LatestVendorDiscoveryResult();
        }

        [AuthorizeHubRole("Administrator, Owner, PIC")]
        [RecordOperation(typeof(LatestVendorDiscoveryResult), RecordOperation.AddNewRecord)]
        public void AddNewLatestVendorDiscoveryResult(LatestVendorDiscoveryResult record)
        {
            DiscoveryResult result = DeriveDiscoveryResult(record);
            result.CreatedByID = GetCurrentUserID();
            result.CreatedOn = DateTime.UtcNow;
            m_dataContext.Table<DiscoveryResult>().AddNewRecord(result);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC")]
        [RecordOperation(typeof(LatestVendorDiscoveryResult), RecordOperation.UpdateRecord)]
        public void UpdateLatestVendorDiscoveryResult(LatestVendorDiscoveryResult record)
        {
            m_dataContext.Table<DiscoveryResult>().UpdateRecord(DeriveDiscoveryResult(record));
        }

        private DiscoveryResult DeriveDiscoveryResult(LatestVendorDiscoveryResult record)
        {
            return new DiscoveryResult
            {
                ID = record.DiscoveryResultID,
                VendorID = record.VendorID,
                ReviewDate = record.ReviewDate,
                ResultKey = record.ResultKey,
                Notes = record.Notes,
                CreatedByID = record.CreatedByID,
                CreatedOn =  record.CreatedOn
            };
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
        /// Gets the absolute path for a virtual path, e.g., ~/Images/Menu
        /// </summary>
        /// <param name="path">Virtual path o convert to absolute path.</param>
        /// <returns>Absolute path for a virtual path.</returns>
        public string GetAbsolutePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return "";

            return VirtualPathUtility.ToAbsolute(path);
        }

        /// <summary>
        /// Gets UserAccount table ID for current user.
        /// </summary>
        /// <returns>UserAccount.ID for current user.</returns>
        public Guid GetCurrentUserID()
        {
            Guid userID;
            AuthorizationCache.UserIDs.TryGetValue(UserInfo.CurrentUserID, out userID);
            return userID;
        }

        #endregion
    }
}
