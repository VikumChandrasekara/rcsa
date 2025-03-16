namespace rcsa.Models
{
    public class Branch
    {
        public int BranchID { get; set; }
        public string Name { get; set; }
        public string Branch_type_code { get; set; }
        public int MAIN_BRANCH_ID {  get; set; }
        public int RegionID { get; set; }
    }

}
