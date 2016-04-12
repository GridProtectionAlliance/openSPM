//******************************************************************************************************
//  FileUploadHandler.ashx.cs - Gbtc
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Web;
using GSF.Data;
using GSF.Data.Model;
using GSF.Identity;
using GSF.IO;
using GSF.Security;
using GSF.Web.Model;
using GSF.Web.Security;
using openSPM.Model;

namespace openSPM
{
    /// <summary>
    /// Handles uploaded files.
    /// </summary>
    public class FileUploadHandler : IHttpHandler
    {
        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="IHttpHandler"/> instance.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the <see cref="IHttpHandler"/> instance is reusable; otherwise, <c>false</c>.
        /// </returns>
        public bool IsReusable => false;

        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="IHttpHandler"/> interface.
        /// </summary>
        /// <param name="context">An <see cref="HttpContext"/> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests. </param>
        public void ProcessRequest(HttpContext context)
        {
            // Initialize the security principal from caller's windows identity if uninitialized, note that
            // simply by checking current provider any existing cached security principal will be restored,
            // if no current provider exists we create a new one
            if (SecurityProviderCache.CurrentProvider == null)
            {
                lock (typeof(FileUploadHandler))
                {
                    // Let's see if we won the race...
                    if (SecurityProviderCache.CurrentProvider == null)
                        SecurityProviderCache.CurrentProvider = SecurityProviderUtility.CreateProvider(string.Empty);
                }
            }

            // TODO: Add role restriction check to file upload processing - load from AppSettings or, better, check DataHub record operations cache...
            if (context.Request.Files.Count > 0 && context.User.Identity.IsAuthenticated)
            {
                NameValueCollection parameters = context.Request.QueryString;
                int sourceID = int.Parse(parameters["SourceID"] ?? "0");
                string sourceField = parameters["SourceField"];
                string tableName = parameters["TableName"];

                if (sourceID > 0 && !string.IsNullOrEmpty(sourceField) && !string.IsNullOrEmpty(tableName))
                {
                    using (DataContext dataContext = new DataContext(exceptionHandler: MvcApplication.LogException))
                    {
                        IEnumerable<int> documentIDs = dataContext.Connection.
                            RetrieveData($"SELECT DocumentID FROM {tableName} WHERE {sourceField} = {{0}}", sourceID).AsEnumerable().
                            Select(row => row.ConvertField<int>("DocumentID", 0));

                        Document[] documents = dataContext.Table<Document>().QueryRecords("Filename", new RecordRestriction($"ID IN ({string.Join(", ", documentIDs)})")).ToArray();

                        HttpFileCollection files = context.Request.Files;

                        for (int i = 0; i < files.Count; i++)
                        {
                            HttpPostedFile file = files[i];
                            string filename = FilePath.GetFileName(file.FileName);
                            Document document = new Document
                            {
                                Filename = filename,
                                DocumentBlob = file.InputStream.ReadStream(),
                                Enabled = true,
                                CreatedOn = DateTime.UtcNow,
                                CreatedByID = GetCurrentUserID()
                            };

                            // Attempt to match file type to document type keys as defined in value list
                            int groupID = dataContext.Connection.ExecuteScalar<int?>("SELECT ID FROM ValueListGroup WHERE Name='fileType' AND Enabled <> 0") ?? 0;
                            Dictionary<string, int> documentTypeKeys = dataContext.Table<ValueList>().QueryRecords("SortOrder", new RecordRestriction("GroupID = {0} AND Enabled <> 0 AND Hidden = 0", groupID)).ToDictionary(vl => vl.AltText1, vl => vl.Key);
                            string extension = FilePath.GetExtension(filename);
                            int documentTypeKey, defaultDocumentTypeKey;

                            // Get default document type key, i.e., "Other"
                            documentTypeKeys.TryGetValue("*", out defaultDocumentTypeKey);

                            if (!string.IsNullOrWhiteSpace(extension))
                            {
                                // Only worry about first the characters of any extension to determine type
                                if (extension.Length > 3)
                                    extension = extension.Substring(0, 3);

                                if (!documentTypeKeys.TryGetValue(extension, out documentTypeKey))
                                    documentTypeKey = defaultDocumentTypeKey;
                            }
                            else
                            {
                                documentTypeKey = defaultDocumentTypeKey;
                            }

                            document.DocumentTypeKey = documentTypeKey;

                            if (documents.Count(doc => doc.Filename.Equals(filename, StringComparison.OrdinalIgnoreCase)) == 0)
                            {
                                // Upload new document
                                dataContext.Table<Document>().AddNewRecord(document);
                            }
                            else
                            {
                                // Update existing document
                                int documentID = dataContext.Connection.ExecuteScalar<int?>("SELECT ID WHERE Filename = {0}", filename) ?? 0;

                                if (documentID == 0)
                                {
                                    dataContext.Table<Document>().AddNewRecord(document);
                                }
                                else
                                {
                                    document.ID = documentID;
                                    dataContext.Table<Document>().UpdateRecord(document);
                                }
                            }
                        }
                    }
                }
            }

            context.Response.ContentType = "application/json";
            context.Response.Write("{}");
        }

        private Guid GetCurrentUserID()
        {
            Guid userID;
            AuthorizationCache.UserIDs.TryGetValue(UserInfo.CurrentUserID, out userID);
            return userID;
        }
    }
}