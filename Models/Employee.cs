using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Client_Invoice_System.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required]
        public string EmployeeName { get; set; }

        public string Designation { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; }

        // Navigation Property
        public virtual ICollection<Resource> Resources { get; set; }
    }

}
