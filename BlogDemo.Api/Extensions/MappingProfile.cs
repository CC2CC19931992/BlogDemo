using AutoMapper;
using BlogDemo.Core.Entities;
using BlogDemo.Infrastructure.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogDemo.Api.Extensions
{
    /// <summary>
    /// 各个类映射关系的配置，通过AutoMapper组件实现
    /// </summary>
    public class MappingProfile: Profile
    {
        public MappingProfile()
        {
            //建立了一个从post到postResource的映射.字段名完全一样的就会产生映射关系，
            //如果不一样，则可以通过ForMember方法，在里面定义映射，如下就是PostResource的UpdateTime字段对应的是Post的LastModified
            CreateMap<Post, PostResource>().ForMember(dest=>dest.UpdateTime,opt=>opt.MapFrom(src=>src.LastModified));

            CreateMap<PostResource, Post>();//建立了一个从postResource到post的映射
        }
    }
}
