using System;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace BlogDemo.Api.Helpers
{
    /// <summary>
    /// 实现了IActionConstraint接口 表示对于Api里的Action的约束，具体可以看看IActionConstraint的文档
    /// 这个类是对于Api里的请求头的约束特性
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
    public class RequestHeaderMatchingMediaTypeAttribute : Attribute, IActionConstraint
    {
        private readonly string _requestHeaderToMatch;
        private readonly string[] _mediaTypes;

        public RequestHeaderMatchingMediaTypeAttribute(string requestHeaderToMatch, string[] mediaTypes)
        {
            _requestHeaderToMatch = requestHeaderToMatch;
            _mediaTypes = mediaTypes;
        }

        public bool Accept(ActionConstraintContext context)
        {
            var requestHeaders = context.RouteContext.HttpContext.Request.Headers;
            if (!requestHeaders.ContainsKey(_requestHeaderToMatch))
            {
                return false;
            }

            foreach (var mediaType in _mediaTypes)
            {
                var mediaTypeMatches = string.Equals(requestHeaders[_requestHeaderToMatch].ToString(),
                    mediaType, StringComparison.OrdinalIgnoreCase);
                if (mediaTypeMatches)
                {
                    return true;
                }
            }

            return false;
        }

        public int Order { get; } = 0;
    }
}
