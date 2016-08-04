﻿//******************************************************************************************************
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
//  07/29/2016 - Billy Ernest
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using GSF.Security;
using GSF.Web.Hosting;
using GSF.Web.Model;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web;

namespace openSPM
{
    /// <summary>
    /// Handles downloading files.
    /// </summary>
    public class CSVDownloadHandler : IHttpHandler
    {
        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="IHttpHandler"/> instance.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the <see cref="IHttpHandler"/> instance is reusable; otherwise, <c>false</c>.
        /// </returns>
        public bool IsReusable => false;

        /// <summary>
        /// Enables processing of HTTP web requests by a custom handler that implements the <see cref="T:GSF.Web.Hosting.IHostedHttpHandler"/> interface.
        /// </summary>
        /// <param name="request">HTTP request message.</param><param name="response">HTTP response message.</param>
        //public Task ProcessRequestAsync(HttpRequestMessage request, HttpResponseMessage response)
        //{
        //    return Task.Run(() =>
        //    {
        //        SecurityProviderCache.ValidateCurrentProvider();
        //        NameValueCollection parameters = request.RequestUri.ParseQueryString();

        //        string modelName = parameters["ModelName"]; // If provided, must include namespace

        //        //Type associatedModel = Type.GetType(modelName);

        //        using (DataContext dataContext = new DataContext())
        //        {
        //            using (DataTable table = dataContext.Connection.RetrieveData($"Select * FROM {modelName}"))
        //            {
        //                StringBuilder sb = new StringBuilder();

        //                IEnumerable<string> columnNames = table.Columns.Cast<DataColumn>().
        //                                                  Select(column => $"\"{column.ColumnName}\"");
        //                sb.AppendLine(string.Join(",", columnNames));

        //                foreach (DataRow row in table.Rows)
        //                {
        //                    IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString().Replace(',', '-'));
        //                    sb.AppendLine(string.Join(",", fields));
        //                }

        //                response.Content = new StreamContent(new MemoryStream(Encoding.Default.GetBytes(sb.ToString())));
        //                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
        //                {
        //                    FileName = "SqlExport.csv"
        //                };
        //                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/text");
        //            }
        //        }
        //    });
        //}

        public void ProcessRequest(HttpContext context)
        {
            HttpResponse response = HttpContext.Current.Response;
            response.ClearContent();
            response.Clear();

            SecurityProviderCache.ValidateCurrentProvider();
            NameValueCollection parameters = context.Request.QueryString;

            string modelName = parameters["ModelName"]; // If provided, must include namespace
            string filename = modelName + "CSVOutput" + DateTime.UtcNow.Ticks.ToString();

            response.ContentType = MimeMapping.GetMimeMapping(filename);
            response.AddHeader("Content-Disposition", "text/csv; filename=" + filename + ";");
            response.BufferOutput = true;

            using (DataContext dataContext = new DataContext(exceptionHandler: MvcApplication.LogException))
            {
                using (DataTable table = dataContext.Connection.RetrieveData($"Select * FROM {modelName}"))
                {
                    StringBuilder sb = new StringBuilder();
                    StreamWriter sw = new StreamWriter(response.OutputStream);
                    IEnumerable<string> columnNames = table.Columns.Cast<DataColumn>().
                                                      Select(column => $"\"{column.ColumnName}\"");
                    sb.AppendLine(string.Join(",", columnNames));

                    foreach (DataRow row in table.Rows)
                    {
                        IEnumerable<string> fields = row.ItemArray.Select(field => $"\"{field.ToString().Replace("\"", "\"\"")}\"");
                        sb.AppendLine(string.Join(",", fields));
                        sw.Write(sb.ToString());
                        sb.Clear();

                    }
                    sw.Flush();
                    response.Flush();

                }

            }
        }
    }
}