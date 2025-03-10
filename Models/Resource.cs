using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Client_Invoice_System.Models
{
    public class Resource
    {
        [Key]
        public int ResourceId { get; set; }

        [ForeignKey("Client")]
        public int ClientId { get; set; }

        [Required]
        public string ResourceName { get; set; }

        [ForeignKey("Employee")]
        public int EmployeeId { get; set; }

        public int ConsumedTotalHours { get; set; }
        public DateTime DueDate { get; set; }

        // Navigation Properties
        public virtual Client Client { get; set; }
        public virtual Employee Employee { get; set; }
    }

}
