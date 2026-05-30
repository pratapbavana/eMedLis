using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eMedLis.Models
{
    public class ReferenceRange
    {
        public int Id { get; set; }
        public int ParameterId { get; set; }
        public string ParameterName { get; set; }
        public int MethodId { get; set; }
        public string Gender { get; set; }
        public string MethodName { get; set; }
        public decimal AgeFromValue { get; set; }
        public string AgeFromUnit { get; set; }
        public decimal AgeToValue { get; set; }
        public string AgeToUnit { get; set; }
        public int AgeFromDays { get; set; }
        public int AgeToDays { get; set; }
        public decimal? NormalMin { get; set; }
        public decimal? NormalMax { get; set; }
        public decimal? CriticalMin { get; set; }
        public decimal? CriticalMax { get; set; }
        public string RangeText { get; set; }
        public bool Active { get; set; }
    }
}
