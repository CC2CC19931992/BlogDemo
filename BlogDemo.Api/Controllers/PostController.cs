using AutoMapper;
using BlogDemo.Core.Entities;
using BlogDemo.Core.Interfaces;
using BlogDemo.Infrastructure.Database;
using BlogDemo.Infrastructure.Extensions;
using BlogDemo.Infrastructure.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlogDemo.Api.Controllers
{
    [Route("api/posts")]
    public class PostController : Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PostController> _logger;
        private readonly ILogger _loggerF;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IUrlHelper _urlHelper;

        public PostController(
            IPostRepository postRepository,
            IUnitOfWork unitOfWork,
            ILogger<PostController> logger,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IUrlHelper urlHelper)
        {
            _postRepository = postRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _loggerF = loggerFactory.CreateLogger("PostController");
            _configuration = configuration;
            _mapper = mapper;//将AutoMapper注入到这个PostController中
            _urlHelper = urlHelper;
        }


        [HttpGet(Name ="GetPosts")]//给方法取个名叫GetPosts
        public async Task<IActionResult> Get(PostParameters postParameters)
        {
            var postList = await _postRepository.GetAllPostsAsync(postParameters);
            //var a =postList.FirstOrDefault();
            var postResource = _mapper.Map<IEnumerable<Post>, IEnumerable<PostResource>>(postList);

            var shapedPostResources = postResource.ToDynamicIEnumerable(postParameters.Fields);//实现集合资源塑形

            var previousPageLink = postList.HasPrevious ?//如果有前一页，则生成前一页链接
             CreatePostUri(postParameters,
                 PaginationResourceUriType.PreviousPage) : null;//生成前一页的链接

            var nextPageLink = postList.HasNext ?//如果有后一页，则生成后一页链接
                CreatePostUri(postParameters,
                    PaginationResourceUriType.NextPage) : null;//生成后一页的链接

            var meta = new
            {
                postList.PageSize,
                postList.PageIndex,
                postList.TotalItemsCount,
                postList.PageCount,
                previousPageLink,
                nextPageLink
            };
            Response.Headers.Add("X-Pagination", 
                JsonConvert.SerializeObject(meta,
                //加上这个设定是将head转换成前端规范的首字母小写的CamelCase规范
                new JsonSerializerSettings { ContractResolver=new CamelCasePropertyNamesContractResolver() }
                ));
            //var v = _configuration["Key1"];//当Key1发生变化时 重新跑下这里也会加载最新的
            //throw new Exception("Error!!!!!");
            //_logger.LogInformation("Get All Posts......");
            //_loggerF.LogError("Get All Posts......");
            //return Ok(postResource);
            return Ok(shapedPostResources);//塑形后返回结果
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id, string fields = null)
        {
            var post =await _postRepository.GetPostByIdAsync(id);
            if(post == null)
            {
                return NotFound();
            }
            var postResource = _mapper.Map<Post, PostResource>(post);
            var shapedPostResource = postResource.ToDynamic(fields);//实现单个对象的塑形

            //return Ok(postResource);
            return Ok(shapedPostResource);
        }


        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var newPost = new Post
            {
                Author = "admin",Body="12123123213",Title="Titel xc",LastModified=DateTime.Now
            };
            _postRepository.AddPost(newPost);
            //为什么要将保存到数据库的操作单独放到UnitOfWork里而不放在Repository里
            //Repository中文意思是仓储，类似于集合。集合有查询排序删除的操作，而保存

            await _unitOfWork.SaveAsync();
            return Ok();
        }


        
        private string CreatePostUri(PostParameters parameters, PaginationResourceUriType uriType)
        {
            switch (uriType)
            {
                case PaginationResourceUriType.PreviousPage:
                    var previousParameters = new
                    {
                        pageIndex = parameters.PageIndex - 1,
                        pageSize = parameters.PageSize,
                        orderBy = parameters.OrderBy,
                        fields = parameters.Fields,
                        title = parameters.Title
                    };
                    return _urlHelper.Link("GetPosts", previousParameters);
                case PaginationResourceUriType.NextPage:
                    var nextParameters = new
                    {
                        pageIndex = parameters.PageIndex + 1,
                        pageSize = parameters.PageSize,
                        orderBy = parameters.OrderBy,
                        fields = parameters.Fields,
                        title = parameters.Title
                    };
                    return _urlHelper.Link("GetPosts", nextParameters);
                default:
                    var currentParameters = new
                    {
                        pageIndex = parameters.PageIndex,
                        pageSize = parameters.PageSize,
                        orderBy = parameters.OrderBy,
                        fields = parameters.Fields,
                        title = parameters.Title
                    };
                    return _urlHelper.Link("GetPosts", currentParameters);
            }
        }

    }
}
