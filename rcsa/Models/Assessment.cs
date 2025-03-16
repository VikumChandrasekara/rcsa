using System.ComponentModel.DataAnnotations;

namespace rcsa.Models
{
    public class Assessment
    {
        [Key]
        public int Id { get; set; }  // Primary Key

        [Required]
        [StringLength(500)]  // Limiting question length
        public string Question { get; set; }

        [Required]
        [StringLength(100)]
        public string MainHeading { get; set; }
        public string SubHeading { get; set; }
        public string headingTitel { get; set; }
        public string subheadingTitel { get; set; }

        public double Marks { get; set; }  
        public int NAflag { get; set; }
       // public Heading MainHeadings { get; set; } // Navigation Property
     //   public SubHeading SubHeading { get; set; } // Navigation Property for SubHeading
    }
}
