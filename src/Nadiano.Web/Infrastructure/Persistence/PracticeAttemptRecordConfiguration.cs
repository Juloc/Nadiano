using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Nadiano.Core.Practice;

namespace Nadiano.Web.Infrastructure.Persistence;

public class PracticeAttemptRecordConfiguration : IEntityTypeConfiguration<PracticeAttemptRecord>
{
    public void Configure(EntityTypeBuilder<PracticeAttemptRecord> builder)
    {
        builder.ToTable("PracticeAttempts");
        builder.HasKey(attempt => attempt.Id);
        builder.Property(attempt => attempt.ResultJson).IsRequired();
        builder.Property(attempt => attempt.NextActionCode).IsRequired().HasMaxLength(50);

        builder.HasIndex(attempt => attempt.SessionId).IsUnique();
    }
}