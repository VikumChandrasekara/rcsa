using System.ComponentModel.DataAnnotations;

namespace rcsa.Models
{
    public class Heading
    {
        public int Id { get; set; }
        public string MainHeadings { get; set; }
        public int Orders { get; set; }
        public List<SubHeading> Subheadings { get; set; } = new List<SubHeading>();

    }
}