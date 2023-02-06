namespace collecting_filter_params
{
    using GraphQL.AspNet.Execution.Contexts;
    using GraphQL.AspNet.Interfaces.Execution;
    using GraphQL.AspNet.Interfaces.Execution.Variables;
    using GraphQL.AspNet.Interfaces.Middleware;
    using GraphQL.AspNet.Interfaces.Schema;
    using GraphQL.AspNet.Middleware;
    using GraphQL.AspNet.Schemas.TypeSystem;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A query pipeline middleware component that can inspect a completed query plan and
    /// extract all supplied argument sets and put them into a dictionary that is made available
    /// to all controllers that are invoked on the reuqest.
    /// </summary>
    /// <typeparam name="TSchema">The type of the schema this middleware component is invoked against (a new copy is
    /// made per registered schema).</typeparam>
    public class ArgumentExtractorMiddleware<TSchema> : IQueryExecutionMiddleware
        where TSchema : class, ISchema
    {
        public ArgumentExtractorMiddleware()
        {
            // DI can be wired up and scoped to the request
            // depending on how you inject the component into the pipeline.
            // As designed this example project injects it as singleton.
        }

        public async Task InvokeAsync(QueryExecutionContext context, GraphMiddlewareInvocationDelegate<QueryExecutionContext> next, CancellationToken cancelToken)
        {
            // Has the pipeline generated a queryplan that is "usable"
            if (context.IsValid && !context.IsCancelled && context.QueryPlan != null)
            {
                // walk the invocation context tree and extract the arguments for each field
                // that has them defined
                var extractedArguments = new Dictionary<string, object>();
                this.ExtractArguments(
                    context.QueryPlan.Operation.FieldContexts,
                    context.ResolvedVariables,
                    extractedArguments);

                // context.QueryRequest.Items is a developer-controlled, shared dictionary available to all
                // controllers as `this.Request.Items`
                context.QueryRequest.Items.Add("operation-arguments", extractedArguments);
            }

            await next(context, cancelToken);
        }

        private void ExtractArguments(IFieldInvocationContextCollection fieldSelectionSet, IResolvedVariableCollection resolvedVariables, Dictionary<string, object> foundArguments)
        {
            if (fieldSelectionSet.Count == 0)
                return;

            // Find all the fields that have arguments in this selection set
            // and index them by their "path" within the query document
            //
            // Note: Any given selection set may have more than one entry per unique field alias
            // due to the possibility of multiple concrete invocations for parent fields that return an interface (or unions).
            // However, all copies will contain the same arguments as there is only
            // one set of arguments passed on the query for a given field.
            foreach (var aliasGroup in fieldSelectionSet.GroupBy(x => x.FieldDocumentPart.Alias))
            {
                // we only care about the arguments, so it doesn't matter which
                // invocation context we extract them from
                var fieldSelection = aliasGroup.First();
                var fieldPathKey = fieldSelection.FieldDocumentPart.Path.ToDotString();

                if (fieldSelection.Arguments == null)
                    continue;

                foreach (var argData in fieldSelection.Arguments)
                {
                    // skip internal, "captured" arguments (like the source parameter on a TypeExtension)
                    if (argData.Argument.ArgumentModifiers.IsNotPartOfTheSchema())
                        continue;

                    // resolve the .NET object from the raw data provided in the query text
                    // or the variable data provided on the request
                    var actualValue = argData.Value.Resolve(resolvedVariables);

                    // key the data based on where the argument is in the query document
                    // "field1.field2[firstArgumentName]"
                    // "field1.field2[secondArgumentName]"
                    // "field1.field2.field3[anotherArgumentName]"
                    foundArguments.Add($"{fieldPathKey}[{argData.Name}]", actualValue);
                }

                // recursively call down through the query plan
                this.ExtractArguments(fieldSelection.ChildContexts, resolvedVariables, foundArguments);
            }
        }
    }
}