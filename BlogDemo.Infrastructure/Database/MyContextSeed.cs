using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BlogDemo.Core.Entities;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;

namespace BlogDemo.Infrastructure.Database
{
    public class MyContextSeed
    {
        public static async Task SeedAsync(MyContext myContext,
                          ILoggerFactory loggerFactory, int retry = 0)
        {
            int retryForAvailability = retry;
            try
            {
                // TODO: Only run this if using a real database
                // myContext.Database.Migrate();

                if (!myContext.Posts.Any())//如果post表里没有任何数据 则会添加以下初始数据
                {
                    myContext.Posts.AddRange(
                        new List<Post>{
                            new Post{
                                Title = "Post Title 1",
                                Body = "Post Body 1",
                                Author = "Dave",
                                LastModified = DateTime.Now
                            },
                            new Post{
                                Title = "Post Title 2",
                                Body = "Post Body 2",
                                Author = "Dave",
                                LastModified = DateTime.Now
                            },
                            new Post{
                                Title = "Post Title 3",
                                Body = "Post Body 3",
                                Author = "Dave",
                                LastModified = DateTime.Now
                            },
                            new Post{
                                Title = "Post Title 4",
                                Body = "Post Body 4",
                                Author = "Dave",
                                LastModified = DateTime.Now
                            },
                            new Post{
                                Title = "Post Title 5",
                                Body = "Post Body 5",
                                Author = "Dave",
                                LastModified = DateTime.Now
                            },
                            new Post{
                                Title = "Post Title 6",
                                Body = "Post Body 6",
                                Author = "Dave",
                                LastModified = DateTime.Now
                            },
                            new Post{
                                Title = "Post Title 7",
                                Body = "Post Body 7",
                                Author = "Dave",
                                LastModified = DateTime.Now
                            },
                            new Post{
                                Title = "Post Title 8",
                                Body = "Post Body 8",
                                Author = "Dave",
                                LastModified = DateTime.Now
                            }
                        }
                    );
                    await myContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                if (retryForAvailability < 10)
                {
                    retryForAvailability++;
                    var logger = loggerFactory.CreateLogger<MyContextSeed>();
                    logger.LogError(ex.Message);
                    await SeedAsync(myContext, loggerFactory, retryForAvailability);
                }
            }
        }
    }
}
