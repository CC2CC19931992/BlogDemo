using AutoMapper;
using BlogDemo.Api.Helpers;
using BlogDemo.Core.Entities;
using BlogDemo.Core.Interfaces;
using BlogDemo.Infrastructure.Database;
using BlogDemo.Infrastructure.Extensions;
using BlogDemo.Infrastructure.Resources;
using BlogDemo.Infrastructure.Services;
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
        private readonly ITypeHelperService _typeHelperService;
        private readonly IPropertyMappingContainer _propertyMappingContainer;
        public PostController(
            IPostRepository postRepository,
            IUnitOfWork unitOfWork,
            ILogger<PostController> logger,
            ILoggerFactory loggerFactory,
            IConfiguration configuration,
            IMapper mapper,
            IUrlHelper urlHelper,
            ITypeHelperService typeHelperService,
            IPropertyMappingContainer propertyMappingContainer)
        {
            _postRepository = postRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _loggerF = loggerFactory.CreateLogger("PostController");
            _configuration = configuration;
            _mapper = mapper;//将AutoMapper注入到这个PostController中
            _urlHelper = urlHelper;
            _typeHelperService = typeHelperService;
            _propertyMappingContainer = propertyMappingContainer;
        }

        //获取集合资源
        [HttpGet(Name = "GetPosts")]//给方法取个名叫GetPosts
        [RequestHeaderMatchingMediaType("Accept", new[] { "application/json" })]//新增媒体类型参数，在Header里是叫做Accept的key
        public async Task<IActionResult> Get(PostParameters postParameters)
        {
            if (!_propertyMappingContainer.ValidateMappingExistsFor<PostResource, Post>(postParameters.OrderBy))
            {
                //如果需要查询排序的字段不存在，则返回400错误
                return BadRequest("Can't finds fields for sorting");
            }

            if (!_typeHelperService.TypeHasProperties<PostResource>(postParameters.Fields))
            {
                //如果需要查询的字段不存在，则返回400错误
                return BadRequest("Fields not exist.");
            }

            var postList = await _postRepository.GetAllPostsAsync(postParameters);
            //var a =postList.FirstOrDefault();
            var postResource = _mapper.Map<IEnumerable<Post>, IEnumerable<PostResource>>(postList);
            //如果Headers里面有注明输出的媒体类型为application/vnd.cgzl.hateoas+json 则走下面，带links的

            var previousPageLink = postList.HasPrevious ?
           CreatePostUri(postParameters,
               PaginationResourceUriType.PreviousPage) : null;

            var nextPageLink = postList.HasNext ?
                CreatePostUri(postParameters,
                    PaginationResourceUriType.NextPage) : null;

            var meta = new
            {
                postList.TotalItemsCount,
                postList.PageSize,
                postList.PageIndex,
                postList.PageCount,
                previousPageLink,
                nextPageLink
            };

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(meta, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));

            return Ok(postResource.ToDynamicIEnumerable(postParameters.Fields));
        }


        [HttpGet(Name = "GetPosts")]
        //新增媒体类型参数，在Header里是叫做Accept的key   application/vnd.cgzl.hateoas+json
        [RequestHeaderMatchingMediaType("Accept", new[] { "application/vnd.cgzl.hateoas+json" })]
        public async Task<IActionResult> GetHateoas(PostParameters postParameters)
        {
            if (!_propertyMappingContainer.ValidateMappingExistsFor<PostResource, Post>(postParameters.OrderBy))
            {
                return BadRequest("Can't finds fields for sorting.");
            }

            if (!_typeHelperService.TypeHasProperties<PostResource>(postParameters.Fields))
            {
                return BadRequest("Fields not exist.");
            }

            var postList = await _postRepository.GetAllPostsAsync(postParameters);
            var postResources = _mapper.Map<IEnumerable<Post>, IEnumerable<PostResource>>(postList);

            var shapedPostResources = postResources.ToDynamicIEnumerable(postParameters.Fields);
            var shapedWithLinks = shapedPostResources.Select(x =>
            {
                var dict = x as IDictionary<string, object>;
                var postLinks = CreateLinksForPost((int)dict["Id"], postParameters.Fields);
                dict.Add("links", postLinks);
                return dict;
            });
            var links = CreateLinksForPosts(postParameters, postList.HasPrevious, postList.HasNext);
            var result = new
            {
                value = shapedWithLinks,
                links
            };

            var meta = new
            {
                postList.PageSize,
                postList.PageIndex,
                postList.TotalItemsCount,
                postList.PageCount
            };
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(meta, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }));

            return Ok(result);
        }


        //获取单个资源
        [HttpGet("{id}", Name = "GetPost")]
        public async Task<IActionResult> Get(int id, string fields = null)
        {
            //这里是查询单个 所以不需要对排序的字段做验证
            if (!_typeHelperService.TypeHasProperties<PostResource>(fields))
            {
                //如果需要查询的字段不存在，则返回400错误
                return BadRequest("Fields not exist.");
            }

            var post = await _postRepository.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            var postResource = _mapper.Map<Post, PostResource>(post);
            var shapedPostResource = postResource.ToDynamic(fields);//实现单个对象的塑形

            //创建资源链接
            var links = CreateLinksForPost(id, fields);
            var result = shapedPostResource as IDictionary<string, Object>;
            result.Add("links", links);
            //return Ok(postResource);
            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var newPost = new Post
            {
                Author = "admin",
                Body = "12123123213",
                Title = "Titel xc",
                LastModified = DateTime.Now
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



        /// <summary>
        /// 为单个资源创建资源相关链接
        /// </summary>
        /// <param name="id">post的主键</param>
        /// <param name="fields">表示相关的链接的字段（塑形的字段）</param>
        /// <returns></returns>
        private IEnumerable<LinkResource> CreateLinksForPost(int id, string fields = null)
        {
            var links = new List<LinkResource>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                    new LinkResource(
                        //GetPost是Action的名，也就是这个页面的 async Task<IActionResult> Get(int id, string fields = null)方法
                        // [HttpGet("{id}",Name ="GetPost")]
                        //第二个参数id使用的是匿名类，而fields不需要塑形 所以可以不用传
                        _urlHelper.Link("GetPost", new { id }), "self", "GET"));
            }
            else
            {
                //这边是需要塑形的 因为fields不为空
                links.Add(
                    new LinkResource(
                        _urlHelper.Link("GetPost", new { id, fields }), "self", "GET"));
            }


            //先新增删除字段的link Action暂时没做
            links.Add(
                new LinkResource(
                    _urlHelper.Link("DeletePost", new { id }), "delete_post", "DELETE"));

            return links;
        }

        /// <summary>
        /// 为多个资源创建资源相关链接（前一页 后一页的链接）
        /// </summary>
        /// <param name="postResourceParameters"></param>
        /// <param name="hasPrevious"></param>
        /// <param name="hasNext"></param>
        /// <returns></returns>
        private IEnumerable<LinkResource> CreateLinksForPosts(PostParameters postResourceParameters,
    bool hasPrevious, bool hasNext)
        {
            var links = new List<LinkResource>
            {
                new LinkResource(
                    CreatePostUri(postResourceParameters, PaginationResourceUriType.CurrentPage),
                    "self", "GET")
            };

            if (hasPrevious)
            {
                links.Add(
                    new LinkResource(
                        CreatePostUri(postResourceParameters, PaginationResourceUriType.PreviousPage),
                        "previous_page", "GET"));
            }

            if (hasNext)
            {
                links.Add(
                    new LinkResource(
                        CreatePostUri(postResourceParameters, PaginationResourceUriType.NextPage),
                        "next_page", "GET"));
            }

            return links;
        }


    }
}
