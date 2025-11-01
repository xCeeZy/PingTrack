using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PingTrack.AppData
{
    public sealed class PaginationService<T>
    {
        #region Поля и свойства
        private List<T> allItems;
        private int currentPage;
        private int pageSize;

        public int CurrentPage
        {
            get { return currentPage; }
        }

        public int TotalPages
        {
            get
            {
                if (allItems == null || allItems.Count == 0) return 0;
                return (int)Math.Ceiling((double)allItems.Count / pageSize);
            }
        }

        public bool HasNextPage
        {
            get { return currentPage < TotalPages; }
        }

        public bool HasPreviousPage
        {
            get { return currentPage > 1; }
        }
        #endregion

        #region Конструктор
        public PaginationService(int pageSize)
        {
            this.pageSize = pageSize;
            this.currentPage = 1;
            this.allItems = new List<T>();
        }
        #endregion

        #region Методы управления
        public void SetItems(List<T> items)
        {
            allItems = items != null ? new List<T>(items) : new List<T>();
            currentPage = 1;
        }

        public List<T> GetCurrentPage()
        {
            if (allItems == null || allItems.Count == 0) return new List<T>();

            int skip = (currentPage - 1) * pageSize;
            return allItems.Skip(skip).Take(pageSize).ToList();
        }

        public void NextPage()
        {
            if (HasNextPage) currentPage++;
        }

        public void PreviousPage()
        {
            if (HasPreviousPage) currentPage--;
        }

        public void GoToPage(int pageNumber)
        {
            if (pageNumber >= 1 && pageNumber <= TotalPages)
                currentPage = pageNumber;
        }
        #endregion
    }
}
