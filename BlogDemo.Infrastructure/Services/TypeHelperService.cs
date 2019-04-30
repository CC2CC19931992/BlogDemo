using System.Reflection;

namespace BlogDemo.Infrastructure.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class TypeHelperService : ITypeHelperService
    {
        /// <summary>
        /// fields参数在类型T中是否存在，全都存在则为true,有一个不存在则为false
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fields"></param>
        /// <returns></returns>

        public bool TypeHasProperties<T>(string fields)
        {
            if (string.IsNullOrWhiteSpace(fields))
            {
                return true;
            }

            var fieldsAfterSplit = fields.Split(',');

            foreach (var field in fieldsAfterSplit)
            {
                var propertyName = field.Trim();

                if (string.IsNullOrEmpty(propertyName))
                {
                    continue;
                }

                var propertyInfo = typeof(T)
                    .GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo == null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
