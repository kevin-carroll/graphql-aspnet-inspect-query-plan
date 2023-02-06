
## Inspecting a Query Plan

This project shows a way to introgate parts of the query plan to extract pieces of information. In this example we extract and index the arguments passed to fields requested in a query.

This project makes use of a custom middleware component to extract the data at the "query level" and makes that information available to whatever may need it via the request `Items` collection.

## Notable Files

* `ArgumentExtractorMiddlewareComponent.cs`  The custom middleware component injected into the query execution pipeline
* `CustomQueryExecutionPipelineBuilder.cs` Since we want to inject our component into the middle of the query execution pipeline (and not the end), this helper class is used to rebuild the pipeline in the proper order with our middleware injected at the appropriate point.
* `CompanyController.cs` A controller where all 3 "filters" objects are shown to be available if needed.

**Disclaimer:** The query plan is a low-level, implementation detail of the library and the way it handles NOT a standard defined by the graphql specification.  While unlikely, it may change in the future if a need arises.

Sample Query:
```graphql
query {
  company {
    divisions {
      list(filters: { name: "Div 1" }) {
        name
        createDateUTC
        departments(filters: { nameLike: "Res" }) {
          code
          name
          employees(filters: { name: "Doe" }) {
            title
            name
          }
        }
      }
    }
  }
}
```
