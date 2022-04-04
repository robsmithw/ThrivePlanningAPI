namespace ThrivePlanningAPI.Models.Requests
{
    public class CompanyRequest
    {
        public string CompanyAdminFirstName { get; set; }
        public string CompanyAdminLastName { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string TaxId { get; set; }
        public string Industry { get; set; }
    }
}
