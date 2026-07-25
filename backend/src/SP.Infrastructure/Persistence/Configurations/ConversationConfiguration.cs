using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SP.Domain.Chat;

namespace SP.Infrastructure.Persistence.Configurations;

internal sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Participant1Id).IsRequired();
        builder.Property(c => c.Participant2Id).IsRequired();
        builder.Property(c => c.IsLocked).IsRequired();
        builder.Property(c => c.UnlockCost).IsRequired();
        builder.Property(c => c.LastMessageText).HasMaxLength(4000);
        builder.Property(c => c.LastMessageSenderId);
        builder.Property(c => c.LastMessageSentAt);
        builder.Property(c => c.CreatedAt).IsRequired();

        builder.HasIndex(c => c.Participant1Id);
        builder.HasIndex(c => c.Participant2Id);
        builder.HasIndex(c => c.LastMessageSentAt);
        // NOTE: uniqueness of (p1,p2) pairs is enforced in application layer (GetByParticipantsAsync)
        // because (A,B) and (B,A) are the same conversation — a DB unique index cannot express this.
    }
}
