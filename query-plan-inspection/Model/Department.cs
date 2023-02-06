namespace collecting_filter_params.Model
{
    using GraphQL.AspNet.Attributes;
    using System.Collections.Generic;
    using System.Linq;

    public class Department
    {
        public string Name { get; set; }

        public string Code { get; set; }

        [GraphField("employees")]
        public List<Employee> FilterEmployees(EmployeeFilter filters)
        {
            // if there was a guarantee that this instance was populated externally
            // with items that match the expected incoming filter
            // then we probably don't need to perform filter again...
            // but if this is a general use method
            // it might be safer to perform said filter anyways.
            return this.Employees
                .Where(x =>
                        (filters.Title != null && (x.Title?.Contains(filters.Title ?? "") ?? false))
                        || (filters.Name != null && (x.Name?.Contains(filters.Name ?? "") ?? false)))
                .ToList();
        }

        // Hide the internal implementation of the list of employees
        [GraphSkip]
        public List<Employee> Employees { get; set; }
    }
}