using System.Collections.Generic;

namespace eMedLis.Models
{
    public class ParameterDropdownSaveRequest
    {
        public int ParameterId { get; set; }
        public List<ParameterDropdownValue> Values { get; set; }
    }
}
