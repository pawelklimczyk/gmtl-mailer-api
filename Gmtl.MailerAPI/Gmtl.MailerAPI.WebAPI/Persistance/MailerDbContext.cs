using Gmtl.MailerAPI.WebAPI.Domain;
using Microsoft.EntityFrameworkCore;

namespace Gmtl.MailerAPI.WebAPI.Persistance
{
    public class MailerDbContext : DbContext
    {
        public MailerDbContext(DbContextOptions<MailerDbContext> options)
           : base(options) { }

        public DbSet<MailMessage> Mails { get; set; }
    }
}
