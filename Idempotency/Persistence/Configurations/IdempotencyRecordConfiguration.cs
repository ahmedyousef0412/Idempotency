
namespace Idempotency.Persistence.Configurations;

public class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasIndex(x => x.IdempotencyKey).IsUnique();

        builder.Property(x => x.IdempotencyKey)
              .IsRequired()
              .HasMaxLength(100);

        builder.Property(x => x.ResponseBody)
               .HasColumnType("nvarchar(max)");

           
    }
}
