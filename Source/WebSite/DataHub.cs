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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GSF.Data;
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

        // Example DataHub Table Operations
        #region [ Company Table Operations ]

        //public int QueryCompanyCount()
        //{
        //    return m_dataContext.Table<Company>().QueryRecordCount();
        //}

        //public IEnumerable<Company> QueryCompanies(string sortField, bool ascending, int page, int pageSize)
        //{
        //    return m_dataContext.Table<Company>().QueryRecords(sortField, ascending, page, pageSize);
        //}

        //public void DeleteCompany(int id)
        //{
        //    m_dataContext.Table<Company>().DeleteRecord(id);
        //}

        //public Company NewCompany()
        //{
        //    return new Company();
        //}

        //public void AddNewCompany(Company company)
        //{
        //    company.CreatedBy = UserInfo.CurrentUserID;
        //    company.CreatedOn = DateTime.UtcNow;
        //    company.UpdatedBy = company.CreatedBy;
        //    company.UpdatedOn = company.CreatedOn;

        //    m_dataContext.Table<Company>().AddNewRecord(company);
        //}

        //public void UpdateCompany(Company company)
        //{
        //    m_dataContext.Table<Company>().UpdateRecord(company);
        //}

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
