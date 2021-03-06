﻿using System.Collections.Generic;

namespace Okra.Data
{
    public struct DataListPageResult<T>
    {
        // *** Constructors ***

        public DataListPageResult(int? totalItemCount, int? itemsPerPage, int? pageNumber, IList<T> page)
            : this()
        {
            TotalItemCount = totalItemCount;
            ItemsPerPage = itemsPerPage;
            PageNumber = pageNumber;
            Page = page;
        }

        // *** Properties ***

        public int? TotalItemCount { get; private set; }
        public int? ItemsPerPage { get; private set;}
        public int? PageNumber { get; private set; }
        public IList<T> Page { get; private set; }
    }
}
