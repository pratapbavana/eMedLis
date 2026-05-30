using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eMedLis.Models
{
    public class InvestigationTemplateItem
    {
        public int Id { get; set; }
        public string InvestigationId { get; set; }
        public string InvestigationName { get; set; }
        public string ItemType { get; set; } // Header | Parameter
        public int? HeaderId { get; set; }
        public string HeaderName { get; set; }
        public int? ParameterId { get; set; }
        public string ParameterName { get; set; }
        public int? MethodId { get; set; }
        public string MethodName { get; set; }
        public int DisplayOrder { get; set; }
        public bool Active { get; set; }
    }
}
