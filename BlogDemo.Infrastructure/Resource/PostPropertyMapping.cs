using System;
using System.Collections.Generic;
using System.Text;
using BlogDemo.Core.Entities;
using BlogDemo.Infrastructure.Services;

namespace BlogDemo.Infrastructure.Resources
{
    /// <summary>
    /// 继承PropertyMapping抽象类，实现属性间的映射
    /// </summary>
    public class PostPropertyMapping : PropertyMapping<PostResource, Post>
    {
        public PostPropertyMapping() : base(
            new Dictionary<string, List<MappedProperty>>
                (StringComparer.OrdinalIgnoreCase)
            {
                [nameof(PostResource.Title)] = new List<MappedProperty>
                    {
                        new MappedProperty{ Name = nameof(Post.Title), Revert = false}
                    },//属性间的映射
                [nameof(PostResource.Body)] = new List<MappedProperty>
                    {
                        new MappedProperty{ Name = nameof(Post.Body), Revert = false}
                    },//属性间的映射
                [nameof(PostResource.Author)] = new List<MappedProperty>
                    {
                        new MappedProperty{ Name = nameof(Post.Author), Revert = false}
                    }//属性间的映射
            })
        {
        }
    }
}
