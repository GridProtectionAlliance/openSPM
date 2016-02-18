//******************************************************************************************************
//  PagedList.cs - Gbtc
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
using System.Linq;

// TODO: Move into GSF

namespace openSPM
{
    public interface IPagedList
    {
        int TotalCount
        {
            get;
        }

        int PageCount
        {
            get;
        }

        int Page
        {
            get;
        }

        int PageSize
        {
            get;
        }
    }

    public class PagedList<T> : List<T>, IPagedList
    {
        public int TotalCount
        {
            get;
            private set;
        }

        public int PageCount
        {
            get;
            private set;
        }

        public int Page
        {
            get;
            private set;
        }

        public int PageSize
        {
            get;
            private set;
        }

        public PagedList(IEnumerable<T> source, int page, int pageSize)
        {
            TotalCount = source.Count();
            PageCount = GetPageCount(pageSize, TotalCount);
            Page = page < 1 ? 0 : page - 1;
            PageSize = pageSize;

            AddRange(source.Skip(Page * PageSize).Take(PageSize).ToList());
        }

        private int GetPageCount(int pageSize, int totalCount)
        {
            if (pageSize == 0)
                return 0;

            var remainder = totalCount % pageSize;
            return (totalCount / pageSize) + (remainder == 0 ? 0 : 1);
        }
    }

    public static class PagedListExtensions
    {
        public static PagedList<T> ToPagedList<T>(this IEnumerable<T> source, int page, int pageSize)
        {
            return new PagedList<T>(source, page, pageSize);
        }
    }
}
