using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eMedLis.Models
{
    public class InvMaster
    {
        public int Id { get; set; }
        public string InvName { get; set; }
        public string ReportHdr { get; set; }
        public decimal Rate { get; set; }
        public int DeptId { get; set; }
        public string DepartmentName { get; set; }
        public int SubDeptId { get; set; }
        public string SubDeptName { get; set; }
        public int SpecimenId { get; set; }
        public string SpecimenName { get; set; }
        public int VacutainerId { get; set; }
        public string VacutainerName { get; set; }
        public string ReportTime { get; set; }
        public string InvCode { get; set; }
        public string GuideLines { get; set; }
        public bool Active { get; set; }


    }
}