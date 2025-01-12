using Microsoft.EntityFrameworkCore;
using test.Models;

namespace test.Data
{
    public class APIContext : DbContext
    {
        public DbSet<Request> requests { get; set; }
        public APIContext(DbContextOptions<APIContext> options) : base(options)
        {
        }
    }
}
