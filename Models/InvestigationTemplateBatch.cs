using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eMedLis.Models
{
    public class InvestigationTemplateBatch
    {
        public string InvestigationId { get; set; }
        public string InterpretationHtml { get; set; }
        public List<InvestigationTemplateItem> Items { get; set; }
    }
}
