using System.Collections.Generic;

namespace MeasureThat.Net.Models
{
    using System;

    public class Pager<T>
    {
        public const int MaxNumberOfPages = 10;

        public readonly int CurrentPage;

        public readonly long NumOfPages;

        public readonly long ItemCount;

        public readonly IEnumerable<T> Entites;

        public readonly int ItemsPerPage;

        public readonly int FirstPage;

        public readonly int LastPage;

        public Pager(int currentPage, long itemCount, IEnumerable<T> entites, int itemsPerPage)
        {
            CurrentPage = currentPage;
            ItemCount = itemCount;
            Entites = entites;
            ItemsPerPage = itemsPerPage;
            NumOfPages = (long) Math.Ceiling((double)ItemCount / ItemsPerPage);
            FirstPage = CurrentPage - MaxNumberOfPages / 2;
            if (FirstPage < 0)
            {
                FirstPage = 0;
            }

            LastPage = CurrentPage + (MaxNumberOfPages - (CurrentPage - FirstPage));
        }
    }
}
