using Microsoft.EntityFrameworkCore;
using PSFR_Repository.Entities;
using PSFR_Repository.Entities.AccessRules;
namespace PSFR_Repository.Context;

public partial class DMSCustomContext : DbContext
{
    public DMSCustomContext()
    {
    }

    public DMSCustomContext(DbContextOptions<DMSCustomContext> options)
        : base(options)
    {
    }
    public virtual DbSet<LookupItems> LookupItems { get; set; }
    public virtual DbSet<ExceptionLog> ExceptionLogs { get; set; }
    public virtual DbSet<AccessRule> AccessRules { get; set; }
    public virtual DbSet<AccessRuleCondition> AccessRuleConditions { get; set; }
    public virtual DbSet<AccessRuleTarget> AccessRuleTargets { get; set; }




    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExceptionLog>(entity =>
        {
            entity.ToTable("ExceptionLog");

            entity.Property(e => e.Level).HasMaxLength(50);
            entity.Property(e => e.MachineName).HasMaxLength(150);
        });


        modelBuilder.Entity<Lookup>(entity =>
        {
            entity.ToTable("Lookup");

            entity.HasIndex(e => e.CreatedByUserId, "IX_Lookup_CreatedByUserId");

            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.Lookups)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Lookup_User");
        });

        modelBuilder.Entity<LookupItems>(entity =>
        {
            entity.HasIndex(e => e.CreatedByUserId, "IX_LookupItems_CreatedByUserId");

            entity.HasIndex(e => e.ParentId, "IX_LookupItems_ParentId");

            entity.HasIndex(e => e.Code, "IX_UniqueCode")
                .IsUnique()
                .HasFilter("([Code] IS NOT NULL)");

            entity.Property(e => e.Code).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.NameAr).HasMaxLength(150);
            entity.Property(e => e.NameFr).HasMaxLength(150);

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.LookupItems)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LookupItems_User");

            entity.HasOne(d => d.Lookup).WithMany(p => p.LookupItems)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK_LookupItems_Lookup");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Firstname).HasMaxLength(150);
            entity.Property(e => e.FirstnameAr).HasMaxLength(150);
            entity.Property(e => e.FirstnameFr).HasMaxLength(150);
            entity.Property(e => e.Lastname).HasMaxLength(150);
            entity.Property(e => e.LastnameAr).HasMaxLength(150);
            entity.Property(e => e.LastnameFr).HasMaxLength(150);
        });

        modelBuilder.Entity<AccessRule>(entity =>
        {
            entity.ToTable("AccessRule");

            entity.HasIndex(e => new { e.ContentTypeId }, "IX_AccessRule_ContentTypeId");

            entity.Property(e => e.ContentTypeText).HasMaxLength(250);
            entity.Property(e => e.CreatedAtUtc).HasColumnType("datetime2");

            entity.HasOne(e => e.CreatedByUser).WithMany().HasForeignKey(e => e.CreatedByUserId).OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("FK_AccessRule_User");

            entity.HasMany(e => e.Conditions).WithOne(e => e.AccessRule).HasForeignKey(e => e.AccessRuleId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_AccessRuleCondition_AccessRule");
            entity.HasMany(e => e.Targets).WithOne(e => e.AccessRule).HasForeignKey(e => e.AccessRuleId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_AccessRuleTarget_AccessRule");
        });

        modelBuilder.Entity<AccessRuleCondition>(entity =>
        {
            entity.ToTable("AccessRuleCondition");

            entity.HasIndex(e => e.AccessRuleId, "IX_AccessRuleCondition_AccessRuleId");

            entity.Property(e => e.FieldKey).HasMaxLength(200);
            entity.Property(e => e.Op).HasMaxLength(50);
            entity.Property(e => e.Value).HasMaxLength(1000);
        });

        modelBuilder.Entity<AccessRuleTarget>(entity =>
        {
            entity.ToTable("AccessRuleTarget");

            entity.HasIndex(e => e.AccessRuleId, "IX_AccessRuleTarget_AccessRuleId");
            entity.HasIndex(e => new { e.Type, e.ExternalId }, "IX_AccessRuleTarget_Type_ExternalId");

            entity.Property(e => e.Type).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(250);
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
