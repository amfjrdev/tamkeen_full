using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SP.Application.Abstractions.Messaging;
using SP.Application.Chat.Dtos;
using SP.Domain.Abstractions;
using SP.Domain.Bookings;
using SP.Domain.Bookings.Repositories;
using SP.Domain.Chat;
using SP.Domain.Chat.Errors;
using SP.Domain.Chat.Repositories;
using SP.Domain.Services.Repositories;
using SP.Domain.Users;

namespace SP.Application.Chat.Commands.SendMessage;

public sealed record SendMessageCommand(
    Guid UserId,
    Guid ConversationId,
    string Text) : ICommand<MessageResponse>;

public sealed class SendMessageCommandHandler
    : ICommandHandler<SendMessageCommand, MessageResponse>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SendMessageCommandHandler(
        IConversationRepository conversationRepository,
        IUserRepository userRepository,
        IBookingRepository bookingRepository,
        IServiceRepository serviceRepository,
        IUnitOfWork unitOfWork)
    {
        _conversationRepository = conversationRepository;
        _userRepository = userRepository;
        _bookingRepository = bookingRepository;
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<MessageResponse>> HandleAsync(
        SendMessageCommand command,
        CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationRepository.GetByIdAsync(command.ConversationId, cancellationToken);
        if (conversation is null)
        {
            return Result.Failure<MessageResponse>(ChatErrors.ConversationNotFound);
        }

        // Validate user is participant
        if (conversation.Participant1Id != command.UserId && conversation.Participant2Id != command.UserId)
        {
            return Result.Failure<MessageResponse>(ChatErrors.Unauthorized);
        }

        // Validate conversation is not locked (except for the client who initiated it)
        if (conversation.IsLocked && command.UserId != conversation.Participant1Id)
        {
            return Result.Failure<MessageResponse>(ChatErrors.ConversationLocked);
        }

        // Check if the sender is a client and we should automatically create a booking request
        var sender = await _userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (sender != null && sender.Role == UserRole.Client)
        {
            var providerId = conversation.Participant1Id == command.UserId 
                ? conversation.Participant2Id 
                : conversation.Participant1Id;

            // Check if there is already an active (Pending or Accepted) booking between this client and provider
            var clientBookings = await _bookingRepository.GetByClientIdAsync(command.UserId, cancellationToken);
            var hasActiveBooking = clientBookings.Any(b => b.ProviderId == providerId && 
                (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Accepted));

            if (!hasActiveBooking)
            {
                // Get the provider's active services
                var activeServices = await _serviceRepository.GetActiveServicesByProviderIdAsync(providerId, cancellationToken);
                var service = activeServices.FirstOrDefault();
                if (service != null)
                {
                    // Create a booking scheduled for 2 days from now as a placeholder
                    var scheduledDate = DateTime.UtcNow.AddDays(2);
                    var bookingResult = Booking.Create(command.UserId, providerId, service.Id, scheduledDate);
                    if (bookingResult.IsSuccess)
                    {
                        await _bookingRepository.AddAsync(bookingResult.Value, cancellationToken);
                    }
                }
            }
        }

        // Create ChatMessage
        var message = SP.Domain.Chat.ChatMessage.Create(command.ConversationId, command.UserId, command.Text);

        // Add message to repository
        await _conversationRepository.AddMessageAsync(message, cancellationToken);

        // Update conversation last message
        conversation.UpdateLastMessage(message.Text, message.SenderId, message.SentAt);

        // Save
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Map response
        var response = new MessageResponse(
            message.Id,
            message.SenderId,
            message.Text,
            message.SentAt,
            message.IsRead);

        return Result.Success(response);
    }
}
