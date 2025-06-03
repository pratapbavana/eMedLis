using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eMedLis.Models
{
    public class SubDepartment
    {
        public int Id { get; set; }
        public string SubDeptName { get; set; }
        public string DepartmentName { get; set; }
        public int DepartmentId { get; set; }
        public string Header { get; set; }
        public bool Active { get; set; }
    }
}