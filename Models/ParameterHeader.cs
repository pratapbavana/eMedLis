using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eMedLis.Models
{
    public class ParameterHeader
    {
        public int Id { get; set; }
        public string HeaderName { get; set; }
        public bool Active { get; set; }
    }
}
