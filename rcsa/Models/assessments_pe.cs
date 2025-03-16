using System.ComponentModel.DataAnnotations;

namespace rcsa.Models
{
    public class assessments_pe
    {

        [Key]
        public int Id { get; set; }
        public int QuestionId { get; set; }  // Primary Key

        [Required]
        [StringLength(500)]  // Limiting question length
        public string QuestionText { get; set; }

        public double Marks { get; set; }
        public string MainHeading { get; set; }
        public string SubHeading { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public string headingTitel { get; set; }
        public string subheadingTitel { get; set; }
        public string Astype { get; set; }
        
        public int NAflag { get; set; }
        public string SubmitBy { get; set; }



    }
}
