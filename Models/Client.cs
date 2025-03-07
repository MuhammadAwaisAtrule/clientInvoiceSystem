using System.ComponentModel.DataAnnotations;

namespace Client_Invoice_System.Models
{
    public class Client
    {
        [Key]
        public int ClientId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; }

        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Currency { get; set; }
        public string Country { get; set; }
        public DateTime DueDate { get; set; }
        public string ClientIdentifier { get; set; }

        // ✅ Ensure Proper Relationship
        public virtual ActiveClient ActiveClient { get; set; }
        public virtual ICollection<Resource> Resources { get; set; }
        public virtual ICollection<ClientProfileCrossTable> ClientProfileCrosses { get; set; }
    }
}
