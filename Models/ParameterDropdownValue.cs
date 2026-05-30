namespace eMedLis.Models
{
    public class ParameterDropdownValue
    {
        public int Id { get; set; }
        public int ParameterId { get; set; }
        public string ValueText { get; set; }
        public int DisplayOrder { get; set; }
        public bool Active { get; set; }
    }
}
