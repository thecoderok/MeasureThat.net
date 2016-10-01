using System.Collections.Generic;

namespace MeasureThat.Net.Models
{
    using System;

    public class Pager<T>
    {
        public readonly int CurrentPage;

        public readonly long NumOfPages;

        public readonly long ItemCount;

        public readonly IEnumerable<T> Entites;

        public readonly int ItemsPerPage;

        public Pager(int currentPage, long itemCount, IEnumerable<T> entites, int itemsPerPage)
        {
            CurrentPage = currentPage;
            ItemCount = itemCount;
            Entites = entites;
            ItemsPerPage = itemsPerPage;
            NumOfPages = (long) Math.Ceiling((double)ItemCount / ItemsPerPage);
        }
    }
}
