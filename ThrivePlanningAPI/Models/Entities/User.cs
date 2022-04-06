using System;

namespace ThrivePlanningAPI.Models.Entities
{
    public class User : Entity<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Username { get; set; }
        public UserType Type { get; set; }
        public virtual Company Company { get; set; }
        public Guid CompanyId { get; set; }
        public bool IsConfirmed { get; set; } = false;
    }

    public enum UserType
    {
        Employer,
        Employee
    }
}
