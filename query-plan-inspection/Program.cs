namespace collecting_filter_params
{
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Schemas;
    using Microsoft.AspNetCore.Builder;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var graphqlBuilder = builder.Services.AddGraphQL(o =>
            {
                o.ResponseOptions.ExposeExceptions = true;
            });

            // rebuild the primary query pipeline so we can inject our custom middleware
            // at the appropriate point in the query execution cycle
            var pipelineBuilder = new CustomQueryExecutionPipelineHelper<GraphSchema>(graphqlBuilder.QueryExecutionPipeline);
            pipelineBuilder.AddDefaultMiddlewareComponents();

            var app = builder.Build();
            app.UseRouting();
            app.UseGraphQL();
            app.Run();
        }
    }
}

/*
 *    // return full object
            // (lower methods will filter the data into the ouput

            return new List<DataItem>()
            {
                new DataItem()
                {
                    Name = "data item 1",
                    RetainUntil = DateTime.Now.AddDays(5),
                    CreatedAt = DateTime.Now.AddDays(-3),
                    AllSessions= new List<SessionItem>()
                    {
                        new SessionItem()
                        {
                            Name = "session 1",
                            Handle = "session handle 1",
                            Rules = new List<RuleItem>()
                            {
                                new RuleItem()
                                {
                                    Handle = "rule handle1",
                                    Severity = 3,
                                },
                                new RuleItem()
                                {
                                    Handle = "rule handle2",
                                    Severity = 2,
                                },
                                new RuleItem()
                                {
                                    Handle = "rule handle0",
                                    Severity = 3,
                                },
                            }
                        },
                                  new SessionItem()
                        {
                            Name = "session 2",
                            Handle = "session handle 2",
                            Rules = new List<RuleItem>()
                            {
                                new RuleItem()
                                {
                                    Handle = "rule handle4",
                                    Severity = 4,
                                },
                                new RuleItem()
                                {
                                    Handle = "rule handle5",
                                    Severity = 7,
                                },
                                new RuleItem()
                                {
                                    Handle = "rule handle6",
                                    Severity = 10,
                                },
                            }
                        }
                    }
                }
            };
*/