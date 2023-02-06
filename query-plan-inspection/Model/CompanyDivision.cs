namespace collecting_filter_params.Model
{
    using GraphQL.AspNet.Attributes;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CompanyDivision
    {
        public string Name { get; set; }

        public DateTime CreateDateUTC { get; set; }

        // Expose this method (with its arguments) to the schema as a field
        [GraphField("departments")]
        public List<Department> FilterDepartments(DepartmentFilter filters)
        {
            // if there was a guarantee that this instance was populated externally
            // with items that match the expected incoming filter
            // then we probably don't need to perform filter again...
            // but if this is a general use method
            // it might be safer to perform said filter anyways.
            return this.Departments.Where(x => (x.Name?.Contains(filters.NameLike ?? "") ?? false)).ToList();
        }

        // Hide the internal implementation of departments
        [GraphSkip]
        public List<Department> Departments { get; set; }
    }
}
