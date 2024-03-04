using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetCore.Identity.LnAuth.Api.Domain.Entities;

namespace NetCore.Identity.LnAuth.Api.Database.Configuration;

public class LinkingKeyConfiguration: IEntityTypeConfiguration<LightningAuthLinkingKey>
{
    public void Configure(EntityTypeBuilder<LightningAuthLinkingKey> builder) 
    {
        builder.ToTable(nameof(LightningAuthLinkingKey), "lightning");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.HasIndex(x => new { x.Type, x.K1 });
    }
}