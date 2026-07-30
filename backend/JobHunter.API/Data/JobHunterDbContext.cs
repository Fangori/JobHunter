using Microsoft.EntityFrameworkCore;

namespace JobHunter.API.Data;

public class JobHunterDbContext : DbContext
{
    public JobHunterDbContext(DbContextOptions<JobHunterDbContext> options) : base(options)
    {
    }

    // DbSet<...> duoc them dan theo tung Phase khi entity tuong ung
    // duoc tao trong Models/, khop dung database/JobHunter_CreateTables.sql
}
