using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace rcsa.Models
{
    public class UserServiceCenter
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int ServiceCenterId { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public virtual RROUsers RROUser { get; set; }

        [ForeignKey("ServiceCenterId")]
        public virtual Branch ServiceCenter { get; set; }
    }
}
