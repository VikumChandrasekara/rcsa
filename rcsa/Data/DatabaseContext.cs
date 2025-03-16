namespace rcsa.Data
{
	using System.Collections.Generic;
	using Microsoft.EntityFrameworkCore;
	using rcsa.Models;

	public class DatabaseContext : DbContext
	{
		public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

		public DbSet<User> Users { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<RLog> RLogs { get; set; }
        public DbSet<Heading> Headings { get; set; }
        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<SubHeading> SubHeadings { get; set; }

        public DbSet<assessments_pe> AssessmentsPe { get; set; }
      
        public DbSet<AssessmentAnswer> AssessmentAnswers { get; set; }
        public DbSet<AssessmentAnswersCopy> AssessmentAnswersCopys { get; set; }
        public DbSet<RROassessmentanswers> RROassessmentanswers { get; set; }
        public DbSet<RROUsers> RROUsers { get; set; }
        public DbSet<UserServiceCenter> UserServiceCenters { get; set; }

        public DbSet<Rangs> Rangs { get; set; }



    }

}
