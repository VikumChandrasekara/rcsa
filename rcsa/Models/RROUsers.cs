namespace rcsa.Models
{
    public class RROUsers
    {

        public int Id { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public int ServiceNo { get; set; }

        public int RegionId { get; set; }

        public string RRoname { get; set; }

    }
}
