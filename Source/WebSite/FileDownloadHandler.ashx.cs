//******************************************************************************************************
//  FileDownloadHandler.ashx.cs - Gbtc
//
//  Copyright © 2016, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  04/07/2016 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Specialized;
using System.Data;
using System.Security;
using System.Threading;
using System.Web;
using GSF.Collections;
using GSF.Data;
using GSF.Data.Model;
using GSF.Security;
using GSF.Web.Model;

namespace openSPM
{
    /// <summary>
    /// Handles downloading files.
    /// </summary>
    public class FileDownloadHandler : IHttpHandler
    {
        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="IHttpHandler"/> instance.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the <see cref="IHttpHandler"/> instance is reusable; otherwise, <c>false</c>.
        /// </returns>
        public bool IsReusable => false;

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="IHttpHandler" /> interface.
        /// </summary>
        /// <param name="context">An <see cref="HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests. </param>
        public void ProcessRequest(HttpContext context)
        {
            HttpResponse response = HttpContext.Current.Response;
            response.ClearContent();
            response.Clear();

            SecurityProviderCache.ValidateCurrentProvider();
            NameValueCollection parameters = context.Request.QueryString;
            string sourceTable = parameters["SourceTable"];
            int documentID = int.Parse(parameters["DocumentID"] ?? "0");
            string modelName = parameters["ModelName"]; // If provided, must include namespace

            if (documentID > 0 && !string.IsNullOrEmpty(sourceTable))
            {
                if (string.IsNullOrEmpty(modelName))
                {
                    string currentNamespace = GetType().FullName;
                    int lastPeriodIndex = currentNamespace.LastIndexOf('.');

                    if (lastPeriodIndex > 0)
                        currentNamespace = currentNamespace.Substring(0, lastPeriodIndex);

                    modelName = $"{currentNamespace}.Model.{sourceTable}";
                }

                Type associatedModel = Type.GetType(modelName);

                // Get any authorized update roles as defined in hub records operations for modeled table
                Tuple<string, string> recordOperation = DataHub.GetRecordOperationsCache().GetRecordOperations(associatedModel)[(int)RecordOperation.UpdateRecord];
                string editRoles = string.IsNullOrEmpty(recordOperation?.Item1) ? "" : recordOperation.Item2 ?? "";

                using (DataContext dataContext = new DataContext(exceptionHandler: MvcApplication.LogException))
                {
                    if (!dataContext.UserIsInRole(editRoles))
                        throw new SecurityException($"Access is denied for user '{Thread.CurrentPrincipal.Identity.Name}': minimum required roles = {editRoles.ToDelimitedString(", ")}.");

                    using (IDataReader reader = dataContext.Connection.ExecuteReader(CommandBehavior.SequentialAccess, dataContext.Connection.DefaultTimeout, "SELECT Filename, DocumentBlob FROM Document WHERE ID = {0} AND Enabled <> 0", documentID))
                    {
                        if (reader.Read())
                        {
                            const int BufferSize = 32768;
                            byte[] buffer = new byte[BufferSize];
                            int index = 0;
                            long readCount;
                            string filename = reader.GetString(0);

                            if (string.IsNullOrWhiteSpace(filename))
                                filename = "Undetermined";

                            response.ContentType = MimeMapping.GetMimeMapping(filename);
                            response.AddHeader("Content-Disposition", "attachment; filename=" + filename + ";");                            
                            response.BufferOutput = true;

                            readCount = reader.GetBytes(1, index, buffer, 0, BufferSize);

                            // Continue reading and writing while there are bytes beyond the size of the buffer.
                            while (readCount == BufferSize)
                            {
                                response.OutputStream.Write(buffer, 0, BufferSize);
                                index += BufferSize;
                                readCount = reader.GetBytes(1, index, buffer, 0, BufferSize);
                            }

                            // Write any remaining buffer
                            if (readCount > 0)
                                response.OutputStream.Write(buffer, 0, (int)readCount);

                            response.Flush();
                        }
                    }
                }
            }

            response.End();
        }
    }
}