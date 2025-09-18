namespace FinalGraduationProject.Models
{
    public class Address
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        public string Street { get; set; } = "";
        public string City { get; set; } = "";
        public string State { get; set; } = "";
        public string PostalCode { get; set; } = "";
        public string Country { get; set; } = "";
    }

}
