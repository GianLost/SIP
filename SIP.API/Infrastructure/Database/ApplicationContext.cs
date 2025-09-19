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

            entity.Property(u => u.Masp)
                .HasColumnType("int")
                .IsRequired();

            entity.Property(u => u.Name)
                .HasColumnType("varchar(150)")
                .IsRequired();

            entity.Property(u => u.Login)
                .HasColumnType("varchar(50)")
                .IsRequired();

            entity.Property(u => u.Email)
                .HasColumnType("varchar(200)")
                .IsRequired();

            entity.Property(u => u.PasswordHash)
                .HasColumnType("varchar(255)")
                .IsRequired();

            entity.Property(u => u.Role)
                .HasColumnType("varchar(50)")
                .IsRequired();

            entity.Property(u => u.IsActive)
                .HasColumnType("tinyint(1)")
                .IsRequired();

            entity.Property(u => u.CreatedAt)
                .HasColumnType("datetime(6)")
                .IsRequired();

            entity.Property(u => u.LastLoginAt)
                .HasColumnType("datetime(6)");

            entity.Property(u => u.UpdatedAt)
                .HasColumnType("datetime(6)");

            entity.Property(u => u.SectorId)
                .HasColumnType("char(36)")
                .IsRequired();

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

            entity.HasOne(u => u.Sector)
                .WithMany(s => s.Users)
                .HasForeignKey(u => u.SectorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(u => u.Role)
                .HasConversion<string>()
                .IsRequired();
        });

        // Sector entity configuration
        modelBuilder.Entity<Sector>(entity =>
        {
            entity.ToTable("tbl_sectors");

            entity.Property(e => e.Name)
                .HasColumnType("varchar(150)")
                .IsRequired();

            entity.Property(e => e.Acronym)
                .HasColumnType("varchar(20)")
                .IsRequired();

            entity.Property(e => e.Phone)
                .HasColumnType("varchar(20)")
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnType("datetime(6)")
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime(6)");

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

            // Relacionamento com usuários
            entity.HasMany(e => e.Users)
                .WithOne(u => u.Sector)
                .HasForeignKey(u => u.SectorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Protocol entity configuration
        modelBuilder.Entity<Protocol>(entity =>
        {

            entity.ToTable("tbl_protocols");

            entity.Property(p => p.Number)
            .HasColumnType("int")
            .IsRequired();

            entity.Property(p => p.Subject)
            .HasColumnType("varchar(200)")
            .IsRequired();

            entity.Property(p => p.Description)
            .HasColumnType("mediumtext")
            .IsRequired();

            entity.Property(p => p.Status)
           .HasConversion<string>()
           .IsRequired();

            entity.Property(p => p.IsArchived)
           .HasColumnType("tinyint(1)")
           .IsRequired();

            entity.Property(p => p.CreatedAt)
            .HasColumnType("datetime(6)")
            .IsRequired();

            entity.Property(p => p.UpdatedAt)
            .HasColumnType("datetime(6)");

            // Relacionamento com o usuário que criou o protocolo
            entity.HasOne(p => p.CreatedBy)
                  .WithMany(u => u.Protocols) // Assumindo que User tem uma ICollection<Protocol> Protocols
                  .HasForeignKey(p => p.CreatedById)
                  .OnDelete(DeleteBehavior.Restrict); // Impede a exclusão de um usuário se ele tiver protocolos

            // Relacionamento com o setor de origem
            entity.HasOne(p => p.OriginSector)
                  .WithMany() // Sem navegação de volta (um setor não precisa de uma lista de "protocolos originados")
                  .HasForeignKey(p => p.OriginSectorId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Relacionamento com o setor de destino
            entity.HasOne(p => p.DestinationSector)
                  .WithMany() // Sem navegação de volta
                  .HasForeignKey(p => p.DestinationSectorId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Relacionamento com o usuário de destino
            entity.HasOne(p => p.DestinationUser)
                  .WithMany() // Sem navegação de volta
                  .HasForeignKey(p => p.DestinationUserId)
                  .OnDelete(DeleteBehavior.Restrict); // Impede a exclusão do usuário de destino
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