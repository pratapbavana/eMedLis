using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eMedLis.Domain.Configuration.Entities
{
    public class Lookups
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public string ItemType { get; set; }
        public bool Active { get; set; }
    }
}