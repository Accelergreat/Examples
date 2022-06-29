using Accelergreat.Idp.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accelergreat.Idp.Database.Configurations
{
    public class BlogConfiguration : IEntityTypeConfiguration<Blog>
    {
        public void Configure(EntityTypeBuilder<Blog> builder)
        {
            builder.HasKey(b => b.Id);
            builder.Property(b => b.Id).ValueGeneratedOnAdd();
            builder
                .HasMany(b => b.BlogEntries)
                .WithOne(be => be.ParentBlog)
                .HasForeignKey(be => be.ParentBlogId)
                .IsRequired();
        }
    }
}
