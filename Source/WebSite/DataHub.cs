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
using System.Threading;
using System.Threading.Tasks;
using GSF.Identity;
using Microsoft.AspNet.SignalR;
using openSPM.Models;

namespace openSPM
{
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
                    {
                        m_dataContext?.Dispose();
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

        #region [ Patch Table Operations ]

        public int QueryPatchCount()
        {
            return m_dataContext.Table<Patch>().QueryRecordCount();
        }

        public IEnumerable<Patch> QueryPatches(string sortField, bool ascending, int page, int pageSize)
        {
            return m_dataContext.Table<Patch>().QueryRecords(sortField, ascending, page, pageSize);
        }

        public void DeletePatch(int id)
        {
            m_dataContext.Table<Patch>().DeleteRecord(id);
        }

        public Patch NewPatch()
        {
            return new Patch();
        }

        public void AddNewPatch(Patch patch)
        {
            patch.CreatedBy = UserInfo.UserNameToSID(UserInfo.CurrentUserID);
            patch.CreatedOn = DateTime.UtcNow;
            patch.UpdatedBy = patch.CreatedBy;
            patch.UpdatedOn = patch.CreatedOn;
            m_dataContext.Table<Patch>().AddNewRecord(patch);
        }

        public void UpdatePatch(Patch patch)
        {
            patch.UpdatedBy = UserInfo.UserNameToSID(UserInfo.CurrentUserID);
            patch.UpdatedOn = DateTime.UtcNow;
            m_dataContext.Table<Patch>().UpdateRecord(patch);
        }

        #endregion

        #region [ Page Table Operations ]

        public int QueryPageCount()
        {
            return m_dataContext.Table<Page>().QueryRecordCount();
        }

        public IEnumerable<Page> QueryPages(string sortField, bool ascending, int page, int pageSize)
        {
            return m_dataContext.Table<Page>().QueryRecords(sortField, ascending, page, pageSize);
        }

        public void DeletePage(int id)
        {
            m_dataContext.Table<Page>().DeleteRecord(id);
        }

        public Page NewPage()
        {
            return new Page();
        }

        public void AddNewPage(Page page)
        {
            m_dataContext.Table<Page>().AddNewRecord(page);
        }

        public void UpdatePage(Page page)
        {
            m_dataContext.Table<Page>().UpdateRecord(page);
        }

        #endregion

        #region [ MenuItem Table Operations ]

        public IEnumerable<MenuItem> QueryMenuItems(int parentID, string sortField, bool ascending, int page, int pageSize)
        {
            return m_dataContext.Table<MenuItem>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction
            {
                FilterExpression = "PageID = {0}",
                Parameters = new object[] { parentID }
            });
        }

        public int QueryMenuItemCount(int parentID)
        {
            return m_dataContext.Table<MenuItem>().QueryRecordCount(new RecordRestriction
            {
                FilterExpression = "PageID = {0}",
                Parameters = new object[] { parentID }
            });
        }

        public void DeleteMenuItem(int id)
        {
            m_dataContext.Table<MenuItem>().DeleteRecord(id);
        }

        public MenuItem NewMenuItem()
        {
            return new MenuItem();
        }

        public void AddNewMenuItem(MenuItem menuItem)
        {
            m_dataContext.Table<MenuItem>().AddNewRecord(menuItem);
        }

        public void UpdateMenuItem(MenuItem menuItem)
        {
            m_dataContext.Table<MenuItem>().UpdateRecord(menuItem);
        }

        #endregion

        #region [ ValueListGroup Table Operations ]

        public int QueryValueListGroupCount()
        {
            return m_dataContext.Table<ValueListGroup>().QueryRecordCount();
        }

        public IEnumerable<ValueListGroup> QueryValueListGroups(string sortField, bool ascending, int page, int pageSize)
        {
            return m_dataContext.Table<ValueListGroup>().QueryRecords(sortField, ascending, page, pageSize);
        }

        public void DeleteValueListGroup(int id)
        {
            m_dataContext.Table<ValueListGroup>().DeleteRecord(id);
        }

        public ValueListGroup NewValueListGroup()
        {
            return new ValueListGroup();
        }

        public void AddNewValueListGroup(ValueListGroup valueListGroup)
        {
            valueListGroup.dtCreated = DateTime.UtcNow;
            m_dataContext.Table<ValueListGroup>().AddNewRecord(valueListGroup);
        }

        public void UpdateValueListGroup(ValueListGroup valueListGroup)
        {
            m_dataContext.Table<ValueListGroup>().UpdateRecord(valueListGroup);
        }

        #endregion

        #region [ ValueList Table Operations ]

        public IEnumerable<ValueList> QueryValueListItems(int parentID, string sortField, bool ascending, int page, int pageSize)
        {
            return m_dataContext.Table<ValueList>().QueryRecords(sortField, ascending, page, pageSize, new RecordRestriction
            {
                FilterExpression = "GroupID = {0}",
                Parameters = new object[] { parentID }
            });
        }

        public int QueryValueListCount(int parentID)
        {
            return m_dataContext.Table<ValueList>().QueryRecordCount(new RecordRestriction
            {
                FilterExpression = "GroupID = {0}",
                Parameters = new object[] { parentID }
            });
        }

        public void DeleteValueList(int id)
        {
            m_dataContext.Table<ValueList>().DeleteRecord(id);
        }

        public ValueList NewValueList()
        {
            return new ValueList();
        }

        public void AddNewValueList(ValueList valueList)
        {
            m_dataContext.Table<ValueList>().AddNewRecord(valueList);
        }

        public void UpdateValueList(ValueList valueList)
        {
            m_dataContext.Table<ValueList>().UpdateRecord(valueList);
        }

        #endregion

        #endregion

        #region [ Static ]

        /// <summary>
        /// Gets the hub connection ID for the current thread.
        /// </summary>
        public static string CurrentConnectionID => s_connectionID.Value;

        // Static Fields
        private static volatile int s_connectCount;
        private static readonly ThreadLocal<string> s_connectionID = new ThreadLocal<string>();

        #endregion
    }
}
