using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Client_Invoice_System.Models
{
    public class PaymentProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string IBANNumber { get; set; }

        public string Currency { get; set; }

        public string AccountTitle { get; set; }

        public string AccountNumber { get; set; }

        // Foreign Key
        [ForeignKey("OwnerProfile")]
        public int OwnerId { get; set; }

        // Navigation Property
        public virtual OwnerProfile Owner { get; set; }
    }

}
