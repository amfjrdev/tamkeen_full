using SP.Domain.Abstractions;

namespace SP.Domain.Chat.Errors;

public static class ChatErrors
{
    public static readonly Error ConversationNotFound = new("Chat.ConversationNotFound", "Conversation not found.");
    public static readonly Error Unauthorized = new("Chat.Unauthorized", "You are not a participant of this conversation.");
    public static readonly Error MessageNotFound = new("Chat.MessageNotFound", "Message not found.");
    public static readonly Error ConversationLocked = new("Chat.ConversationLocked", "Conversation is locked.");
    public static readonly Error MessageSenderMismatch = new("Chat.MessageSenderMismatch", "Only the message receiver can mark it as read.");
    public static readonly Error AlreadyUnlocked = new("Chat.AlreadyUnlocked", "Conversation is already unlocked.");
    public static readonly Error InsufficientBalance = new("Chat.InsufficientBalance", "Insufficient connects balance.");
}
