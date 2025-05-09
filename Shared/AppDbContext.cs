using Microsoft.EntityFrameworkCore;
using Shared.Models;
using Shared.Models.Enums;

namespace Shared;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) 
        : base(options)
    { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>()
            .Property<GenerationStatus>(e => e.Status)
            .HasConversion<int>();
        
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=TASL-00127\SQLEXPRESS;Database=SahroDb;Trusted_Connection=True;TrustServerCertificate=True");
    }
    
    public DbSet<Document> Documents { get; set; }
}