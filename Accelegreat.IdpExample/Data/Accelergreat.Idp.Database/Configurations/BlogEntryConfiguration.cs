using Accelergreat.Idp.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accelergreat.Idp.Database.Configurations;

public class BlogEntryConfiguration : IEntityTypeConfiguration<BlogEntry>
{
    public void Configure(EntityTypeBuilder<BlogEntry> builder)
    {
        builder.HasKey(be => be.Id);
        builder.Property(s => s.Posted).HasDefaultValueSql("now()");
    }
}