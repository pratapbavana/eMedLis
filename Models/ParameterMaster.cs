using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eMedLis.Models
{
    public class ParameterMaster
    {
        public int Id { get; set; }
        public int? ParameterHeaderId { get; set; }
        public string ParameterHeaderName { get; set; }
        public string ParameterName { get; set; }
        public string ShortName { get; set; }
        public string Unit { get; set; }
        public string ResultType { get; set; }
        public int DecimalPrecision { get; set; }
        public bool AllowRange { get; set; }
        public bool AllowCriticalRange { get; set; }
        public bool IsCalculated { get; set; }
        public string Formula { get; set; }
        public string DropdownDisplayValues { get; set; }
        public bool Active { get; set; }
    }
}
