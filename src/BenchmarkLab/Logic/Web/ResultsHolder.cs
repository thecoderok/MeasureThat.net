using System.Collections.Generic;
using MeasureThat.Net.Logic.Validation;

namespace BenchmarkLab.Logic.Web
{
    public class ResultsPaginationHolder<T>
    {
        public const int MaxNumberOfPageButtons = 8;

        public IList<T> Entities
        {
            get; private set;
        }

        public int Page
        {
            get; private set;
        }

        public int TotalRecordsCount
        {
            get; private set;
        }

        public int PageSize
        {
            get; private set;
        }

        public IList<int> AvailablePages
        {
            get; private set;
        }

        public int NumberOfPages
        {
            get; private set;
        }

        public bool ButtonPreviousActive
        {
            get
            {
                return Page > 0;
            }
        }

        public bool ButtonNextActive
        {
            get
            {
                return Page < NumberOfPages;
            }
        }

        private IList<int> ComputeAvailablePages()
        {
            var result = new List<int>();
            result.Add(Page);
            for (int i = 0, pageLeft = Page - 1, pageRight = Page + 1; i < MaxNumberOfPageButtons; i++)
            {
                if (IsValidPage(pageLeft))
                {
                    result.Add(pageLeft);
                    pageLeft--;
                }

                if (IsValidPage(pageRight))
                {
                    result.Add(pageRight);
                    pageRight++;
                }
                if (result.Count >= MaxNumberOfPageButtons)
                {
                    break;
                }
            }
            result.Sort();
            return result;
        }

        private bool IsValidPage(int page)
        {
            if (page >= 0 && page < GetNumberOfPages())
            {
                return true;
            }

            return false;
        }

        private int GetNumberOfPages()
        {
            double pageNumDouble = TotalRecordsCount / PageSize;
            int pageNumInt = TotalRecordsCount / PageSize;
            if (pageNumDouble > pageNumInt)
            {
                pageNumInt++;
            }
            return pageNumInt;
        }

        public ResultsPaginationHolder(IList<T> entities, int page, int totalRecordsCount, int pageSize)
        {
            Preconditions.ToBeNonNegative(page);
            Preconditions.ToBeNonNegative(totalRecordsCount);
            Preconditions.ToBeNonNegative(pageSize);
            Preconditions.ToNotBeNull(entities);
            this.Entities = entities;
            this.Page = page;
            this.TotalRecordsCount = totalRecordsCount;
            this.PageSize = pageSize;
            this.NumberOfPages = this.GetNumberOfPages();
            this.AvailablePages = this.ComputeAvailablePages();
        }
    }
}
