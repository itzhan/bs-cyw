using Microsoft.EntityFrameworkCore;
using SmartStreetlight.Api.Models.Entities;

namespace SmartStreetlight.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Streetlight> Streetlights => Set<Streetlight>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<Cabinet> Cabinets => Set<Cabinet>();
    public DbSet<Alarm> Alarms => Set<Alarm>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<ControlLog> ControlLogs => Set<ControlLog>();
    public DbSet<ControlStrategy> ControlStrategies => Set<ControlStrategy>();
    public DbSet<EnergyRecord> EnergyRecords => Set<EnergyRecord>();
    public DbSet<RepairReport> RepairReports => Set<RepairReport>();
    public DbSet<Announcement> Announcements => Set<Announcement>();
    public DbSet<MqttMessage> MqttMessages => Set<MqttMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User-Role many-to-many via UserRole join table
        modelBuilder.Entity<User>()
            .HasMany(u => u.Roles)
            .WithMany(r => r.Users)
            .UsingEntity<UserRole>(
                j => j.HasOne<Role>().WithMany().HasForeignKey(ur => ur.RoleId),
                j => j.HasOne<User>().WithMany().HasForeignKey(ur => ur.UserId),
                j => j.ToTable("user_role"));

        // Streetlight — don't auto-include navigations to avoid circular refs
        modelBuilder.Entity<Streetlight>()
            .Navigation(s => s.Area).AutoInclude(false);
        modelBuilder.Entity<Streetlight>()
            .Navigation(s => s.Cabinet).AutoInclude(false);

        // Cabinet auto-include Area
        modelBuilder.Entity<Cabinet>()
            .Navigation(c => c.Area).AutoInclude();

        // Alarm auto-include Streetlight and Area
        modelBuilder.Entity<Alarm>()
            .Navigation(a => a.Streetlight).AutoInclude();
        modelBuilder.Entity<Alarm>()
            .Navigation(a => a.Area).AutoInclude();

        // WorkOrder auto-include Streetlight and Area
        modelBuilder.Entity<WorkOrder>()
            .Navigation(w => w.Streetlight).AutoInclude();
        modelBuilder.Entity<WorkOrder>()
            .Navigation(w => w.Area).AutoInclude();

        // ControlStrategy auto-include Area
        modelBuilder.Entity<ControlStrategy>()
            .Navigation(cs => cs.Area).AutoInclude();

        // User auto-include Roles
        modelBuilder.Entity<User>()
            .Navigation(u => u.Roles).AutoInclude();

        // Unique indexes
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<Role>().HasIndex(r => r.Name).IsUnique();
        modelBuilder.Entity<Area>().HasIndex(a => a.Code).IsUnique();
        modelBuilder.Entity<Streetlight>().HasIndex(s => s.Code).IsUnique();
        modelBuilder.Entity<Cabinet>().HasIndex(c => c.Code).IsUnique();
        modelBuilder.Entity<Alarm>().HasIndex(a => a.AlarmCode).IsUnique();
        modelBuilder.Entity<WorkOrder>().HasIndex(w => w.OrderNo).IsUnique();
        modelBuilder.Entity<RepairReport>().HasIndex(r => r.ReportNo).IsUnique();
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);
        foreach (var entry in entries)
        {
            var prop = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
            if (prop != null) prop.CurrentValue = DateTime.Now;
        }
    }
}
