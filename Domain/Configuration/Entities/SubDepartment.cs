using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eMedLis.Domain.Configuration.Entities
{
    public class SubDepartments
    {
        public int Id { get; set; }
        public string SubDeptName { get; set; }
        public string DepartmentName { get; set; }
        public int DepartmentId { get; set; }
        public string Header { get; set; }
        public bool Active { get; set; }
    }
}