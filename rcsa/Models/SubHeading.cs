using System.ComponentModel.DataAnnotations;

namespace rcsa.Models
{
   
   
        public class SubHeading
        {
            public int Id { get; set; }
            public int HeadingId { get; set; }
            public string SubHeadingT { get; set; }

        // Foreign key to link subheadings with a heading
      
        public Heading Heading { get; set; }

    }
    
    
}
