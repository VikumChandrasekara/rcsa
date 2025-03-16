using System.ComponentModel.DataAnnotations;

namespace rcsa.Models
{
    public class AssessmentAnswer
    {

        [Key]
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public int NAflag { get; set; }
        
        public string AnswerText { get; set; }
        public string ServiceCenterAnswer { get; set; } // The inputted marks
        public int ServiceCenterId { get; set; }
        public int ServiceCenterBranch { get; set; }
        public int ServiceCenterRegion { get; set; }

        public int BranchId { get; set; }
        public int RegionId { get; set; }
        
        public int ServiceCenterflag { get; set; }
        public string BranchAnswer {  get; set; }

        public string RegionAnswer { get; set; }
        public int branchflag { get; set; }
        public int Regionflag { get; set; }
        public double Marks { get; set; }
        public double TotalMarks { get; set; }
        public double TotalScore { get; set; }
        public double BrRTotalScore { get; set; }
        public double ReRTotalScore { get; set; }
        public double BrRTotalMarks { get; set; }
        public double ReRTotalMarks { get; set; }
        public string MainHeading { get; set; }
        public string SubHeading { get; set; }
        public string Scomment { get; set; }
        public string Bcomment { get; set; }
        public string Rcomment { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
        public string headingTitel { get; set; }
        public string subheadingTitel { get; set; }

        public DateTime AnswerDate { get; set; }
        public string AnswerUser { get; set; }
        public string Astype { get; set; }
        





    }
}
