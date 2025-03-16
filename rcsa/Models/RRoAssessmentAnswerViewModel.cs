namespace rcsa.Models
{
    public class RRoAssessmentAnswerViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public int Marks { get; set; }
        public string RRoAnswer { get; set; }
        public int ServiceCenterId { get; set; }
        public int BranchId { get; set; }

        public string MainHeading { get; set; }
        public string subheading { get; set; }
        public DateTime DateofVist { get; set; }
        public DateTime fromdate { get; set; }
        public TimeOnly Intime { get; set; }
        public TimeOnly OutTime { get; set; }
        public string RRoComment { get; set; }
      
        public string headingTitel { get; set; }
        public string subheadingTitel { get; set; }
        public int SaveAnswer { get; set; }

     


    }
}
