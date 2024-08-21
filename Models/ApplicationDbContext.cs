using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_App_prac1.Models
{

public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }

    }

}
