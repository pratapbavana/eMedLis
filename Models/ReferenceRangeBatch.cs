using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eMedLis.Models
{
    public class ReferenceRangeBatch
    {
        public int ParameterId { get; set; }
        public int MethodId { get; set; }
        public string Mode { get; set; }
        public List<ReferenceRange> Ranges { get; set; }
    }
}
