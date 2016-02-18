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
            s_connectCount++;
            MvcApplication.LogStatusMessage($"DataHub connect by {Context.User?.Identity?.Name ?? "Undefined User"} [{Context.ConnectionId}] - count = {s_connectCount}");
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            if (stopCalled)
            {
                s_connectCount--;
                MvcApplication.LogStatusMessage($"DataHub disconnect by {Context.User?.Identity?.Name ?? "Undefined User"} [{Context.ConnectionId}] - count = {s_connectCount}");
            }

            return base.OnDisconnected(stopCalled);
        }

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

        // Static Fields
        private static volatile int s_connectCount;

        #endregion
    }
}
