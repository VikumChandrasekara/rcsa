namespace rcsa.Models
{
    public class AssessmentAnswerViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public double Marks { get; set; }
        public string ServiceCenterAnswer { get; set; } 
        public int ServiceCenterId { get; set; } 
        public int ServiceCenterflag { get; set; }
        public string BranchAnswer { get; set; }
        public int BranchId { get; set; }

        public string RegionAnswer { get; set; }
        public string MainHeading { get; set; }
        public string SubHeading { get; set; }
        public string Comment { get; set; }
        public string BComment { get; set; }
        public string Rcomment { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
       
        public string headingTitel { get; set; }
        public string subheadingTitel { get; set; }
        public string Astype { get; set; }

        public int NAflag { get; set; }

    }
}
