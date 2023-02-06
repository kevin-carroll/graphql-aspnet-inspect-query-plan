namespace collecting_filter_params
{
    using GraphQL.AspNet.Configuration;
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Configuration;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Middleware.QueryExecution;
    using GraphQL.AspNet.Security;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// A custom helper that can builder the primary query execution pipeline with some modifications.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this helper constructs a pipeline for.</typeparam>
    public class CustomQueryExecutionPipelineHelper<TSchema> : QueryExecutionPipelineHelper<TSchema>
        where TSchema : class, ISchema
    {
        public CustomQueryExecutionPipelineHelper(ISchemaPipelineBuilder<TSchema, IQueryExecutionMiddleware, QueryExecutionContext> pipelineBuilder)
            : base(pipelineBuilder)
        {
            // ********************************************************
            // Note: If you have subscriptions enabled you would want to
            // base this class off SubscriptionQueryExecutionPipelineHelper<TSchema>
            // ********************************************************
        }

        public override QueryExecutionPipelineHelper<TSchema> AddDefaultMiddlewareComponents(SchemaOptions options = null)
        {
            // clear the default components added during .AddGraphQL()
            this.PipelineBuilder.Clear();

            // Rebuild the pipeline from scratch injecting our custom component
            // at the appropriate point
            this.AddValidateRequestMiddleware()
            .AddRecordQueryMetricsMiddleware();

            if (options?.CacheOptions != null && !options.CacheOptions.Disabled)
                this.AddQueryPlanCacheMiddleware();

            this.AddQueryDocumentParsingMiddleware()
                .AddValidateQueryDocumentMiddleware()
                .AddAssignOperationMiddleware()
                .AddValidateOperationVariableDataMiddleware();

            var authOption = options?.AuthorizationOptions?.Method ?? AuthorizationMethod.PerField;
            if (authOption == AuthorizationMethod.PerRequest)
            {
                this.AddQueryOperationAuthorizationMiddleware();
            }

            this.AddApplyOperationDirectivesMiddleware();
            this.AddQueryPlanCreationMiddleware();

            // *********************************************
            // Inject the custom middlware component just before a query plan
            // is executed. This ensures the plan is in its final state and all
            // fragments, variables, directives etc. are accounted for and locked in place
            // *********************************************
            this.AddArgumentExtractionMiddleware();

            this.AddQueryPlanExecutionMiddleware()
                .AddResultCreationMiddleware();

            return this;
        }

        private QueryExecutionPipelineHelper<TSchema> AddArgumentExtractionMiddleware()
        {
            this.PipelineBuilder.AddMiddleware<ArgumentExtractorMiddleware<TSchema>>(ServiceLifetime.Singleton);
            return this;
        }
    }
}