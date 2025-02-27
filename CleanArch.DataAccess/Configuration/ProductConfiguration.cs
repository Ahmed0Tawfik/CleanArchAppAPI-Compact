using CleanArch.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanArch.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // Table name
        builder.ToTable("Products");

        // Primary Key
        builder.HasKey(p => p.Id);

        // Properties Configuration
        builder.Property(p => p.Name)
            .IsRequired() // Name is required
            .HasMaxLength(100); // Limit to 100 characters

        builder.Property(p => p.Description)
            .HasMaxLength(256); // Optional, limit to 500 characters

        builder.Property(p => p.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)"); // Use decimal for monetary values

        builder.Property(p => p.ImageUrl)
            .IsRequired()
            .HasMaxLength(256); // URLs can be long

        builder.Property(p => p.InStock)
            .IsRequired()
            .HasDefaultValue(true); // Default to "out of stock"

        // Indexes (for query optimization)
        builder.HasIndex(p => p.Name); // Faster lookup by product name
        builder.HasIndex(p => p.InStock); // Faster filtering by stock status
    }
}