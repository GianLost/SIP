using SIP.API.Domain.Entities.Attachments;
using SIP.API.Domain.Entities.Movements;
using SIP.API.Domain.Entities.Protocols;
using SIP.API.Domain.Entities.Sectors;
using SIP.API.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace SIP.API.Infrastructure.Database;

/// <summary>
/// Represents the Entity Framework Core database context for the SIP.API application.
/// Manages entity sets and configures model relationships and mappings.
/// </summary>
public class ApplicationContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Sector> Sectors => Set<Sector>();
    public DbSet<Protocol> Protocols => Set<Protocol>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<Movement> Movements => Set<Movement>();

    /// <summary>
    /// Configures the entity mappings and relationships for the database context.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User entity configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasOne(u => u.Sector)
                .WithMany(s => s.Users)
                .HasForeignKey(u => u.SectorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(u => u.Role)
                .HasConversion<string>()
                .IsRequired();

            entity.ToTable("tbl_users");
        });

        // Sector entity configuration
        modelBuilder.Entity<Sector>(entity =>
        {
            entity.ToTable("tbl_sectors");
        });

        // Protocol entity configuration
        modelBuilder.Entity<Protocol>(entity =>
        {

            entity.HasOne(p => p.CreatedBy)
                 .WithMany(u => u.Protocols)
                 .HasForeignKey(p => p.CreatedById)
                 .OnDelete(DeleteBehavior.Restrict);

            entity.Property(u => u.Status)
                .HasConversion<string>()
                .IsRequired();

            entity.ToTable("tbl_protocols");
        });

        // Attachment entity configuration
        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.ToTable("tbl_attachments");
        });

        // Movement entity configuration
        modelBuilder.Entity<Movement>(entity =>
        {
            entity.HasOne(m => m.FromSector)
                .WithMany()
                .HasForeignKey(m => m.FromSectorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.ToSector)
                .WithMany()
                .HasForeignKey(m => m.ToSectorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.ToTable("tbl_movements");
        });
    }
}