using BlogDemo.Core.Entities;
using BlogDemo.Core.Interfaces;
using BlogDemo.Infrastructure.Database;
using BlogDemo.Infrastructure.Extensions;
using BlogDemo.Infrastructure.Resources;
using BlogDemo.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogDemo.Infrastructure.Repositoies
{
    public class PostRepository : IPostRepository
    {
        private readonly MyContext _myContext;
        private readonly IPropertyMappingContainer _propertyMappingContainer;
        public PostRepository(MyContext myContext,IPropertyMappingContainer propertyMappingContainer)//构造方法注入
        {
            _myContext = myContext;
            _propertyMappingContainer = propertyMappingContainer;
        }

        public void AddPost(Post post)
        {
            _myContext.Posts.Add(post);
        }


        public async Task<PaginatedList<Post>> GetAllPostsAsync(PostParameters postParameters)
        {
            var query = _myContext.Posts.AsQueryable();

            //根据传入的标题来进行过滤
            if (!string.IsNullOrEmpty(postParameters.Title))
            {
                var title = postParameters.Title.ToLowerInvariant();//转换为小写
                query = query.Where(x => x.Title.ToLowerInvariant()==title);
            }
            //搜索的话，则是根据传入的条件模糊查询，这里就先不做

            //排序应用
            query = query.ApplySort(postParameters.OrderBy, _propertyMappingContainer.Resolve<PostResource, Post>());
            var count = await query.CountAsync();
            var data = await query
                //跳过多少条返回(如PageIndex=0，跳过0条，返回第一页；PageIndex=1，跳过1*PageSize条，则返回第二页)
                .Skip(postParameters.PageIndex * postParameters.PageSize)
                .Take(postParameters.PageSize)//每页返回多少个
                .ToListAsync();
            return new PaginatedList<Post>(postParameters.PageIndex, postParameters.PageSize, count, data);
            //return await _myContext.Posts.ToListAsync();
        }

        public async Task<Post> GetPostByIdAsync(int id)
        {
            return await _myContext.Posts.FindAsync(id);
        }
    }
}
