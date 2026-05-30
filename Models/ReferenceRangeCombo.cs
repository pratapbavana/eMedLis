using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eMedLis.Models
{
    public class ReferenceRangeCombo
    {
        public int ParameterId { get; set; }
        public string ParameterName { get; set; }
        public int MethodId { get; set; }
        public string MethodName { get; set; }
        public int RangeCount { get; set; }
        public int ActiveCount { get; set; }
    }
}
