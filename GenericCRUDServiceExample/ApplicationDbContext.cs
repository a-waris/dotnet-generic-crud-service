using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GenericCRUDServiceExample;

public class ApplicationDbContext : DbContext, IContext
{
    private readonly IConfiguration? _configuration;

    public static readonly IModel StaticModel = BuildStaticModel();

    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
    }
    
    

    public DbSet<Hero> Heroes { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseInMemoryDatabase("ApplicationDb");
        }
    }

    private static IModel BuildStaticModel()
    {
        using var dbContext = new ApplicationDbContext();
        return dbContext.Model;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HeroConfiguration).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}