//******************************************************************************************************
//  AdminController.cs - Gbtc
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

using System.Web.Mvc;
using openSPM.Models;

namespace openSPM.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        #region [ Members ]

        // Fields
        private readonly DataContext m_dataContext;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        public AdminController()
        {
            // Set default model for pages used by layout
            ViewData.Model = MvcApplication.DefaultModel;

            // Establish data context for the view
            m_dataContext = new DataContext();
            ViewData.Add("DataContext", m_dataContext);
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="MainController"/> object and optionally releases the managed resources.
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

        public ActionResult Home()
        {
            return View();
        }

        public ActionResult Pages()
        {
            return View();
        }

        public ActionResult MenuItems()
        {
            return View();
        }

        public ActionResult ValueListGroups()
        {
            return View();
        }

        public ActionResult ValueListItems()
        {
            return View();
        }

        #endregion
    }
}