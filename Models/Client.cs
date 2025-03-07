using System.ComponentModel.DataAnnotations;

namespace Client_Invoice_System.Models
{
    public class Client
    {
        [Key]
        public int ClientId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
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
