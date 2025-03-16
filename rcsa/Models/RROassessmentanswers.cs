using System.ComponentModel.DataAnnotations;

namespace rcsa.Models
{
    public class RROassessmentanswers
    {


        [Key]
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string AnswerText { get; set; }
        public int Marks { get; set; }
        public int TotalMarks { get; set; }
        public int TotalScore { get; set; }

        public string RRoAnswer { get; set; }

     
        public string subheading { get; set; }
        public string MainHeading { get; set; }
        public string RRoComment { get; set; }
      
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public int ServiceCenterId { get; set; }

        public int BranchId { get; set; }
        public int RegionId { get; set; }
        public DateTime DateofVist { get; set; }
        public int Perd { get; set; }
        public TimeSpan Intime { get; set; }
        public TimeSpan OutTime { get; set; }
        public string headingTitel { get; set; }
        public string subheadingTitel { get; set; }

        
        public int SaveAnswer { get; set; }
        public DateTime ASubimteddate { get; set; }

        public int Suser { get; set; }



    }
}
