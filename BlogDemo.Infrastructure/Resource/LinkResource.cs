namespace BlogDemo.Infrastructure.Resources
{
    /// <summary>
    /// 链接资源
    /// </summary>
    public class LinkResource
    {
        public LinkResource(string href, string rel, string method)
        {
            Href = href;
            Rel = rel;
            Method = method;
        }
        /// <summary>
        /// 链接URL
        /// </summary>
        public string Href { get; set; }
        /// <summary>
        /// 动作类型
        /// </summary>
        public string Rel { get; set; }
        /// <summary>
        /// 方法
        /// </summary>
        public string Method { get; set; }
    }
}
