using System;

namespace eMedLis.Models
{
    public class DoctorMaster
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Designation { get; set; }
        public string RegistrationNumber { get; set; }
        public string SubDepartmentIds { get; set; }
        public string SubDepartments { get; set; }
        public bool Active { get; set; }
        public bool HasSignature { get; set; }

        // client upload payload (data URL format)
        public string SignatureBase64 { get; set; }
    }
}
