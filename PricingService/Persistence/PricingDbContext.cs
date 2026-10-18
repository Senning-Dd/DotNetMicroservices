using Microsoft.EntityFrameworkCore;

namespace PricingService.Persistence;

public sealed class PricingDbContext : DbContext
{
    public PricingDbContext(DbContextOptions<PricingDbContext> options) : base(options)
    {
    }

    internal DbSet<TariffRecord> Tariffs => Set<TariffRecord>();
    internal DbSet<BasePremiumRuleRecord> BasePremiumRules => Set<BasePremiumRuleRecord>();
    internal DbSet<DiscountMarkupRuleRecord> DiscountMarkupRules => Set<DiscountMarkupRuleRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TariffRecord>(entity =>
        {
            entity.ToTable("Tariffs");
            entity.HasKey(tariff => tariff.Code);
            entity.Property(tariff => tariff.Code).HasMaxLength(64);
        });

        modelBuilder.Entity<BasePremiumRuleRecord>(entity =>
        {
            entity.ToTable("BasePremiumRules");
            entity.HasKey(rule => rule.Id);
            entity.Property(rule => rule.TariffCode).HasMaxLength(64);
            entity.Property(rule => rule.CoverCode).HasMaxLength(64).IsRequired();
            entity.Property(rule => rule.Condition).HasColumnType("text");
            entity.Property(rule => rule.Expression).HasColumnType("text").IsRequired();
            entity.HasIndex(rule => new { rule.TariffCode, rule.CoverCode, rule.Priority }).IsUnique();
            entity.HasOne(rule => rule.Tariff)
                .WithMany(tariff => tariff.BasePremiumRules)
                .HasForeignKey(rule => rule.TariffCode)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DiscountMarkupRuleRecord>(entity =>
        {
            entity.ToTable("DiscountMarkupRules");
            entity.HasKey(rule => rule.Id);
            entity.Property(rule => rule.TariffCode).HasMaxLength(64);
            entity.Property(rule => rule.Condition).HasColumnType("text").IsRequired();
            entity.Property(rule => rule.Multiplier).HasPrecision(18, 6);
            entity.HasIndex(rule => new { rule.TariffCode, rule.Priority }).IsUnique();
            entity.HasOne(rule => rule.Tariff)
                .WithMany(tariff => tariff.DiscountMarkupRules)
                .HasForeignKey(rule => rule.TariffCode)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
