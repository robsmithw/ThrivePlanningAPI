using System;

namespace ThrivePlanningAPI.Models
{
    public class Employer
    {
        public string HashKey { get; set; }
        public string RangeKey { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsConfirmed { get; set; } = false;
    }
}
