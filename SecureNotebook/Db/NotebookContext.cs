using Microsoft.EntityFrameworkCore;

using SecureNotebook.Db.Entities;

namespace SecureNotebook.Db
{
    public class NotebookContext : DbContext
    {
        public DbSet<File> Files { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserData> UserDatas { get; set; }

        public NotebookContext()
        {
            Database.EnsureCreated();
        }

        public NotebookContext(DbContextOptions<NotebookContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        /*
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=notebookdb;Trusted_Connection=True;");
        }
        */
    }
}
