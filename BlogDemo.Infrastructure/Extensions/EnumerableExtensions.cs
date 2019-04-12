using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace BlogDemo.Infrastructure.Extensions
{
    /// <summary>
    /// 用于集合资源塑形的类
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        ///【这边使用的是扩展方法】
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source">针对于IEnumerable<T>类型接口的扩展方法</param>
        /// <param name="fields">需要返回的字段</param>
        /// <returns></returns>
        public static IEnumerable<ExpandoObject> ToDynamicIEnumerable<TSource>(this IEnumerable<TSource> source, string fields = null)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var expandoObjectList = new List<ExpandoObject>();//返回的结果
            //放需要返回属性的信息，【fields里面的属性信息】,
            var propertyInfoList = new List<PropertyInfo>();
            if (string.IsNullOrWhiteSpace(fields))
            {
                //fields为空则不塑性，原类有多少属性都全部返回

                //获得TSource的全部属性
                var propertyInfos = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                propertyInfoList.AddRange(propertyInfos);
            }
            else
            {
                var fieldsAfterSplit = fields.Split(',').ToList();
                foreach (var field in fieldsAfterSplit)
                {
                    var propertyName = field.Trim();
                    if (string.IsNullOrEmpty(propertyName))
                    {
                        continue;
                    }
                    //通过propertyName取得TSource类的属性
                    var propertyInfo = typeof(TSource).GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (propertyInfo == null)
                    {
                        throw new Exception($"Property {propertyName} wasn't found on {typeof(TSource)}");
                    }
                    propertyInfoList.Add(propertyInfo);
                }
            }

            //遍历source集合的每一条数据
            foreach (TSource sourceObject in source)
            {
                var dataShapedObject = new ExpandoObject();

                //遍历每一个需要返回的属性
                foreach (var propertyInfo in propertyInfoList)
                {
                    //通过反射的方式获得遍历的每一个属性的值
                    var propertyValue = propertyInfo.GetValue(sourceObject);
                    //将ExpandoObject类型转换成IDictionary<string,object>形式，并将属性名称和属性值放入字典中
                    ((IDictionary<string, object>)dataShapedObject).Add(propertyInfo.Name, propertyValue);
                }
                //加入到返回的列表里
                expandoObjectList.Add(dataShapedObject);
            }

            return expandoObjectList;
        }
    }
}
