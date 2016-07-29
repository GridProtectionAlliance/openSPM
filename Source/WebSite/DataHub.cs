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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using GSF;
using GSF.Data.Model;
using GSF.Identity;
using GSF.Web.Model;
using GSF.Web.Security;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using openSPM.Model;
using openSPM.Models;

namespace openSPM
{
    [AuthorizeHubRole]
    public class DataHub : Hub, IRecordOperationsHub
    {
        #region [ Members ]

        // Fields
        private readonly DataContext m_dataContext;
        private DataContext m_miPlanContext;
        private AppModel m_appmodel;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        public DataHub()
        {
            m_dataContext = new DataContext(exceptionHandler: MvcApplication.LogException);
            m_appmodel = new AppModel(m_dataContext);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets <see cref="GSF.Web.Model.RecordOperationsCache"/> for SignalR hub.
        /// </summary>
        public RecordOperationsCache RecordOperationsCache => s_recordOperationsCache;

        // Gets reference to MiPlan context, creating it if needed
        private DataContext MiPlanContext => m_miPlanContext ?? (m_miPlanContext = new DataContext("miPlanDB", exceptionHandler: MvcApplication.LogException));

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
                    {
                        m_dataContext?.Dispose();
                        m_miPlanContext?.Dispose();
                    }
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

        // Static Methods

        /// <summary>
        /// Gets statically cached instance of <see cref="RecordOperationsCache"/> for <see cref="DataHub"/> instances.
        /// </summary>
        /// <returns>Statically cached instance of <see cref="RecordOperationsCache"/> for <see cref="DataHub"/> instances.</returns>
        public static RecordOperationsCache GetRecordOperationsCache() => s_recordOperationsCache;

        // Static Constructor
        static DataHub()
        {
            // Analyze and cache record operations of security hub
            s_recordOperationsCache = new RecordOperationsCache(typeof(DataHub));
        }

        #endregion

        // Client-side script functionality

        #region [ Patch Table Operations ]
        
        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(Patch), RecordOperation.QueryRecordCount)]
        public int QueryPatchCount(bool showDeleted, string filterText = "%")
        {

            if (filterText == null) filterText = "%";
            else
            {
                // Build your filter string here!
                filterText += "%";
            }



            if (showDeleted)
                return m_dataContext.Table<Patch>().QueryRecordCount(new RecordRestriction("VendorPatchName LIKE {0}", filterText));
            return m_dataContext.Table<Patch>().QueryRecordCount(new RecordRestriction("IsDeleted = 0 AND VendorPatchName LIKE {0}", filterText));
            
        }

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(Patch), RecordOperation.QueryRecords)]
        public IEnumerable<Patch> QueryPatches(bool showDeleted, string sortField, bool ascending, int page, int pageSize, string filterText = "%")
        {
            if (filterText == null) filterText = "%";
            else
            {
                // Build your filter string here!
                filterText += "%";
            }



            if (showDeleted)
               return m_dataContext.Table<Patch>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("VendorPatchName LIKE {0}", filterText));
            return m_dataContext.Table<Patch>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("IsDeleted = 0 AND VendorPatchName LIKE {0}", filterText));
        }

        public Patch QueryAPatch(int id)
        {
           
            return m_dataContext.Table<Patch>().LoadRecord(id);
        }

        [AuthorizeHubRole("Administrator, Owner, SME")]
        [RecordOperation(typeof(Patch), RecordOperation.DeleteRecord)]
        public void DeletePatch(int id)
        {
            IEnumerable<PatchStatus> records = m_dataContext.Table<PatchStatus>().QueryRecords(restriction: new RecordRestriction("PatchID = {0}", id));
            foreach (PatchStatus ps in records)
            {
                m_dataContext.Table<PatchStatus>().DeleteRecord(ps.ID);
            }

            // For Patches, we only "mark" a record as deleted
            m_dataContext.Connection.ExecuteNonQuery("UPDATE Patch SET IsDeleted=1 WHERE ID={0}", id);
        }

        [AuthorizeHubRole("Administrator, Owner, SME")]
        public void UpdatePatchInitatedFlag(int id)
        {
            // For Patches, we only "mark" a record as deleted
            m_dataContext.Connection.ExecuteNonQuery("UPDATE Patch SET IsInitiated=1 WHERE ID={0}", id);
        }

        [RecordOperation(typeof(Patch), RecordOperation.CreateNewRecord)]
        public Patch NewPatch()
        {
            return new Patch();
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        [RecordOperation(typeof(Patch), RecordOperation.AddNewRecord)]
        public void AddNewPatch(Patch record)
        {
            string companyname = m_appmodel.Global.CompanyAcronym.ToString();
            string year = DateTime.UtcNow.Year.ToString();
            string month = DateTime.UtcNow.Month.ToString().PadLeft(2,'0');
            string count = m_appmodel.GetNextCounterValue().ToString().PadLeft(4,'0');
            record.VendorPatchName = companyname + '-' + year + '-' + month + '-' + count;  
            record.CreatedByID = GetCurrentUserID();
            record.CreatedOn = DateTime.UtcNow;
            record.UpdatedByID = record.CreatedByID;
            record.UpdatedOn = record.CreatedOn;
            record.IsInitiated = false;
            m_dataContext.Table<Patch>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        public int GetLastPatchID()
        {
            return m_dataContext.Connection.ExecuteScalar<int?>("SELECT IDENT_CURRENT('Patch')") ?? 0;
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        [RecordOperation(typeof(Patch), RecordOperation.UpdateRecord)]
        public void UpdatePatch(Patch record)
        {
            record.UpdatedByID = GetCurrentUserID();
            record.UpdatedOn = DateTime.UtcNow;
            m_dataContext.Table<Patch>().UpdateRecord(record);


        }

        #endregion

        #region [ PatchView Table Operations ]

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(PatchView), RecordOperation.QueryRecordCount)]
        public int QueryPatchViewCount(bool showDeleted, string filterText = "%")
        {
            string patchFilter = "%";
            string productFilter = "%";
            string vendorFilter = "%";
            if(filterText != "%")
            {
                string[] filters = filterText.Split(';');
                if(filters.Length == 3)
                {
                    patchFilter += filters[0] + '%';
                    productFilter += filters[1] += '%';
                    vendorFilter += filters[2] += '%';
                }
            }

            if (showDeleted)
                return m_dataContext.Table<PatchView>().QueryRecordCount(new RecordRestriction("VendorPatchName LIKE {0} AND ProductName LIKE {1} AND VendorName LIKE {2}", patchFilter, productFilter, vendorFilter));
            return m_dataContext.Table<PatchView>().QueryRecordCount(new RecordRestriction("IsDeleted = 0 AND VendorPatchName LIKE {0} AND ProductName LIKE {1} AND VendorName LIKE {2}", patchFilter, productFilter, vendorFilter));

        }

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(PatchView), RecordOperation.QueryRecords)]
        public IEnumerable<PatchView> QueryPatchViews(bool showDeleted, string sortField, bool ascending, int page, int pageSize, string filterText = "%")
        {
            string patchFilter = "%";
            string productFilter = "%";
            string vendorFilter = "%";
            if (filterText != "%")
            {
                string[] filters = filterText.Split(';');
                if (filters.Length == 3)
                {
                    patchFilter += filters[0] + '%';
                    productFilter += filters[1] += '%';
                    vendorFilter += filters[2] += '%';
                }
            }



            if (showDeleted)
                return m_dataContext.Table<PatchView>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("VendorPatchName LIKE {0} AND ProductName LIKE {1} AND VendorName LIKE {2}", patchFilter, productFilter, vendorFilter));
            return m_dataContext.Table<PatchView>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("IsDeleted = 0 AND VendorPatchName LIKE {0} AND ProductName LIKE {1} AND VendorName LIKE {2}", patchFilter, productFilter, vendorFilter));
        }

        public PatchView QueryAPatchView(int id)
        {

            return m_dataContext.Table<PatchView>().LoadRecord(id);
        }

        [AuthorizeHubRole("Administrator, Owner, SME")]
        [RecordOperation(typeof(PatchView), RecordOperation.DeleteRecord)]
        public void DeletePatchView(int id)
        {
            IEnumerable<PatchStatus> records = m_dataContext.Table<PatchStatus>().QueryRecords(restriction: new RecordRestriction("PatchID = {0}", id));
            foreach (PatchStatus ps in records)
            {
                m_dataContext.Table<PatchStatus>().DeleteRecord(ps.ID);
            }

            // For Patches, we only "mark" a record as deleted
            m_dataContext.Connection.ExecuteNonQuery("UPDATE Patch SET IsDeleted=1 WHERE ID={0}", id);
        }

        [AuthorizeHubRole("Administrator, Owner, SME")]
        public void UpdatePatchViewInitatedFlag(int id)
        {
            // For PatchViewViewes, we only "mark" a record as deleted
            m_dataContext.Connection.ExecuteNonQuery("UPDATE Patch SET IsInitiated=1 WHERE ID={0}", id);
        }

        [RecordOperation(typeof(PatchView), RecordOperation.CreateNewRecord)]
        public PatchView NewPatchView()
        {
            return new PatchView();
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        [RecordOperation(typeof(PatchView), RecordOperation.AddNewRecord)]
        public void AddNewPatchView(PatchView record)
        {
            string companyname = m_appmodel.Global.CompanyAcronym.ToString();
            string year = DateTime.UtcNow.Year.ToString();
            string month = DateTime.UtcNow.Month.ToString().PadLeft(2, '0');
            string count = m_appmodel.GetNextCounterValue().ToString().PadLeft(4, '0');
            record.VendorPatchName = companyname + '-' + year + '-' + month + '-' + count;
            record.CreatedByID = GetCurrentUserID();
            record.CreatedOn = DateTime.UtcNow;
            record.UpdatedByID = record.CreatedByID;
            record.UpdatedOn = record.CreatedOn;
            record.IsInitiated = false;
            m_dataContext.Table<Patch>().AddNewRecord(BuildPatch(record));
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        public int GetLastPatchViewID()
        {
            return m_dataContext.Connection.ExecuteScalar<int?>("SELECT IDENT_CURRENT('Patch')") ?? 0;
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        [RecordOperation(typeof(PatchView), RecordOperation.UpdateRecord)]
        public void UpdatePatchView(PatchView record)
        {
            record.UpdatedByID = GetCurrentUserID();
            record.UpdatedOn = DateTime.UtcNow;
            m_dataContext.Table<Patch>().UpdateRecord(BuildPatch(record));


        }

        private Patch BuildPatch(PatchView record)
        {
            Patch patch = new Patch();
            patch.ID = record.ID;
            patch.ParentID = record.ParentID;
            patch.VendorID = record.VendorID;
            patch.PlatformID = record.PlatformID;
            patch.PatchClassKey = record.PatchClassKey;
            patch.VendorPatchName = record.VendorPatchName;
            patch.VendorReleaseDate = record.VendorReleaseDate;
            patch.ImpactKey = record.ImpactKey;
            patch.Summary = record.Summary;
            patch.Detail = record.Detail;
            patch.Citations = record.Citations;
            patch.AlarmCriticalDays = record.AlarmCriticalDays;
            patch.CloseOutNotes = record.CloseOutNotes;
            patch.Vulnerability = record.Vulnerability;
            patch.EvaluationDeadline = record.EvaluationDeadline;
            patch.CreatedByID = record.CreatedByID;
            patch.CreatedOn = record.CreatedOn;
            patch.UpdatedOn = record.UpdatedOn;
            patch.UpdatedByID = record.UpdatedByID;
            patch.IsDeleted = record.IsDeleted;
            patch.IsInitiated = record.IsInitiated;
            patch.IsExpedited = record.IsExpedited;
            patch.Title = record.Title;
            patch.Link = record.Link;
            return patch;
        }

        #endregion


        #region [ PatchStatus Table Operations ]

        [RecordOperation(typeof(PatchStatus), RecordOperation.QueryRecordCount)]
        public int QueryPatchStatusCount(string filterText = "%")
        {
            return m_dataContext.Table<PatchStatus>().QueryRecordCount();
        }

        [RecordOperation(typeof(PatchStatus), RecordOperation.QueryRecords)]
        public IEnumerable<PatchStatus> QueryPatchStatus(int parentID, string filterText = "%")
        {
            return m_dataContext.Table<PatchStatus>().QueryRecords(restriction: new RecordRestriction("ID = {0}", parentID));
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        [RecordOperation(typeof(PatchStatus), RecordOperation.DeleteRecord)]
        public void DeletePatchStatus(int id)
        {
            m_dataContext.Table<PatchStatus>().DeleteRecord(id);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        [RecordOperation(typeof(PatchStatus), RecordOperation.CreateNewRecord)]
        public PatchStatus NewPatchStatus()
        {
            return new PatchStatus();
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        [RecordOperation(typeof(PatchStatus), RecordOperation.AddNewRecord)]
        public void AddNewPatchStatus(PatchStatus record)
        {
            record.CreatedOn = DateTime.UtcNow;
            record.CreatedByID = GetCurrentUserID();
            record.StatusChangeOn = record.CreatedOn;
            m_dataContext.Table<PatchStatus>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        [RecordOperation(typeof(PatchStatus), RecordOperation.UpdateRecord)]
        public void UpdatePatchStatus(PatchStatus record)
        {
            record.StatusChangeOn = DateTime.UtcNow;
            m_dataContext.Table<PatchStatus>().UpdateRecord(record);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        [RecordOperation(typeof(Vendor), RecordOperation.DeleteRecord)]
        public void UpdatePatchStatusKey(int id, int key)
        {
            // For Vendors, we only "mark" a record as deleted
            m_dataContext.Connection.ExecuteNonQuery("UPDATE PatchStatus SET PatchStatusKey={0} WHERE ID={1}",key, id);
        }

        #endregion

        #region [ ClosedPatch Table Operations ]

        [RecordOperation(typeof(ClosedPatch), RecordOperation.QueryRecordCount)]
        public int QueryClosedPatchCount(string filterText = "%")
        {
            return m_dataContext.Table<ClosedPatch>().QueryRecordCount();
        }

        [RecordOperation(typeof(ClosedPatch), RecordOperation.QueryRecords)]
        public IEnumerable<ClosedPatch> QueryClosedPatch(string sortField, bool ascending, int page, int pageSize, string filterText = "%")
        {
            return m_dataContext.Table<ClosedPatch>().QueryRecords(sortField, ascending, page, pageSize);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        public int GetLastClosedPatchID()
        {
            return m_dataContext.Connection.ExecuteScalar<int?>("SELECT IDENT_CURRENT('ClosedPatch')") ?? 0;
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        [RecordOperation(typeof(ClosedPatch), RecordOperation.DeleteRecord)]
        public void DeleteClosedPatch(int id)
        {
            m_dataContext.Table<ClosedPatch>().DeleteRecord(id);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        [RecordOperation(typeof(ClosedPatch), RecordOperation.CreateNewRecord)]
        public ClosedPatch NewClosedPatch()
        {
            return new ClosedPatch();
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        [RecordOperation(typeof(ClosedPatch), RecordOperation.AddNewRecord)]
        public void AddNewClosedPatch(ClosedPatch record)
        {
            m_dataContext.Table<ClosedPatch>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        [RecordOperation(typeof(ClosedPatch), RecordOperation.UpdateRecord)]
        public void UpdateClosedPatch(ClosedPatch record)
        {
            m_dataContext.Table<ClosedPatch>().UpdateRecord(record);
        }

        #endregion


        #region [ Vendor Table Operations ]

        [RecordOperation(typeof(Vendor), RecordOperation.QueryRecordCount)]
        public int QueryVendorCount(bool showDeleted, string filterText)
        {
            if (filterText == null) filterText = "%";
            else
            {
                // Build your filter string here!
                filterText += "%";
            }


            if (showDeleted)
                return m_dataContext.Table<Vendor>().QueryRecordCount(new RecordRestriction("Name LIKE {0}", filterText));

            return m_dataContext.Table<Vendor>().QueryRecordCount(new RecordRestriction("IsDeleted = 0 AND Name LIKE {0}", filterText));
        }

        [RecordOperation(typeof(Vendor), RecordOperation.QueryRecords)]
        public IEnumerable<Vendor> QueryVendors(bool showDeleted, string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            if (filterText == null) filterText = "%";
            else
            {
                // Build your filter string here!
                filterText += "%";
            }

            if (showDeleted)
                return m_dataContext.Table<Vendor>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("Name LIKE {0}", filterText));

            return m_dataContext.Table<Vendor>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("IsDeleted = 0 AND Name LIKE {0}", filterText));
        }

        public IEnumerable<Vendor> QueryVendorsNonPaged()
        {
           return m_dataContext.Table<Vendor>().QueryRecords("Name", new RecordRestriction("IsDeleted = 0"));
        }

        public IEnumerable<Vendor> QueryOneVendor(int id)
        {
           return m_dataContext.Table<Vendor>().QueryRecords(restriction: new RecordRestriction("ID = {0}", id));
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(Vendor), RecordOperation.DeleteRecord)]
        public void DeleteVendor(int id)
        {
            // For Vendors, we only "mark" a record as deleted
            Vendor v = m_dataContext.Table<Vendor>().LoadRecord(id);
            if (v.IsDeleted == false)
            {
                v.DeletedByID = GetCurrentUserID();
                v.DeletedON = DateTime.UtcNow;
                v.IsDeleted = true;
                m_dataContext.Table<Vendor>().UpdateRecord(v);
            }
        }

        [RecordOperation(typeof(Vendor), RecordOperation.CreateNewRecord)]
        public Vendor NewVendor()
        {
            return new Vendor();
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
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

        /// <summary>
        /// Filters Vendors by name with no limit on total returned records.
        /// </summary>
        /// <param name="searchText">Search text to lookup.</param>
        /// <param name="showDeleted">Show deleted or not</param>
        /// <param name="sortField">Field to sort query</param>
        /// <param name="ascending">Bool for ascending sort</param>
        /// <param name="page">Page to return</param>
        /// <param name="pageSize">Size of pages to return</param>
        /// <returns>Filtered results as Vendor Table records.</returns>
        public IEnumerable<Vendor> FilterVendors(string searchText, bool showDeleted, string sortField, bool ascending, int page, int pageSize)
        {
            if (showDeleted)
            {
                return m_dataContext
                    .Table<Vendor>()
                    .QueryRecords(sortField, ascending, page, pageSize)
                    .Where(record => (record?.Name?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0);

            }
            return m_dataContext
                .Table<Vendor>()
                .QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("IsDeleted = 0"))
                .Where(record => (record?.Name?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0);
        }


        /// <summary>
        /// Searches Vendors by name with no limit on total returned records.
        /// </summary>
        /// <param name="searchText">Search text to lookup.</param>
        /// <returns>Search results as "IDLabel" values - serialized as JSON [{ id: "value", label : "name" }, ...]; useful for dynamic lookup lists.</returns>
        public IEnumerable<IDLabel> SearchVendors(string searchText)
        {
            return SearchVendors(searchText, -1);
        }

        /// <summary>
        /// Searches Vendors by name limited to the specified number of records.
        /// </summary>
        /// <param name="searchText">Search text to lookup.</param>
        /// <param name="limit">Limit of number of record to return.</param>
        /// <returns>Search results as "IDLabel" values - serialized as JSON [{ id: "value", label : "name" }, ...]; useful for dynamic lookup lists.</returns>
        public IEnumerable<IDLabel> SearchVendors(string searchText, int limit)
        {
            if (limit < 1)
                return m_dataContext
                    .Table<Vendor>()
                    .QueryRecords()
                    .Where(record => (record?.Name?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0)
                    .Select(record => IDLabel.Create(record.ID.ToString(), record.Name));

            return m_dataContext
                .Table<Vendor>()
                .QueryRecords()
                .Where(record => (record?.Name?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0)
                .Take(limit)
                .Select(record => IDLabel.Create(record.ID.ToString(), record.Name));
        }
        #endregion

        #region [ Platform Table Operations ]

        [RecordOperation(typeof(Platform), RecordOperation.QueryRecordCount)]
        public int QueryPlatformCount(bool showDeleted, string filterText)
        {
            
            if (filterText == null) filterText = "%";
            else
            {
                // Build your filter string here!
                filterText += "%";
            }

            if (showDeleted)
                return m_dataContext.Table<Platform>().QueryRecordCount(new RecordRestriction("Name LIKE {0}", filterText));

            return m_dataContext.Table<Platform>().QueryRecordCount(new RecordRestriction("IsDeleted = 0 AND Name LIKE {0}", filterText));
        }

        [RecordOperation(typeof(Platform), RecordOperation.QueryRecords)]
        public IEnumerable<Platform> QueryPlatforms(bool showDeleted, string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            if (filterText == null) filterText = "%";
            else
            {
                // Build your filter string here!
                filterText += "%";
            }

            if (showDeleted)
                return m_dataContext.Table<Platform>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("Name LIKE {0}", filterText));

            return m_dataContext.Table<Platform>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("IsDeleted = 0 AND Name LIKE {0}", filterText));
        }

        public IEnumerable<Platform> QueryPlatformsByVendor(int parentID)
        {
            return m_dataContext.Table<Platform>().QueryRecords(restriction: new RecordRestriction("IsDeleted = 0 AND VendorID = {0}", parentID));
        }

    
        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(Platform), RecordOperation.DeleteRecord)]
        public void DeletePlatform(int id)
        {
            // For Platforms, we only "mark" a record as deleted
            Platform platform = m_dataContext.Table<Platform>().LoadRecord(id);
            if (platform.IsDeleted == false)
            {
                platform.DeletedByID = GetCurrentUserID();
                platform.DeletedON = DateTime.UtcNow;
                platform.IsDeleted = true;
                m_dataContext.Table<Platform>().UpdateRecord(platform);
            }
        }

        [RecordOperation(typeof(Platform), RecordOperation.CreateNewRecord)]
        public Platform NewPlatform()
        {
            return new Platform();
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        [RecordOperation(typeof(Platform), RecordOperation.AddNewRecord)]
        public void AddNewPlatform(Platform record)
        {
            record.CreatedByID = GetCurrentUserID();
            record.CreatedOn = DateTime.UtcNow;
            record.UpdatedByID = record.CreatedByID;
            record.UpdatedOn = record.CreatedOn;
            record.DatePlatformEnrolled = record.CreatedOn;
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
        /// Filters Platforms by name with no limit on total returned records.
        /// </summary>
        /// <param name="searchText">Search text to lookup.</param>
        /// <param name="showDeleted">Show deleted or not</param>
        /// <param name="sortField">Field to sort query</param>
        /// <param name="ascending">Bool for ascending sort</param>
        /// <param name="page">Page to return</param>
        /// <param name="pageSize">Size of pages to return</param>
        /// <returns>Filtered results as Vendor Table records.</returns>
        public IEnumerable<Platform> FilterPlatforms(string searchText, bool showDeleted, string sortField, bool ascending, int page, int pageSize)
        {
            if (showDeleted)
            {
                return m_dataContext
                    .Table<Platform>()
                    .QueryRecords(sortField, ascending, page, pageSize)
                    .Where(record => (record?.Name?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0);

            }
            return m_dataContext
                .Table<Platform>()
                .QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("IsDeleted = 0"))
                .Where(record => (record?.Name?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0);
        }

        /// <summary>
        /// Searches platforms by name with no limit on total returned records.
        /// </summary>
        /// <param name="searchText">Search text to lookup.</param>
        /// <returns>Search results as "IDLabel" values - serialized as JSON [{ id: "value", label : "name" }, ...]; useful for dynamic lookup lists.</returns>
        public IEnumerable<IDLabel> SearchPlatforms(string searchText)
        {
            return SearchPlatforms(searchText, -1);
        }

        /// <summary>
        /// Searches platforms by name limited to the specified number of records.
        /// </summary>
        /// <param name="searchText">Search text to lookup.</param>
        /// <param name="limit">Limit of number of record to return.</param>
        /// <returns>Search results as "IDLabel" values - serialized as JSON [{ id: "value", label : "name" }, ...]; useful for dynamic lookup lists.</returns>
        public IEnumerable<IDLabel> SearchPlatforms(string searchText, int limit)
        {
            if (limit < 1)
                return m_dataContext
                    .Table<Platform>()
                    .QueryRecords()
                    .Where(record => (record?.Name?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0 && record.IsDeleted == false)
                    .Select(record => IDLabel.Create(record.ID.ToString(), record.Name));

            return m_dataContext
                .Table<Platform>()
                .QueryRecords()
                .Where(record => (record?.Name?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0 && record.IsDeleted == false)
                .Take(limit)
                .Select(record => IDLabel.Create(record.ID.ToString(), record.Name));
        }

        #endregion

        #region [ PlatformView Table Operations ]

        [RecordOperation(typeof(PlatformView), RecordOperation.QueryRecordCount)]
        public int QueryPlatformViewCount(bool showDeleted, string filterText)
        {
            string productFilter = "%";
            string vendorFilter = "%";
            if (filterText != "%")
            {
                string[] filters = filterText.Split(';');
                if (filters.Length == 2)
                {
                    productFilter += filters[0] += '%';
                    vendorFilter += filters[1] += '%';
                }
            }

            if (showDeleted)
                return m_dataContext.Table<PlatformView>().QueryRecordCount(new RecordRestriction("Name LIKE {0} AND VendorName LIKE {1}", productFilter, vendorFilter));
            return m_dataContext.Table<PlatformView>().QueryRecordCount(new RecordRestriction("IsDeleted = 0 AND Name LIKE {0} AND VendorName LIKE {1}", productFilter, vendorFilter));
        }

        [RecordOperation(typeof(PlatformView), RecordOperation.QueryRecords)]
        public IEnumerable<PlatformView> QueryPlatformViews(bool showDeleted, string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            string productFilter = "%";
            string vendorFilter = "%";
            if (filterText != "%")
            {
                string[] filters = filterText.Split(';');
                if (filters.Length == 2)
                {
                    productFilter += filters[0] += '%';
                    vendorFilter += filters[1] += '%';
                }
            }



            if (showDeleted)
                return m_dataContext.Table<PlatformView>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("Name LIKE {0} AND VendorName LIKE {1}", productFilter, vendorFilter));
            return m_dataContext.Table<PlatformView>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("IsDeleted = 0 AND Name LIKE {0} AND VendorName LIKE {1}", productFilter, vendorFilter));
        }

        public IEnumerable<PlatformView> QueryPlatformViewsByVendor(int parentID)
        {
            return m_dataContext.Table<PlatformView>().QueryRecords(restriction: new RecordRestriction("IsDeleted = 0 AND VendorID = {0}", parentID));
        }


        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(PlatformView), RecordOperation.DeleteRecord)]
        public void DeletePlatformView(int id)
        {
            // For PlatformViews, we only "mark" a record as deleted
            PlatformView PlatformView = m_dataContext.Table<PlatformView>().LoadRecord(id);
            if (PlatformView.IsDeleted == false)
            {
                PlatformView.DeletedByID = GetCurrentUserID();
                PlatformView.DeletedON = DateTime.UtcNow;
                PlatformView.IsDeleted = true;
                m_dataContext.Table<Platform>().UpdateRecord(BuildPlatform(PlatformView));
            }
        }

        [RecordOperation(typeof(PlatformView), RecordOperation.CreateNewRecord)]
        public PlatformView NewPlatformView()
        {
            return new PlatformView();
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        [RecordOperation(typeof(PlatformView), RecordOperation.AddNewRecord)]
        public void AddNewPlatformView(PlatformView record)
        {
            record.CreatedByID = GetCurrentUserID();
            record.CreatedOn = DateTime.UtcNow;
            record.UpdatedByID = record.CreatedByID;
            record.UpdatedOn = record.CreatedOn;
            record.DatePlatformEnrolled = record.CreatedOn;
            m_dataContext.Table<Platform>().AddNewRecord(BuildPlatform(record));
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(PlatformView), RecordOperation.UpdateRecord)]
        public void UpdatePlatformView(PlatformView record)
        {
            record.UpdatedByID = GetCurrentUserID();
            record.UpdatedOn = DateTime.UtcNow;
            m_dataContext.Table<Platform>().UpdateRecord(BuildPlatform(record));
        }


        /// <summary>
        /// Filters PlatformViews by name with no limit on total returned records.
        /// </summary>
        /// <param name="searchText">Search text to lookup.</param>
        /// <param name="showDeleted">Show deleted or not</param>
        /// <param name="sortField">Field to sort query</param>
        /// <param name="ascending">Bool for ascending sort</param>
        /// <param name="page">Page to return</param>
        /// <param name="pageSize">Size of pages to return</param>
        /// <returns>Filtered results as Vendor Table records.</returns>
        public IEnumerable<PlatformView> FilterPlatformViews(string searchText, bool showDeleted, string sortField, bool ascending, int page, int pageSize)
        {
            if (showDeleted)
            {
                return m_dataContext
                    .Table<PlatformView>()
                    .QueryRecords(sortField, ascending, page, pageSize)
                    .Where(record => (record?.Name?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0);

            }
            return m_dataContext
                .Table<PlatformView>()
                .QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("IsDeleted = 0"))
                .Where(record => (record?.Name?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0);
        }

        /// <summary>
        /// Searches PlatformViews by name with no limit on total returned records.
        /// </summary>
        /// <param name="searchText">Search text to lookup.</param>
        /// <returns>Search results as "IDLabel" values - serialized as JSON [{ id: "value", label : "name" }, ...]; useful for dynamic lookup lists.</returns>
        public IEnumerable<IDLabel> SearchPlatformViews(string searchText)
        {
            return SearchPlatformViews(searchText, -1);
        }

        /// <summary>
        /// Searches PlatformViews by name limited to the specified number of records.
        /// </summary>
        /// <param name="searchText">Search text to lookup.</param>
        /// <param name="limit">Limit of number of record to return.</param>
        /// <returns>Search results as "IDLabel" values - serialized as JSON [{ id: "value", label : "name" }, ...]; useful for dynamic lookup lists.</returns>
        public IEnumerable<IDLabel> SearchPlatformViews(string searchText, int limit)
        {
            if (limit < 1)
                return m_dataContext
                    .Table<PlatformView>()
                    .QueryRecords()
                    .Where(record => (record?.Name?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0 && record.IsDeleted == false)
                    .Select(record => IDLabel.Create(record.ID.ToString(), record.Name));

            return m_dataContext
                .Table<PlatformView>()
                .QueryRecords()
                .Where(record => (record?.Name?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0 && record.IsDeleted == false)
                .Take(limit)
                .Select(record => IDLabel.Create(record.ID.ToString(), record.Name));
        }

        public Platform BuildPlatform(PlatformView record)
        {
            Platform platform = new Platform();
            platform.ID = record.ID;
            platform.VendorID = record.VendorID;
            platform.Name = record.Name;
            platform.IsDeleted = record.IsDeleted;
            platform.UpdatedOn = record.UpdatedOn;
            platform.UpdatedByID = record.UpdatedByID;
            platform.CreatedOn = record.CreatedOn;
            platform.CreatedByID = record.CreatedByID;
            platform.Version = record.Version;
            platform.DatePlatformEnrolled = record.DatePlatformEnrolled;
            platform.DatePlatformRetired = record.DatePlatformRetired;
            platform.Notes = record.Notes;
            platform.DeletedON = record.DeletedON;
            platform.DeletedByID = record.DeletedByID;
            platform.Abbreviation = record.Abbreviation;

            return platform;
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

        #region [ UserAccountPlatformDetail Table Operations ]

        public IEnumerable<UserAccountPlatformDetail> QueryPlatformSMEs(int platformID)
        {
            return m_dataContext.Table<UserAccountPlatformDetail>().QueryRecords(restriction: new RecordRestriction("PlatformID = {0}", platformID));
        }

        public int QueryPlatformSMECount(int platformID)
        {
            return m_dataContext.Table<UserAccountPlatformDetail>().QueryRecordCount(restriction: new RecordRestriction("PlatformID = {0}", platformID));
        }

        #endregion


        #region [ UserAccountPlatformBusinessUnitDetail Table Operations ]

        public int QueryUserAccountPlatformBusinessUnitDetailCount(int platformID)
        {
            return m_dataContext.Table<UserAccountPlatformBusinessUnitDetail>().QueryRecordCount(new RecordRestriction("PlatformID = {0}", platformID));
        }

        public IEnumerable<UserAccountPlatformBusinessUnitDetail> QueryUserAccountPlatformBusinessUnitDetails(int platformID)
        {
            return m_dataContext.Table<UserAccountPlatformBusinessUnitDetail>().QueryRecords(restriction: new RecordRestriction("PlatformID = {0}", platformID));
        }

        #endregion


        #region [ BusinessUnit Table Operations ]

        [RecordOperation(typeof(BusinessUnit), RecordOperation.QueryRecordCount)]
        public int QueryBusinessUnitCount(bool showDeleted, string filterText = "%")
        {
            if (filterText == null) filterText = "%";
            else
            {
                // Build your filter string here!
                filterText += "%";
            }

            if (showDeleted)
                return m_dataContext.Table<BusinessUnit>().QueryRecordCount(new RecordRestriction("Name LIKE {0}", filterText));

            return m_dataContext.Table<BusinessUnit>().QueryRecordCount(new RecordRestriction("IsDeleted = 0 AND Name LIKE {0}", filterText));
        }

        [RecordOperation(typeof(BusinessUnit), RecordOperation.QueryRecords)]
        public IEnumerable<BusinessUnit> QueryBusinessUnits(bool showDeleted, string sortField, bool ascending, int page, int pageSize, string filterText = "%")
        {
            if (filterText == null) filterText = "%";
            else
            {
                // Build your filter string here!
                filterText += "%";
            }

            if (showDeleted)
                return m_dataContext.Table<BusinessUnit>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("Name LIKE {0}", filterText));

            return m_dataContext.Table<BusinessUnit>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("IsDeleted = 0 AND Name LIKE {0}", filterText));
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(BusinessUnit), RecordOperation.DeleteRecord)]
        public void DeleteBusinessUnit(int id)
        {
            // For BusinessUnits, we only "mark" a record as deleted
            BusinessUnit bu = m_dataContext.Table<BusinessUnit>().LoadRecord(id);
            if (bu.IsDeleted == false)
            {
                bu.DeletedByID = GetCurrentUserID();
                bu.DeletedON = DateTime.UtcNow;
                bu.IsDeleted = true;
                m_dataContext.Table<BusinessUnit>().UpdateRecord(bu);
            }
        }


        [RecordOperation(typeof(BusinessUnit), RecordOperation.CreateNewRecord)]
        public BusinessUnit NewBusinessUnit()
        {
            return new BusinessUnit();
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
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

        /// <summary>
        /// Filters BUs by name with no limit on total returned records.
        /// </summary>
        /// <param name="searchText">Search text to lookup.</param>
        /// <param name="showDeleted">Show deleted or not</param>
        /// <param name="sortField">Field to sort query</param>
        /// <param name="ascending">Bool for ascending sort</param>
        /// <param name="page">Page to return</param>
        /// <param name="pageSize">Size of pages to return</param>
        /// <returns>Filtered results as Vendor Table records.</returns>
        public IEnumerable<BusinessUnit> FilterBusinessUnits(string searchText, bool showDeleted, string sortField, bool ascending, int page, int pageSize)
        {
            if (showDeleted)
            {
                return m_dataContext
                    .Table<BusinessUnit>()
                    .QueryRecords(sortField, ascending, page, pageSize)
                    .Where(record => (record?.Name?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0);

            }
            return m_dataContext
                .Table<BusinessUnit>()
                .QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("IsDeleted = 0"))
                .Where(record => (record?.Name?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) ?? -1) >= 0);
        }
        #endregion

        #region [ BusinessUnitUserAccount Table Operations ]

        public int QueryBusinessUnitUserAccountCount(int businessUnitID)
        {
            return m_dataContext.Table<BusinessUnitUserAccount>().QueryRecordCount(new RecordRestriction("BusinessUnitID = {0}", businessUnitID));
        }

        public IEnumerable<BusinessUnitUserAccountDetail> QueryBusinessUnitUserAccounts(int businessUnitID, bool ascending, int page, int pageSize)
        {
            return m_dataContext.Table<BusinessUnitUserAccountDetail>().QueryRecords("UserAccountName", true, page, pageSize, new RecordRestriction("BusinessUnitID = {0}", businessUnitID)).
                Select(account =>
                {
                    account.UserAccountName = UserInfo.SIDToAccountName(account.UserAccountName);
                    return account;
                });

            // The following will properly return a list sorted by user name, but on systems with slow AD response,
            // resolution of a large list of users could be slow - note that entire list needs to be resolved at
            // each query to ensure proper sort. Caching full list at client side might help...

            //IEnumerable<BusinessUnitUserAccountDetail> resolvedAccountRecords = m_dataContext.Table<BusinessUnitUserAccountDetail>().
            //    QueryRecords(restriction: new RecordRestriction("BusinessUnitID = {0}", businessUnitID)).
            //    Select(account =>
            //    {
            //        account.UserAccountName = UserInfo.SIDToAccountName(account.UserAccountName);
            //        return account;
            //    });

            //if (ascending)
            //    resolvedAccountRecords = resolvedAccountRecords.OrderBy(key => key.UserAccountName);
            //else
            //    resolvedAccountRecords = resolvedAccountRecords.OrderByDescending(key => key.UserAccountName);

            //return resolvedAccountRecords.ToPagedList(page, pageSize);
        }

        public int QueryUserBusinessUnitCount(Guid userID)
        {
            return m_dataContext.Table<BusinessUnitUserAccount>().QueryRecordCount(new RecordRestriction("UserAccountID = {0}", userID));
        }

        public IEnumerable<BusinessUnitUserAccount> QueryUserBusinessUnits(Guid userID)
        {
            return m_dataContext.Table<BusinessUnitUserAccount>().QueryRecords(restriction: new RecordRestriction("UserAccountID = {0}", userID));
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

        #region [ PatchUserAccountPlatformBusinessUnitUserAccountView Table Operations ]

        public int QueryPatchUserAccountPlatformBusinessUnitUserAccountViewCount(int platformID)
        {
            return m_dataContext.Table<PatchUserAccountPlatformBusinessUnitUserAccountView>().QueryRecordCount(new RecordRestriction("PlatformID = {0}", platformID));
        }

        public IEnumerable<PatchUserAccountPlatformBusinessUnitUserAccountView>
            QueryPatchUserAccountPlatformBusinessUnitUserAccountViews(int platformID)
        {
            return m_dataContext.Table<PatchUserAccountPlatformBusinessUnitUserAccountView>().QueryRecords("PlatformID", new RecordRestriction("PlatformID = {0}", platformID));
        }

        #endregion

        #region [ Document Table Operations ]

        [RecordOperation(typeof(Document), RecordOperation.QueryRecordCount)]
        public int QueryDocumentCount(string filterText = "%")
        {
            return m_dataContext.Table<Document>().QueryRecordCount();
        }

        //[RecordOperation(typeof(Document), RecordOperation.QueryRecords)]
        //public IEnumerable<Document> QueryDocuments(int sourceID, string sourceField, string tableName)
        //{
        //    IEnumerable<int> documentIDs =
        //        m_dataContext.Connection.RetrieveData($"SELECT DocumentID FROM {tableName} WHERE {sourceField} = {{0}}", sourceID)
        //            .AsEnumerable()
        //            .Select(row => row.ConvertField<int>("DocumentID", 0));            

        //    return m_dataContext.Table<Document>().QueryRecords("Filename", new RecordRestriction($"ID IN ({string.Join(", ", documentIDs)})"));
        //}

        [AuthorizeHubRole("Administrator, Owner, PIC, SME, BUC")]
        [RecordOperation(typeof(Document), RecordOperation.DeleteRecord)]
        public void DeleteDocument(int id, string filterText = "%")
        {
            m_dataContext.Table<Document>().DeleteRecord(id);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME, BUC")]
        [RecordOperation(typeof(Document), RecordOperation.CreateNewRecord)]
        public Document NewDocument()
        {
            return new Document();
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME, BUC")]
        [RecordOperation(typeof(Document), RecordOperation.AddNewRecord)]
        public void AddNewDocument(Document record)
        {
            record.CreatedOn = DateTime.UtcNow;
            m_dataContext.Table<Document>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME, BUC")]
        [RecordOperation(typeof(Document), RecordOperation.UpdateRecord)]
        public void UpdateDocument(Document record)
        {
            m_dataContext.Table<Document>().UpdateRecord(record);
        }

        #endregion

        #region [ DocumentDetail View Operations ]

        [RecordOperation(typeof(DocumentDetail), RecordOperation.QueryRecordCount)]
        public int QueryDocumentDetailCount(string sourceTable, int sourceID, string filterText)
        {
            return m_dataContext.Table<DocumentDetail>().QueryRecordCount(new RecordRestriction("SourceTable = {0} AND SourceID = {1}", sourceTable, sourceID));
        }

        [RecordOperation(typeof(DocumentDetail), RecordOperation.QueryRecords)]
        public IEnumerable<DocumentDetail> QueryDocumentDetailResults(string sourceTable, int sourceID, string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            return m_dataContext.Table<DocumentDetail>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("SourceTable = {0} AND SourceID = {1}", sourceTable, sourceID));
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME, BUC")]
        [RecordOperation(typeof(DocumentDetail), RecordOperation.DeleteRecord)]
        public void DeleteDocumentDetail(string sourceTable, int sourceID, int documentID)
        {
            m_dataContext.Connection.ExecuteNonQuery($"DELETE FROM {sourceTable}Document WHERE {sourceTable}ID = {{0}} AND DocumentID = {{1}}", sourceID, documentID);
            m_dataContext.Table<Document>().DeleteRecord(documentID);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME, BUC")]
        [RecordOperation(typeof(DocumentDetail), RecordOperation.CreateNewRecord)]
        public DocumentDetail NewDocumentDetail()
        {
            return new DocumentDetail();
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME, BUC")]
        [RecordOperation(typeof(DocumentDetail), RecordOperation.AddNewRecord)]
        public void AddNewDocumentDetail(DocumentDetail record)
        {
            // Stub function exists to assign rights to document related UI operations
            throw new NotImplementedException();
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME, BUC")]
        [RecordOperation(typeof(DocumentDetail), RecordOperation.UpdateRecord)]
        public void UpdateDocumentDetail(DocumentDetail record)
        {
            // Stub function exists to assign rights to document related UI operations
            throw new NotImplementedException();
        }

        #endregion

        #region [ PatchDocument Table Operations ]

        [AuthorizeHubRole("Administrator, Owner, PIC, SME, BUC, Viewer")]
        [RecordOperation(typeof(PatchDocument), RecordOperation.UpdateRecord)]
        public void UpdatePatchDocument(PatchDocument record)
        {
            // Stub function exists to assign rights to file upload operations
            throw new NotImplementedException();
        }

        #endregion

        #region [ InstallDocument Table Operations ]

        [AuthorizeHubRole("Administrator, Owner, PIC, SME, BUC")]
        [RecordOperation(typeof(InstallDocument), RecordOperation.UpdateRecord)]
        public void UpdateInstallDocument(InstallDocument record)
        {
            // Stub function exists to assign rights to file upload operations
            throw new NotImplementedException();
        }

        #endregion

        #region [ AssessmentDocument Table Operations ]

        [AuthorizeHubRole("Administrator, Owner, PIC, SME, BUC")]
        [RecordOperation(typeof(AssessmentDocument), RecordOperation.UpdateRecord)]
        public void UpdateAssessmentDocument(AssessmentDocument record)
        {
            // Stub function exists to assign rights to file upload operations
            throw new NotImplementedException();
        }

        #endregion

        #region [ ClosedPatchDocument Table Operations ]

        [AuthorizeHubRole("Administrator, Owner, PIC, SME, BUC")]
        [RecordOperation(typeof(ClosedPatchDocument), RecordOperation.UpdateRecord)]
        public void UpdateClosedPatchDocument(ClosedPatchDocument record)
        {
            // Stub function exists to assign rights to file upload operations
            throw new NotImplementedException();
        }

        #endregion

        #region [ DiscoveryResultDocument Table Operations ]

        [AuthorizeHubRole("Administrator, Owner, PIC, SME, BUC")]
        [RecordOperation(typeof(DiscoveryResultDocument), RecordOperation.UpdateRecord)]
        public void UpdateDiscoveryResultDocument(DiscoveryResultDocument record)
        {
            // Stub function exists to assign rights to file upload operations
            throw new NotImplementedException();
        }

        #endregion

        #region [ Page Table Operations ]

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Page), RecordOperation.QueryRecordCount)]
        public int QueryPageCount(string filterText = "%")
        {
            return m_dataContext.Table<Page>().QueryRecordCount();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Page), RecordOperation.QueryRecords)]
        public IEnumerable<Page> QueryPages(string sortField, bool ascending, int page, int pageSize, string filterText = "%")
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
        public int QueryMenuCount(string filterText = "%")
        {
            return m_dataContext.Table<Menu>().QueryRecordCount();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Menu), RecordOperation.QueryRecords)]
        public IEnumerable<Menu> QueryMenus(string sortField, bool ascending, int page, int pageSize, string filterText = "%")
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
        public int QueryMenuItemCount(int parentID, string filterText = "%")
        {
            return m_dataContext.Table<MenuItem>().QueryRecordCount(new RecordRestriction("MenuID = {0}", parentID));
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(MenuItem), RecordOperation.QueryRecords)]
        public IEnumerable<MenuItem> QueryMenuItems(int parentID, string sortField, bool ascending, int page, int pageSize, string filterText = "%")
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
        public int QueryValueListGroupCount(string filterText = "%")
        {
            return m_dataContext.Table<ValueListGroup>().QueryRecordCount();
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueListGroup), RecordOperation.QueryRecords)]
        public IEnumerable<ValueListGroup> QueryValueListGroups(string sortField, bool ascending, int page, int pageSize, string filterText = "%")
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
        public int QueryValueListCount(int parentID, string filterText = "%")
        {
            return m_dataContext.Table<ValueList>().QueryRecordCount(new RecordRestriction("GroupID = {0}", parentID));
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ValueList), RecordOperation.QueryRecords)]
        public IEnumerable<ValueList> QueryValueListItems(int parentID, string sortField, bool ascending, int page, int pageSize, string filterText = "%")
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

        [RecordOperation(typeof(Assessment), RecordOperation.QueryRecordCount)]
        public int QueryAssessmentCount(string filterText = "%")
        {
            return m_dataContext.Table<Assessment>().QueryRecordCount();
        }

        [RecordOperation(typeof(Assessment), RecordOperation.QueryRecords)]
        public IEnumerable<Assessment> QueryAssessments(string sortField, bool ascending, int page, int pageSize, string filterText = "%")
        {
            return m_dataContext.Table<Assessment>().QueryRecords(sortField, ascending, page, pageSize);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        public int GetLastAssessmentID()
        {
            return m_dataContext.Connection.ExecuteScalar<int?>("SELECT IDENT_CURRENT('Assessment')") ?? 0;
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        [RecordOperation(typeof(Assessment), RecordOperation.DeleteRecord)]
        public void DeleteAssessment(int id)
        {
            m_dataContext.Table<Assessment>().DeleteRecord(id);
        }
        

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        [RecordOperation(typeof(Assessment), RecordOperation.CreateNewRecord)]
        public Assessment NewAssessment()
        {
            return new Assessment();
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        [RecordOperation(typeof(Assessment), RecordOperation.AddNewRecord)]
        public void AddNewAssessment(Assessment record)
        {

            record.CreatedByID = GetCurrentUserID();
            record.CreatedOn = DateTime.UtcNow;
            record.UpdatedByID = GetCurrentUserID();
            record.UpdatedOn = DateTime.UtcNow;
            record.IsAssessed = true;
            m_dataContext.Table<Assessment>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        [RecordOperation(typeof(Assessment), RecordOperation.UpdateRecord)]
        public void UpdateAssessment(Assessment record)
        {
            record.UpdatedByID = GetCurrentUserID();
            record.UpdatedOn = DateTime.UtcNow;
            m_dataContext.Table<Assessment>().UpdateRecord(record);
        }

        #endregion

        #region [Install Table Operations]

        [RecordOperation(typeof(Install), RecordOperation.QueryRecordCount)]
        public int QueryInstallCount(string filterText = "%")
        {
            return m_dataContext.Table<Install>().QueryRecordCount( new RecordRestriction("IsInstalled = 0"));
        }

        [RecordOperation(typeof(Install), RecordOperation.QueryRecords)]
        public IEnumerable<Install> QueryInstalls(string sortField, bool ascending, int page, int pageSize, string filterText = "%")
        {
            return m_dataContext.Table<Install>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("IsInstalled = 0"));
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        public int GetLastInstallID()
        {
            return m_dataContext.Connection.ExecuteScalar<int?>("SELECT IDENT_CURRENT('Install')") ?? 0;
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(Install), RecordOperation.DeleteRecord)]
        public void DeleteInstall(int id)
        {
            m_dataContext.Table<Install>().DeleteRecord(id);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        [RecordOperation(typeof(Install), RecordOperation.CreateNewRecord)]
        public Install NewInstall()
        {
            return new Install();
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        [RecordOperation(typeof(Install), RecordOperation.AddNewRecord)]
        public void AddNewInstall(Install record)
        {
            record.CreatedByID = GetCurrentUserID();
            record.CreatedOn = DateTime.UtcNow;
            record.UpdatedByID = GetCurrentUserID();
            record.UpdatedOn = DateTime.UtcNow;
            record.IsInstalled = false;
            m_dataContext.Table<Install>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        [RecordOperation(typeof(Install), RecordOperation.UpdateRecord)]
        public void UpdateInstall(Install record)
        {
            record.UpdatedByID = GetCurrentUserID();
            record.UpdatedOn = DateTime.UtcNow;
            m_dataContext.Table<Install>().UpdateRecord(record);
        }

        #endregion

        #region [MitigationPlan Table Operations]

        [RecordOperation(typeof(MitigationPlan), RecordOperation.QueryRecordCount)]
        public int QueryMitigationPlanCount(bool showDeleted, string filterText = "%")
        {
            if (showDeleted)
                return m_dataContext.Table<MitigationPlan>().QueryRecordCount();

            return m_dataContext.Table<MitigationPlan>().QueryRecordCount(new RecordRestriction("IsDeleted = 0 AND IsMitigated = 0"));
        }

        [RecordOperation(typeof(MitigationPlan), RecordOperation.QueryRecords)]
        public IEnumerable<MitigationPlan> QueryMitigationPlans(bool showDeleted, string sortField, bool ascending, int page, int pageSize, string filterText = "%")
        {
            if (showDeleted)
                return m_dataContext.Table<MitigationPlan>().QueryRecords(sortField, ascending, page, pageSize);

            return m_dataContext.Table<MitigationPlan>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("IsDeleted = 0 AND IsMitigated = 0"));
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        public int GetLastMitigationPlanID()
        {
            return m_dataContext.Connection.ExecuteScalar<int?>("SELECT IDENT_CURRENT('MitigationPlan')") ?? 0;
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        public MitigationPlan GetMitigationPlan(int id)
        {
            return m_dataContext.Table<MitigationPlan>().LoadRecord(id);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        [RecordOperation(typeof(MitigationPlan), RecordOperation.DeleteRecord)]
        public void DeleteMitigationPlan(int id)
        {
            // For MitigationPlanes, we only "mark" a record as deleted
            MitigationPlan mp = m_dataContext.Table<MitigationPlan>().LoadRecord(id);
            if(mp.IsDeleted == false)
            {
                mp.DeletedByID = GetCurrentUserID();
                mp.DeletedON = DateTime.UtcNow;
                mp.IsDeleted = true;
                m_dataContext.Table<MitigationPlan>().UpdateRecord(mp);
            }
            
        }

        [RecordOperation(typeof(MitigationPlan), RecordOperation.CreateNewRecord)]
        public MitigationPlan NewMitigationPlan()
        {
            return new MitigationPlan();
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        [RecordOperation(typeof(MitigationPlan), RecordOperation.AddNewRecord)]
        public void AddNewMitigationPlan(MitigationPlan record)
        {
            record.CreatedByID = GetCurrentUserID();
            record.CreatedOn = DateTime.UtcNow;
            record.UpdatedByID = record.CreatedByID;
            record.UpdatedOn = record.CreatedOn;
            record.IsMitigated = false;
            m_dataContext.Table<MitigationPlan>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC, SME")]
        [RecordOperation(typeof(MitigationPlan), RecordOperation.UpdateRecord)]
        public void UpdateMitigationPlan(MitigationPlan record)
        {
            record.UpdatedByID = GetCurrentUserID();
            record.UpdatedOn = DateTime.UtcNow;
            m_dataContext.Table<MitigationPlan>().UpdateRecord(record);
        }

        #endregion

        #region [PatchPatchStatusDetail Table Operations]

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(PatchPatchStatusDetail), RecordOperation.QueryRecordCount)]
        public int QueryPatchPatchStatusDetailCount(string filterText)
        {

            Dictionary<string, string> parameters = filterText.ParseKeyValuePairs();

            string setting;
            int action;
            string filter;

            if (!parameters.TryGetValue("Action", out setting) || !int.TryParse(setting, out action))
                action = 0;

            parameters.TryGetValue("Filter", out filter);

            if (filter == null) filter = "%";

            else if(action == 1)
            {
                // Do this (action 1)
                return m_dataContext.Table<MyAssessmentsView>().QueryRecordCount(new RecordRestriction("UserAccountID = {0}", GetCurrentUserID()));
            }
            else
            {
                // Do that (action 0)
                // Build your filter string here!
                filter += "%";
            }

            return m_dataContext.Table<PatchPatchStatusDetail>().QueryRecordCount(new RecordRestriction("BusinessUnitID LIKE {0}", filter));
        }

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(PatchPatchStatusDetail), RecordOperation.QueryRecords)]
        public IEnumerable<PatchPatchStatusDetail> QueryPatchPatchStatusDetails(string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            Dictionary<string, string> parameters = filterText.ParseKeyValuePairs();

            string setting;
            int action;
            string filter;

            if (!parameters.TryGetValue("Action", out setting) || !int.TryParse(setting, out action))
                action = 0;

            parameters.TryGetValue("Filter", out filter);

            if (filter == null) filter = "%";

            else if (action == 1)
            {
                // Do this (action 1)
                return m_dataContext.Table<MyAssessmentsView>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("UserAccountID = {0}", GetCurrentUserID()));

            }
            else
            {
                // Do that (action 0)
                // Build your filter string here!
                filter += "%";
            }

            return m_dataContext.Table<PatchPatchStatusDetail>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("BusinessUnitID LIKE {0}", filter));
        }

        [AuthorizeHubRole("Administrator, BUC, PIC")]
        [RecordOperation(typeof(PatchPatchStatusDetail), RecordOperation.DeleteRecord)]
        public void DeletePatchPatchStatus(int id)
        {
            m_dataContext.Table<PatchStatus>().DeleteRecord(id);
        }
        [RecordOperation(typeof(PatchPatchStatusDetail), RecordOperation.CreateNewRecord)]
        public PatchPatchStatusDetail NewPatchPatchStatusDetail()
        {
            return new PatchPatchStatusDetail();
        }

        [AuthorizeHubRole("Administrator, Owner, SME")]
        [RecordOperation(typeof(PatchPatchStatusDetail), RecordOperation.AddNewRecord)]
        public void AddNewPatchPatchStatusDetailInstall(PatchPatchStatusDetail record)
        {
            Assessment result = DeriveAssessment(record);
            result.CreatedByID = GetCurrentUserID();
            result.CreatedOn = DateTime.UtcNow;
            m_dataContext.Table<Assessment>().AddNewRecord(result);
        }

        [AuthorizeHubRole("Administrator, Owner, SME")]
        [RecordOperation(typeof(PatchPatchStatusDetail), RecordOperation.UpdateRecord)]
        public void UpdatePatchPatchStatusDetailInstallTable(PatchPatchStatusDetail record)
        {
            m_dataContext.Table<Assessment>().UpdateRecord(DeriveAssessment(record));
        }

        private Assessment DeriveAssessment(PatchPatchStatusDetail record)
        {
            return new Assessment()
            {
                PatchStatusID = record.PatchStatusID,
                AssessmentResultKey = record.ImpactKey,
                Details = record.Detail,
                CreatedOn = DateTime.UtcNow,
                CreatedByID = GetCurrentUserID(),
                UpdatedOn = DateTime.UtcNow,
                UpdatedByID = GetCurrentUserID(),
                IsAssessed = false

            };
        }

        #endregion

        #region [MyAssessmentsView Table Operations]

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(MyAssessmentsView), RecordOperation.QueryRecordCount)]
        public int QueryMyAssessmentsViewCount(string filterText)
        {
            if (filterText == null) filterText = "%";
            else
            {
                // Build your filter string here!
                filterText += "%";
            }

            return m_dataContext.Table<MyAssessmentsView>().QueryRecordCount(new RecordRestriction("UserAccountID = {0}", GetCurrentUserID()));
        }

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(MyAssessmentsView), RecordOperation.QueryRecords)]
        public IEnumerable<MyAssessmentsView> QueryMyAssessmentsViews(string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            if (filterText == null) filterText = "%";
            else
            {
                // Build your filter string here!
                filterText += "%";
            }

            return m_dataContext.Table<MyAssessmentsView>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("UserAccountID = {0}", GetCurrentUserID()));
        }

        [RecordOperation(typeof(MyAssessmentsView), RecordOperation.CreateNewRecord)]
        public MyAssessmentsView NewMyAssessmentsView()
        {
            return new MyAssessmentsView();
        }

        [AuthorizeHubRole("Administrator, Owner, PIC")]
        [RecordOperation(typeof(MyAssessmentsView), RecordOperation.AddNewRecord)]
        public void AddNewMyAssessmentsViewInstall(MyAssessmentsView record)
        {
            Assessment result = DeriveAssessment(record);
            result.CreatedByID = GetCurrentUserID();
            result.CreatedOn = DateTime.UtcNow;
            m_dataContext.Table<Assessment>().AddNewRecord(result);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC")]
        [RecordOperation(typeof(MyAssessmentsView), RecordOperation.UpdateRecord)]
        public void UpdateMyAssessmentsViewInstallTable(MyAssessmentsView record)
        {
            m_dataContext.Table<Assessment>().UpdateRecord(DeriveAssessment(record));
        }

        private Assessment DeriveAssessment(MyAssessmentsView record)
        {
            return new Assessment()
            {
                PatchStatusID = record.PatchStatusID,
                AssessmentResultKey = record.ImpactKey,
                Details = record.Detail,
                CreatedOn = DateTime.UtcNow,
                CreatedByID = GetCurrentUserID(),
                UpdatedOn = DateTime.UtcNow,
                UpdatedByID = GetCurrentUserID(),
                IsAssessed = false

            };
        }

        #endregion


        #region [PatchStatusAssessmentDetail Table Operations]

        public int QueryPatchStatusAssessmentDetailCount()
        {
            return m_dataContext.Table<PatchStatusAssessmentDetail>().QueryRecordCount();
        }

        public IEnumerable<PatchStatusAssessmentDetail> QueryPatchStatusAssessmentDetails()
        {
            return m_dataContext.Table<PatchStatusAssessmentDetail>().QueryRecords("PatchMnemonic");
        }

        public PatchStatusAssessmentDetail NewPatchStatusAssessmentDetail()
        {
            return new PatchStatusAssessmentDetail();
        }

        #endregion

        #region [ LatestVendorDiscoveryResult View Operations ]

        [RecordOperation(typeof(LatestVendorDiscoveryResult), RecordOperation.QueryRecordCount)]
        public int QueryLatestVendorDiscoveryResultCount(bool showDeleted, string filterText)
        {
            if (filterText == null) filterText = "%";
            else
            {
                // Build your filter string here!
                filterText += "%";
            }

            if (showDeleted)
                return m_dataContext.Table<LatestVendorDiscoveryResult>().QueryRecordCount(new RecordRestriction("Abbreviation LIKE {0}", filterText));

            return m_dataContext.Table<LatestVendorDiscoveryResult>().QueryRecordCount(new RecordRestriction("IsDeleted = 0 AND Abbreviation LIKE {0}", filterText));
        }

        [RecordOperation(typeof(LatestVendorDiscoveryResult), RecordOperation.QueryRecords)]
        public IEnumerable<LatestVendorDiscoveryResult> QueryLatestVendorDiscoveryResults(bool showDeleted, string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            if (filterText == null) filterText = "%";
            else
            {
                // Build your filter string here!
                filterText += "%";
            }

            if (showDeleted)
                return m_dataContext.Table<LatestVendorDiscoveryResult>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("VendorPatchName LIKE {0}", filterText));

            return m_dataContext.Table<LatestVendorDiscoveryResult>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("IsDeleted = 0 AND Abbreviation LIKE {0}", filterText));
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
        public int GetLastDiscoveryResultID()
        {
            return m_dataContext.Connection.ExecuteScalar<int?>("SELECT IDENT_CURRENT('DiscoveryResult')") ?? 0;
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

        #region [ LatestProductDiscoveryResult View Operations ]

        [RecordOperation(typeof(LatestProductDiscoveryResult), RecordOperation.QueryRecordCount)]
        public int QueryLatestProductDiscoveryResultCount(string filterText)
        {

            string productFilter = "%";
            string vendorFilter = "%";
            if (filterText != "%")
            {
                string[] filters = filterText.Split(';');
                if (filters.Length == 2)
                {
                    productFilter = filters[0] + '%';
                    vendorFilter = filters[1] + '%';
                }
            }

            return m_dataContext.Table<LatestProductDiscoveryResult>().QueryRecordCount(new RecordRestriction("ProductName LIKE {0} AND Company LIKE {1}", productFilter, vendorFilter));
        }

        [RecordOperation(typeof(LatestProductDiscoveryResult), RecordOperation.QueryRecords)]
        public IEnumerable<LatestProductDiscoveryResult> QueryLatestProductDiscoveryResults(string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            string productFilter = "%";
            string vendorFilter = "%";
            if (filterText != "%")
            {
                string[] filters = filterText.Split(';');
                if (filters.Length == 2)
                {
                    productFilter = filters[0] + '%';
                    vendorFilter = filters[1] + '%';
                }
            }

            return m_dataContext.Table<LatestProductDiscoveryResult>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("ProductName LIKE {0} AND Company LIKE {1}", productFilter, vendorFilter));
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(LatestProductDiscoveryResult), RecordOperation.DeleteRecord)]
        public void DeleteLatestProductDiscoveryResult(int id)
        {
            // Delete associated DiscoveryResult record
            m_dataContext.Table<DiscoveryResult>().DeleteRecord(id);
        }

        [RecordOperation(typeof(LatestProductDiscoveryResult), RecordOperation.CreateNewRecord)]
        public LatestProductDiscoveryResult NewLatestProductDiscoveryResult()
        {
            return new LatestProductDiscoveryResult();
        }

        [AuthorizeHubRole("Administrator, Owner, PIC")]
        [RecordOperation(typeof(LatestProductDiscoveryResult), RecordOperation.AddNewRecord)]
        public void AddNewLatestProductDiscoveryResult(LatestProductDiscoveryResult record)
        {
            DiscoveryResult result = DeriveDiscoveryResult(record);
            result.CreatedByID = GetCurrentUserID();
            result.CreatedOn = DateTime.UtcNow;
            m_dataContext.Table<DiscoveryResult>().AddNewRecord(result);
        }

        //[AuthorizeHubRole("Administrator, Owner, PIC")]
        //public int GetLastDiscoveryResultID()
        //{
        //    return m_dataContext.Connection.ExecuteScalar<int?>("SELECT IDENT_CURRENT('DiscoveryResult')") ?? 0;
        //}

        [AuthorizeHubRole("Administrator, Owner, PIC")]
        [RecordOperation(typeof(LatestProductDiscoveryResult), RecordOperation.UpdateRecord)]
        public void UpdateLatestProductDiscoveryResult(LatestProductDiscoveryResult record)
        {
            m_dataContext.Table<DiscoveryResult>().UpdateRecord(DeriveDiscoveryResult(record));
        }

        private DiscoveryResult DeriveDiscoveryResult(LatestProductDiscoveryResult record)
        {
            return new DiscoveryResult
            {
                ID = record.DiscoveryResultID,
                VendorID = record.VendorID,
                ProductID = record.ProductID,
                ReviewDate = record.ReviewDate,
                ResultKey = record.ResultKey,
                Notes = record.Notes,
                CreatedByID = record.CreatedByID,
                CreatedOn = record.CreatedOn
            };
        }

        #endregion


        #region [ PatchStatusAssessmentView View Operations ]

        [RecordOperation(typeof(PatchStatusAssessmentView), RecordOperation.QueryRecordCount)]
        public int QueryPatchStatusAssessmentViewCount(string filterText = "%")
        {
                return m_dataContext.Table<PatchStatusAssessmentView>().QueryRecordCount();

        }

        [RecordOperation(typeof(PatchStatusAssessmentView), RecordOperation.QueryRecords)]
        public IEnumerable<PatchStatusAssessmentView> QueryPatchStatusAssessmentViews( string sortField, bool ascending, int page, int pageSize, string filterText = "%")
        {
                return m_dataContext.Table<PatchStatusAssessmentView>().QueryRecords(sortField, ascending, page, pageSize);
        }


        [RecordOperation(typeof(PatchStatusAssessmentView), RecordOperation.CreateNewRecord)]
        public PatchStatusAssessmentView NewPatchStatusAssessmentView()
        {
            return new PatchStatusAssessmentView();
        }

        [AuthorizeHubRole("Administrator, Owner, PIC")]
        [RecordOperation(typeof(PatchStatusAssessmentView), RecordOperation.AddNewRecord)]
        public void AddNewPatchStatusAssessmentViewInstall(PatchStatusAssessmentView record)
        {
            Install result = DeriveInstall(record);
            result.CreatedByID = GetCurrentUserID();
            result.CreatedOn = DateTime.UtcNow;
            m_dataContext.Table<Install>().AddNewRecord(result);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC")]
        [RecordOperation(typeof(PatchStatusAssessmentView), RecordOperation.UpdateRecord)]
        public void UpdatePatchStatusAssessmentViewInstallTable(PatchStatusAssessmentView record)
        {
            m_dataContext.Table<Install>().UpdateRecord(DeriveInstall(record));
        }

        private Install DeriveInstall(PatchStatusAssessmentView record)
        {
            return new Install
            {
                PatchStatusID = record.ID,
                Summary = record.Details,
                CompletedOn = DateTime.UtcNow,
                CreatedOn = DateTime.UtcNow,
                CreatedByID = GetCurrentUserID(),
                UpdatedOn = DateTime.UtcNow,
                UpdatedByID = GetCurrentUserID(),
                
            };
        }

        #endregion

        #region [HistoryView Table Operations]

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(HistoryView), RecordOperation.QueryRecordCount)]
        public int QueryHistoryViewCount(string filterText)
        {
            string patchFilter = "%";
            string productFilter = "%";
            string vendorFilter = "%";
            if (filterText != "%")
            {
                string[] filters = filterText.Split(';');
                if (filters.Length == 3)
                {
                    patchFilter += filters[0] + '%';
                    productFilter += filters[1] += '%';
                    vendorFilter += filters[2] += '%';
                }
            }
            return m_dataContext.Table<HistoryView>().QueryRecordCount(new RecordRestriction("VendorPatchName LIKE {0} AND ProductName LIKE {1} AND VendorName LIKE {2}", patchFilter, productFilter, vendorFilter));
        }

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(HistoryView), RecordOperation.QueryRecords)]
        public IEnumerable<HistoryView> QueryHistoryViews(string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            string patchFilter = "%";
            string productFilter = "%";
            string vendorFilter = "%";
            if (filterText != "%")
            {
                string[] filters = filterText.Split(';');
                if (filters.Length == 3)
                {
                    patchFilter += filters[0] + '%';
                    productFilter += filters[1] += '%';
                    vendorFilter += filters[2] += '%';
                }
            }
            return m_dataContext.Table<HistoryView>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("VendorPatchName LIKE {0} AND ProductName LIKE {1} AND VendorName LIKE {2}", patchFilter, productFilter, vendorFilter));
        }

        #endregion

        #region [PatchVendorPlatformView Table Operations]

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(PatchVendorPlatformView), RecordOperation.QueryRecordCount)]
        public int QueryPatchVendorPlatformViewCount(string filterText = "%")
        {
            return m_dataContext.Table<PatchVendorPlatformView>().QueryRecordCount();
        }

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(PatchVendorPlatformView), RecordOperation.QueryRecords)]
        public IEnumerable<PatchVendorPlatformView> QueryPatchVendorPlatformViews(int id, string filterText = "%")
        {
            return m_dataContext.Table<PatchVendorPlatformView>().QueryRecords(restriction: new RecordRestriction("ID = {0}", id));
        }

        #endregion

        #region [AssessmentHistoryView Table Operations]

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(AssessmentHistoryView), RecordOperation.QueryRecordCount)]
        public int QueryAssessmentHistoryViewCount(string filterText = "%")
        {
            return m_dataContext.Table<AssessmentHistoryView>().QueryRecordCount();
        }

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(AssessmentHistoryView), RecordOperation.QueryRecords)]
        public IEnumerable<AssessmentHistoryView> QueryAssessmentHistoryViews(int id, string filterText = "%")
        {
            return m_dataContext.Table<AssessmentHistoryView>().QueryRecords(restriction: new RecordRestriction("AssessmentID = {0}", id));
        }

        #endregion

        #region [InstallHistoryView Table Operations]

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(InstallHistoryView), RecordOperation.QueryRecordCount)]
        public int QueryInstallHistoryViewCount(string filterText = "%")
        {
            return m_dataContext.Table<InstallHistoryView>().QueryRecordCount();
        }

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(InstallHistoryView), RecordOperation.QueryRecords)]
        public IEnumerable<InstallHistoryView> QueryInstallHistoryViews(int id, string filterText = "%")
        {
            return m_dataContext.Table<InstallHistoryView>().QueryRecords(restriction: new RecordRestriction("InstallID = {0}", id));
        }

        #endregion

        #region [MitigationPlanHistoryView Table Operations]

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(MitigationPlanHistoryView), RecordOperation.QueryRecordCount)]
        public int QueryMitigationPlanHistoryViewCount(string filterText = "%")
        {
            return m_dataContext.Table<MitigationPlanHistoryView>().QueryRecordCount();
        }

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(MitigationPlanHistoryView), RecordOperation.QueryRecords)]
        public IEnumerable<MitigationPlanHistoryView> QueryMitigationPlanHistoryViews(int id, string filterText = "%")
        {
            return m_dataContext.Table<MitigationPlanHistoryView>().QueryRecords(restriction: new RecordRestriction("MitigationPlanID = {0}", id));
        }

        #endregion

        #region [AssessmentInstallView Table Operations]

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(AssessmentInstallView), RecordOperation.QueryRecordCount)]
        public int QueryAssessmentInstallViewCount(string filterText)
        {
            if (filterText == null) filterText = "%";
            else
            {
                // Build your filter string here!
                filterText += "%";
            }
            return m_dataContext.Table<AssessmentInstallView>().QueryRecordCount(new RecordRestriction("VendorPatchName LIKE {0}", filterText));
        }

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(AssessmentInstallView), RecordOperation.QueryRecords)]
        public IEnumerable<AssessmentInstallView> QueryAssessmentInstallViews(string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            if (filterText == null) filterText = "%";
            else
            {
                // Build your filter string here!
                filterText += "%";
            }
            return m_dataContext.Table<AssessmentInstallView>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("VendorPatchName LIKE {0}", filterText));
        }

        [RecordOperation(typeof(AssessmentInstallView), RecordOperation.CreateNewRecord)]
        public AssessmentInstallView NewAssessmentInstallView()
        {
            return new AssessmentInstallView();
        }

        [AuthorizeHubRole("Administrator, Owner, SME")]
        [RecordOperation(typeof(AssessmentInstallView), RecordOperation.AddNewRecord)]
        public void AddNewAssessmentInstallViewInstall(AssessmentInstallView record)
        {
            Install result = DeriveInstall(record);
            result.CreatedByID = GetCurrentUserID();
            result.CreatedOn = DateTime.UtcNow;
            m_dataContext.Table<Install>().AddNewRecord(result);
        }

        [AuthorizeHubRole("Administrator, Owner, SME")]
        [RecordOperation(typeof(AssessmentInstallView), RecordOperation.UpdateRecord)]
        public void UpdateAssessmentInstallViewInstallTable(AssessmentInstallView record)
        {
            m_dataContext.Table<Assessment>().UpdateRecord(DeriveAssessment(record));
        }

        private Install DeriveInstall(AssessmentInstallView record)
        {
            return new Install()
            {
                PatchStatusID = record.PatchStatusID,
                Summary = record.Summary,
                CompletedOn = record.CompletedOn,
                CreatedOn = DateTime.UtcNow,
                CreatedByID = GetCurrentUserID(),
                UpdatedOn = DateTime.UtcNow,
                UpdatedByID = GetCurrentUserID(),
                IsInstalled = true,
                WorkManagementID =  record.WorkManagementID,
                CompletedNotes = record.CompletedNotes

            };
        }

        private Assessment DeriveAssessment(AssessmentInstallView record)
        {
            return new Assessment()
            {
                ID = record.ID,
                PatchStatusID = record.PatchStatusID,
                AssessmentResultKey = record.AssessmentResultKey,
                Details = record.Details,
                CreatedOn = record.CreatedOn,
                CreatedByID = record.CreatedByID,
                UpdatedOn = DateTime.UtcNow,
                UpdatedByID = GetCurrentUserID(),
                IsAssessed = true

            };
        }

        #endregion

        #region [AssessmentMitigateView Table Operations]

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(AssessmentMitigateView), RecordOperation.QueryRecordCount)]
        public int QueryAssessmentMitigateViewCount(string filterText)
        {
            if (filterText == null) filterText = "%";
            else
            {
                // Build your filter string here!
                filterText += "%";
            }

            return m_dataContext.Table<AssessmentMitigateView>().QueryRecordCount(new RecordRestriction("VendorPatchName LIKE {0}", filterText));
        }

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(AssessmentMitigateView), RecordOperation.QueryRecords)]
        public IEnumerable<AssessmentMitigateView> QueryAssessmentMitigateViews(string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            if (filterText == null) filterText = "%";
            else
            {
                // Build your filter string here!
                filterText += "%";
            }

            return m_dataContext.Table<AssessmentMitigateView>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("VendorPatchName LIKE {0}", filterText));
        }

        [RecordOperation(typeof(AssessmentMitigateView), RecordOperation.CreateNewRecord)]
        public AssessmentMitigateView NewAssessmentMitigateView()
        {
            return new AssessmentMitigateView();
        }

        [AuthorizeHubRole("Administrator, Owner, SME")]
        [RecordOperation(typeof(AssessmentMitigateView), RecordOperation.AddNewRecord)]
        public void AddNewAssessmentMitigateViewMitigate(AssessmentMitigateView record)
        {
            int themeID = MiPlanContext.Connection.ExecuteScalar<int?>("Select ID FROM Theme WHERE IsDefault = 1") ?? -1;
            ++record.PatchStatusKey;
            if (record.AssessmentResultKey > 2)
                ++record.PatchStatusKey;

            UpdatePatchStatusKey(record.PatchStatusID, record.PatchStatusKey);
            MiPlanContext.Table<MiPlan>().AddNewRecord(DeriveMiPlan(record, themeID));

            MitigationPlan result = DeriveMitigate(record);
            result.MiPlanID =  GetLastMitigationPlanID();
            result.CreatedByID = GetCurrentUserID();
            result.CreatedOn = DateTime.UtcNow;
            result.UpdatedOn = result.CreatedOn;
            result.UpdatedByID = result.CreatedByID;
            m_dataContext.Table<MitigationPlan>().AddNewRecord(result);

        }

        [AuthorizeHubRole("Administrator, Owner, SME")]
        [RecordOperation(typeof(AssessmentMitigateView), RecordOperation.UpdateRecord)]
        public void UpdateAssessmentMitigateView(AssessmentMitigateView record)
        {
            m_dataContext.Table<Assessment>().UpdateRecord(DeriveAssessment(record));
        }

        private MitigationPlan DeriveMitigate(AssessmentMitigateView record)
        {
            return new MitigationPlan()
            {
                PatchStatusID = record.PatchStatusID,
                Summary = record.Summary + ' ',
                MiPlanID = record.MiPlanID,
                CreatedOn = DateTime.UtcNow,
                CreatedByID = GetCurrentUserID(),
                UpdatedOn = DateTime.UtcNow,
                UpdatedByID = GetCurrentUserID(),
                IsMitigated = false

            };
        }

        private Assessment DeriveAssessment(AssessmentMitigateView record)
        {
            return new Assessment()
            {
                ID = record.ID,
                PatchStatusID = record.PatchStatusID,
                AssessmentResultKey = record.AssessmentResultKey,
                Details = record.Details,
                CreatedOn = record.CreatedOn,
                CreatedByID = record.CreatedByID,
                UpdatedOn = DateTime.UtcNow,
                UpdatedByID = GetCurrentUserID(),
                IsAssessed = true

            };
        }

        private MiPlan DeriveMiPlan(AssessmentMitigateView record, int themeID)
        {
            MiPlan miPlan = new MiPlan();
            miPlan.Title = record.VendorPatchName;
            miPlan.ThemeID = themeID;
            miPlan.BusinessUnitID = record.BusinessUnitID;
            miPlan.Field1 = record.PlatformName;
            miPlan.Field3 = record.Summary;
            miPlan.Description = "";
            miPlan.StatusNotes = "";
            miPlan.CreatedByID = GetCurrentUserID();
            miPlan.CreatedOn = DateTime.UtcNow;
            miPlan.UpdatedByID = miPlan.CreatedByID;
            miPlan.UpdatedOn = miPlan.CreatedOn;
            return miPlan;
        }


        #endregion

        #region [ClosingReviewView Table Operations]

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(ClosingReviewView), RecordOperation.QueryRecordCount)]
        public int QueryClosingReviewViewCount(string filterText)
        {
            if (filterText == null) filterText = "%";
            else
            {
                // Build your filter string here!
                filterText += "%";
            }

            return m_dataContext.Table<ClosingReviewView>().QueryRecordCount(new RecordRestriction("VendorPatchName LIKE {0}", filterText));
        }

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(ClosingReviewView), RecordOperation.QueryRecords)]
        public IEnumerable<ClosingReviewView> QueryClosingReviewViews(string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            if (filterText == null) filterText = "%";
            else
            {
                // Build your filter string here!
                filterText += "%";
            }
            return m_dataContext.Table<ClosingReviewView>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("VendorPatchName LIKE {0}", filterText));
        }

        [AuthorizeHubRole("Administrator, Owner, PIC")]
        [RecordOperation(typeof(ClosingReviewView), RecordOperation.CreateNewRecord)]
        public ClosingReviewView NewClosingReviewView()
        {
            return new ClosingReviewView();
        }

        [AuthorizeHubRole("Administrator, Owner, PIC")]
        [RecordOperation(typeof(ClosingReviewView), RecordOperation.AddNewRecord)]
        public void AddNewClosingReviewViewMitigate(ClosingReviewView record)
        {
            ClosedPatch result = DeriveClosedPatch(record);
            m_dataContext.Table<ClosedPatch>().AddNewRecord(result);
        }

        [AuthorizeHubRole("Administrator, Owner, PIC")]
        [RecordOperation(typeof(ClosingReviewView), RecordOperation.UpdateRecord)]
        public void UpdateClosingReviewView(ClosingReviewView record)
        {
            m_dataContext.Table<ClosedPatch>().UpdateRecord(DeriveClosedPatch(record));
        }

        private ClosedPatch DeriveClosedPatch(ClosingReviewView record)
        {
            return new ClosedPatch()
            {
                PatchStatusID = record.ID,
                ClosedDate = DateTime.UtcNow,
                Details = record.Details,
                ActionKey = record.AssessmentResultKey

            };
        }


        #endregion

        #region [VendorPlatformView Table Operations]

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(VendorPlatformView), RecordOperation.QueryRecordCount)]
        public int QueryVendorPlatformViewCount(string filterText = "%")
        {
            return m_dataContext.Table<VendorPlatformView>().QueryRecordCount();
        }

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(VendorPlatformView), RecordOperation.QueryRecords)]
        public IEnumerable<VendorPlatformView> QueryVendorPlatformViews(int id, string filterText = "%")
        {
            return m_dataContext.Table<VendorPlatformView>().QueryRecords(restriction: new RecordRestriction("IsDeleted = 0 AND VendorID = {0}", id));
        }

        #endregion

        #region [ NoticeLog Table Operations ]


        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(NoticeLog), RecordOperation.QueryRecordCount)]
        public int QueryNoticeLogCount(string filterText)
        {
            if (filterText == null) filterText = "%";
            else
            {
                // Build your filter string here!
                filterText += "%";
            }
            return m_dataContext.Table<NoticeLog>().QueryRecordCount(new RecordRestriction("Title LIKE {0}", filterText));
        }

        [AuthorizeHubRole("*")]
        public IEnumerable<NoticeLog> QueryNoticeLogs()
        {

            return m_dataContext.Table<NoticeLog>().QueryRecords();
        }

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(NoticeLog), RecordOperation.QueryRecords)]
        public IEnumerable<NoticeLog> QueryNoticeLogs(string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            if (filterText == null) filterText = "%";
            else
            {
                // Build your filter string here!
                filterText += "%";
            }
            return m_dataContext.Table<NoticeLog>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("Title LIKE {0}", filterText));
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(NoticeLog), RecordOperation.DeleteRecord)]
        public void DeleteNoticeLog(int id)
        {
            m_dataContext.Table<NoticeLog>().DeleteRecord(id);
        }

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(NoticeLog), RecordOperation.CreateNewRecord)]
        public NoticeLog NewNoticeLog()
        {
            return new NoticeLog();
        }

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(NoticeLog), RecordOperation.AddNewRecord)]
        public void AddNewNoticeLog(NoticeLog record)
        {
            record.CreatedOn = DateTime.UtcNow;
            m_dataContext.Table<NoticeLog>().AddNewRecord(record);
        }

        #endregion

        #region [ NoticeLogView Table Operations ]


        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(NoticeLogView), RecordOperation.QueryRecordCount)]
        public int QueryNoticeLogViewCount(string filterText)
        {
            if (filterText == null) filterText = "%";
            else
            {
                // Build your filter string here!
                filterText += "%";
            }
            return m_dataContext.Table<NoticeLogView>().QueryRecordCount(new RecordRestriction("VendorPatchName LIKE {0}", filterText));
        }

        [AuthorizeHubRole("*")]
        public IEnumerable<NoticeLogView> QueryNoticeLogViews()
        {
            return m_dataContext.Table<NoticeLogView>().QueryRecords();
        }

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(NoticeLogView), RecordOperation.QueryRecords)]
        public IEnumerable<NoticeLogView> QueryNoticeLogViews(string sortField, bool ascending, int page, int pageSize, string filterText)
        {
            if (filterText == null) filterText = "%";
            else
            {
                // Build your filter string here!
                filterText += "%";
            }
            return m_dataContext.Table<NoticeLogView>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("VendorPatchName LIKE {0}", filterText));
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(NoticeLogView), RecordOperation.DeleteRecord)]
        public void DeleteNoticeLogView(int id)
        {
            m_dataContext.Table<NoticeLogView>().DeleteRecord(id);
        }

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(NoticeLogView), RecordOperation.CreateNewRecord)]
        public NoticeLogView NewNoticeLogView()
        {
            return new NoticeLogView();
        }

        [AuthorizeHubRole("*")]
        [RecordOperation(typeof(NoticeLogView), RecordOperation.AddNewRecord)]
        public void AddNewNoticeLogView(NoticeLogView record)
        {
            record.CreatedOn = DateTime.UtcNow;
            m_dataContext.Table<NoticeLogView>().AddNewRecord(record);
        }

        #endregion


        #region [ MiPlan Table Operations ]

        // NOTE: These hub operations directly operate on MiPlan database and will apply authorization rights in context
        // of openSPM database security. If MiPlan local security should be taken into account, these database operations
        // should be dropped in-lieu of iframe and/or service based access to MiPlan...

        [RecordOperation(typeof(MiPlan), RecordOperation.QueryRecordCount)]
        public int QueryMiPlanCount(bool showDeleted, string filterText = "%")
        {
            if (showDeleted)
                return MiPlanContext.Table<MiPlan>().QueryRecordCount(new RecordRestriction());

            return MiPlanContext.Table<MiPlan>().QueryRecordCount(new RecordRestriction("IsDeleted = 0"));
        }

        [RecordOperation(typeof(MiPlan), RecordOperation.QueryRecords)]
        public IEnumerable<MiPlan> QueryMiPlanes(bool showDeleted, string sortField, bool ascending, int page, int pageSize, string filterText = "%")
        {
            if (showDeleted)
                return MiPlanContext.Table<MiPlan>().QueryRecords(sortField, ascending, page, pageSize);

            return MiPlanContext.Table<MiPlan>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("IsDeleted = 0"));
        }

        [RecordOperation(typeof(MiPlan), RecordOperation.QueryRecords)]
        public IEnumerable<MiPlan> GetMiPlanRecord(int id)
        {
            return MiPlanContext.Table<MiPlan>().QueryRecords( restriction: new RecordRestriction("ID = {0}", id));
        }

        [RecordOperation(typeof(MiPlan), RecordOperation.CreateNewRecord)]
        public MiPlan NewMiPlan()
        {
            return new MiPlan();
        }

        [AuthorizeHubRole("Administrator, Owner, SME")]
        [RecordOperation(typeof(MiPlan), RecordOperation.AddNewRecord)]
        public void AddNewMiPlan(MiPlan record)
        {
            record.CreatedByID = GetCurrentUserID();
            record.CreatedOn = DateTime.UtcNow;
            record.UpdatedByID = record.CreatedByID;
            record.UpdatedOn = record.CreatedOn;
            record.IsCompleted = false;
            record.IsDeleted = false;
            record.IsApproved = false;
            MiPlanContext.Table<MiPlan>().AddNewRecord(record);
        }

        [AuthorizeHubRole("Administrator, Owner, SME")]
        [RecordOperation(typeof(MiPlan), RecordOperation.UpdateRecord)]
        public void UpdateMiPlan(MiPlan record)
        {
            record.UpdatedByID = GetCurrentUserID();
            record.UpdatedOn = DateTime.UtcNow;
            MiPlanContext.Table<MiPlan>().UpdateRecord(record);
        }

        [AuthorizeHubRole("Administrator, Owner")]
        [RecordOperation(typeof(MiPlan), RecordOperation.DeleteRecord)]
        public int GetLastMiPlanRecord()
        {
            // For MitigationPlanes, we only "mark" a record as deleted
            return MiPlanContext.Connection.ExecuteScalar<int?>("SELECT IDENT_CURRENT('MitigationPlan')") ?? 0;
        }

        #endregion

            #region [ ThemeFields Table Operations ]


        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ThemeFields), RecordOperation.QueryRecordCount)]
        public int QueryThemeFieldsCount(int parentID, string filterText = "%")
        {
            return MiPlanContext.Table<ThemeFields>().QueryRecordCount(new RecordRestriction("GroupID = {0}", parentID));
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ThemeFields), RecordOperation.QueryRecords)]
        public IEnumerable<ThemeFields> QueryThemeFieldsItems(int parentID, string sortField, bool ascending, int page, int pageSize, string filterText = "%")
        {
            return MiPlanContext.Table<ThemeFields>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction("GroupID = {0}", parentID));
        }

        [AuthorizeHubRole("Administrator")]
        [RecordOperation(typeof(ThemeFields), RecordOperation.DeleteRecord)]
        public void DeleteThemeFields(int id)
        {
            MiPlanContext.Table<ThemeFields>().DeleteRecord(id);
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
            AuthorizationCache.UserIDs.TryGetValue(Thread.CurrentPrincipal.Identity.Name, out userID);
            return userID;
        }

        #endregion
    }
}
