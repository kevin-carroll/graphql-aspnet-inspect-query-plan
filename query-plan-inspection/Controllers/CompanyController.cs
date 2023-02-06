namespace collecting_filter_params.Controllers
{
    using collecting_filter_params.Model;
    using GraphQL.AspNet.Attributes;
    using GraphQL.AspNet.Controllers;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    [GraphRoute("company")]
    public class SampleController : GraphController
    {
        [Query("divisions/list")]
        public async Task<List<CompanyDivision>> RetrieveCompanies(CompanyFilter filters)
        {
            var extractedArguments = this.Request.Items["operation-arguments"] as IDictionary<string, object>;

            DepartmentFilter departmentFilter = extractedArguments["company.divisions.list.departments[filters]"] as DepartmentFilter;
            EmployeeFilter employeeFilter = extractedArguments["company.divisions.list.departments.employees[filters]"] as EmployeeFilter;

            // use all 3 filters to fetch one large object from a database
            // or deserialize a json object from an external http call

            // whatever the method lets assume that a fully hydrated set of objects now exists
            // that conforms to the chosen filter set.

            // perhaps that information was then used to populate a data model that reprsents the schema
            var divisions = new List<CompanyDivision>()
            {
                new CompanyDivision()
                {
                    CreateDateUTC = DateTime.UtcNow.AddDays(-30),
                    Name = "Division 1",
                    Departments = new List<Department>()
                    {
                        new Department()
                        {
                            Code = "RES",
                            Name = "Research",
                            Employees = new List<Employee>()
                            {
                                new Employee()
                                {
                                    Name = "Bob Smith",
                                    Title = "Lead Researcher",
                                },
                                new Employee()
                                {
                                    Name = "Jane Doe",
                                    Title = "Ass. Researcher",
                                },
                            },
                        },

                        new Department()
                        {
                            Code = "ACC",
                            Name = "Accounting",
                            Employees = new List<Employee>()
                            {
                                new Employee()
                                {
                                    Name = "Joe Everyman",
                                    Title = "Accountant",
                                },
                                new Employee()
                                {
                                    Name = "John Jingle",
                                    Title = "Bookkeeper",
                                },
                            },
                        },
                    }
                }
            };


            return await Task.FromResult(divisions);
        }
    }
}