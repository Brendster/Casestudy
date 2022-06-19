using Microsoft.EntityFrameworkCore;
using Casestudy.DAL.DomainClasses;
using CaseStudyAPI.DAL.DomainClasses;

namespace Casestudy.DAL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public virtual DbSet<Product>? Product { get; set; }
        public virtual DbSet<Brand>? Brand { get; set; }
        public virtual DbSet<Customer>? Customers { get; set; }
        public virtual DbSet<Order>? Orders { get; set; }
        public virtual DbSet<OrderLineItem>? OrderLineItems { get; set; }
        public virtual DbSet<Branch>? Branches { get; set; }

    }
}