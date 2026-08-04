using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nadiano.Core.Profiles;

namespace Nadiano.Web.Infrastructure.Persistence;

public class ProfilePreferencesConfiguration : IEntityTypeConfiguration<ProfilePreferences>
{
    public void Configure(EntityTypeBuilder<ProfilePreferences> builder)
    {
        builder.ToTable("ProfilePreferences");
        builder.HasKey(preferences => preferences.ProfileId);
        builder.Property(preferences => preferences.Language).IsRequired().HasMaxLength(10);
        builder.Property(preferences => preferences.NoteNameSystem).IsRequired().HasMaxLength(20);
        builder.Property(preferences => preferences.PreferredMidiDeviceId).HasMaxLength(200);
        builder.Property(preferences => preferences.PreferredMidiDeviceName).HasMaxLength(200);

        builder.HasOne<LearnerProfile>()
            .WithOne()
            .HasForeignKey<ProfilePreferences>(preferences => preferences.ProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
