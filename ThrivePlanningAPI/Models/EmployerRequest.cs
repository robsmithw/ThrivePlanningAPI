namespace ThrivePlanningAPI.Models
{
    public class EmployerRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsConfirmed { get; set; } = false;
    }
}
