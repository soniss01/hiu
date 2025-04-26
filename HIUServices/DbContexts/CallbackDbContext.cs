using HIUServices.Entities.Callbacks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HIUServices.DbContexts
{
    public class CallbackDbContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        public CallbackDbContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = Configuration.GetConnectionString("ConnStr");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("The connection string 'ConnStr' is not defined.");
            }

            optionsBuilder.UseNpgsql(connectionString);
        }

        public DbSet<PatientFindResponse> PatientFindResponses { get; set; }
        public DbSet<ConsentRequestsResponse> ConsentRequestsResponses { get; set; }
    }
}
