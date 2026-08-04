using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Nadiano.Core.Profiles;

namespace Nadiano.Web.Infrastructure.Persistence;

public class LearnerProfileConfiguration : IEntityTypeConfiguration<LearnerProfile>
{
    public void Configure(EntityTypeBuilder<LearnerProfile> builder)
    {
        builder.ToTable("LearnerProfiles");
        builder.HasKey(profile => profile.Id);
        builder.Property(profile => profile.Name)
            .IsRequired()
            .HasMaxLength(80);
        builder.Property(profile => profile.CreatedAtUtc)
            .IsRequired();
    }
}