using System;
using System.Collections.Generic;
using System.Text;

namespace BlogDemo.Core.Entities
{
    /// <summary>
    /// 分页元数据类，记录当前是第几页，当前页的数据，总数据量，总页码数，继承List<T>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PaginatedList<T> : List<T> where T : class
    {
        /// <summary>
        /// 每页的元素个数
        /// </summary>
        public int PageSize { get; set; }
        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; }

        private int _totalItemsCount;
        /// <summary>
        /// 总数量
        /// </summary>
        public int TotalItemsCount
        {
            get => _totalItemsCount;
            set => _totalItemsCount = value >= 0 ? value : 0;
        }

        /// <summary>
        /// 页数量
        /// </summary>
        public int PageCount => TotalItemsCount / PageSize + (TotalItemsCount % PageSize > 0 ? 1 : 0);

        /// <summary>
        /// 是否有前一页
        /// </summary>
        public bool HasPrevious => PageIndex > 0;
        /// <summary>
        /// 是否有后一页
        /// </summary>
        public bool HasNext => PageIndex < PageCount - 1;

        public PaginatedList(int pageIndex, int pageSize, int totalItemsCount, IEnumerable<T> data)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalItemsCount = totalItemsCount;
            AddRange(data);//将data数据加到该集合里【继承的List<T>，加入到该集合】，
        }
    }
}
