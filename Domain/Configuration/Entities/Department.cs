using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eMedLis.Domain.Configuration.Entities
{
    public class Departments
    {
        public int Id { get; set; }
        public string DepartmentName { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }

    }
}