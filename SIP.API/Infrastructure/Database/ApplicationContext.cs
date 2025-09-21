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
            entity.ToTable("tbl_users");

            //Conversão do enum para string
            entity.Property(u => u.Role)
                .HasConversion<string>()
                .IsRequired();

            // índices únicos
            entity.HasIndex(u => u.Masp)
                .IsUnique()
                .HasDatabaseName("uc_User_Masp");

            entity.HasIndex(u => u.Name)
                .IsUnique()
                .HasDatabaseName("uc_User_Name");

            entity.HasIndex(u => u.Login)
                .IsUnique()
                .HasDatabaseName("uc_User_Login");

            entity.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("uc_User_Email");

            entity.HasIndex(u => u.SectorId)
                .HasDatabaseName("idx_User_SectorId");

            // Relacionamento com protocolos
            entity.HasMany(u => u.ProtocolsCreated)
                .WithOne(p => p.CreatedBy)
                .HasForeignKey(p => p.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(u => u.ProtocolsReceived)
                .WithOne(p => p.DestinationUser)
                .HasForeignKey(p => p.DestinationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relacionamento com usuários (Para gestão)
            entity.HasOne(u => u.CreatedBy)
                .WithMany()
                .HasForeignKey(u => u.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(u => u.UpdatedBy)
                .WithMany()
                .HasForeignKey(u => u.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(u => u.Sector)
                .WithMany(s => s.Users)
                .HasForeignKey(u => u.SectorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Sector entity configuration
        modelBuilder.Entity<Sector>(entity =>
        {
            entity.ToTable("tbl_sectors");

            // Relacionamento com usuários (para gestão)
            entity.HasOne(s => s.CreatedBy)
                .WithMany()
                .HasForeignKey(s => s.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.UpdatedBy)
                .WithMany()
                .HasForeignKey(s => s.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Users)
                .WithOne(u => u.Sector)
                .HasForeignKey(u => u.SectorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices únicos
            entity.HasIndex(e => e.Name)
                .IsUnique()
                .HasDatabaseName("uc_Sector_Name");

            entity.HasIndex(e => e.Acronym)
                .IsUnique()
                .HasDatabaseName("uc_Sector_Acronym");

            entity.HasIndex(e => e.Phone)
                .IsUnique()
                .HasDatabaseName("uc_Sector_Phone");
        });

        // Protocol entity configuration
        modelBuilder.Entity<Protocol>(entity =>
        {
            entity.ToTable("tbl_protocols");

            // Índices
            entity.HasIndex(p => p.Number)
                .IsUnique()
                .HasDatabaseName("uc_Protocol_Number");

            entity.HasIndex(p => p.CreatedById)
                .HasDatabaseName("idx_Protocol_CreatedById");

            entity.HasIndex(p => p.DestinationUserId)
                .HasDatabaseName("idx_Protocol_DestinationUserId");

            entity.HasIndex(p => p.OriginSectorId)
                .HasDatabaseName("idx_Protocol_OriginSectorId");

            entity.HasIndex(p => p.DestinationSectorId)
                .HasDatabaseName("idx_Protocol_DestinationSectorId");

            // Relacionamentos com usuários
            entity.HasOne(p => p.CreatedBy)
                .WithMany(u => u.ProtocolsCreated)
                .HasForeignKey(p => p.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.DestinationUser)
                .WithMany(u => u.ProtocolsReceived)
                .HasForeignKey(p => p.DestinationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.UpdatedBy)
                .WithMany()
                .HasForeignKey(p => p.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.OriginSector)
                  .WithMany()
                  .HasForeignKey(p => p.OriginSectorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.DestinationSector)
                  .WithMany()
                  .HasForeignKey(p => p.DestinationSectorId)
                  .OnDelete(DeleteBehavior.Restrict);
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