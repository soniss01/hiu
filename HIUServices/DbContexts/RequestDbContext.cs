using HIUServices.Entities.Requests.Consents;
using HIUServices.Entities.Requests.Masters;
using HIUServices.Entities.Requests.Patients;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HIUServices.DbContexts
{
    public class RequestDbContext : DbContext
    {
        public RequestDbContext(DbContextOptions<RequestDbContext> options) : base(options) { }

        //protected override void OnConfiguring(DbContextOptionsBuilder options)
        //{
        //    var connectionString = Configuration.GetConnectionString("ConnStr");
        //    if (string.IsNullOrEmpty(connectionString))
        //    {
        //        throw new InvalidOperationException("The connection string 'ConnStr' is not defined.");
        //    }

        //    options.UseNpgsql(connectionString);
        //}

        public DbSet<PatientInfo> PatientInfos { get; set; }
        public DbSet<ConsentRequestInfo> ConsentRequestInfos { get; set; }
        public DbSet<PurposeOfUse> PurposeOfUses { get; set; }
        public DbSet<HiTypes> HiTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Additional model configuration
        }
    }
}
