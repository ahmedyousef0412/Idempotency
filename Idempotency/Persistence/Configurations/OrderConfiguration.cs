namespace Idempotency.Persistence.Configurations;

internal class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.ProductName)
            .IsRequired();
        builder.Property(o => o.CreatedAt).IsRequired();
    }
}