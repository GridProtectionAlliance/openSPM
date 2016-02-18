//******************************************************************************************************
//  WebExtensions.cs - Gbtc
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

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Web;
using GSF;
using openSPM.Models;

namespace openSPM
{
    // TODO: Move this code into GSF.Web...
    public static class WebExtensions
    {
        /// <summary>
        /// Performs JavaScript encoding on given string.
        /// </summary>
        /// <param name="text">The string to be encoded.</param>
        public static string JavaScriptEncode(this string text)
        {
            return HttpUtility.JavaScriptStringEncode(text.ToNonNullString());
        }

        /// <summary>
        /// Performs HTML encoding on the given string.
        /// </summary>
        /// <param name="text">The string to be encoded.</param>
        public static string HtmlEncode(this string text)
        {
            return HttpUtility.HtmlEncode(text.ToNonNullString());
        }

        /// <summary>
        /// Performs URL encoding on the given string.
        /// </summary>
        /// <param name="text">The string to be encoded.</param>
        public static string UrlEncode(this string text)
        {
            return HttpUtility.UrlEncode(text.ToNonNullString());
        }

        /// <summary>
        /// Converts a name/value collection to a dictionary.
        /// </summary>
        /// <param name="collection">Name/value collection.</param>
        /// <returns>Dictionary converted from a name/value collection.</returns>
        public static Dictionary<string, string> ToDictionary(this NameValueCollection collection)
        {
            return collection.AllKeys.ToDictionary(key => key, key => collection[key]);
        }

        /// <summary>
        /// Gets query parameters for current request message
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Dictionary<string, string> QueryParameters(this HttpRequestMessage request)
        {
            return request.GetQueryNameValuePairs().ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        public static Dictionary<string, string> LoadDatabaseSettings(this DataContext dataContext, string scope)
        {
            Dictionary<string, string> settings = new Dictionary<string, string>();

            foreach (Settings setting in dataContext.QueryRecords<Settings>("SELECT ID FROM Settings WHERE Scope={0}", scope))
            {
                if (!string.IsNullOrEmpty(setting.Name))
                    settings.Add(setting.Name, setting.Value);
            }

            return settings;
        }
    }
}