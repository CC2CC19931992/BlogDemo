namespace BlogDemo.Core.Entities
{
    /// <summary>
    /// 具体查询的分页的实体类，集成抽象类
    /// </summary>
    public class PostParameters : QueryParameters
    {
        public string Title { get; set; }
    }
}