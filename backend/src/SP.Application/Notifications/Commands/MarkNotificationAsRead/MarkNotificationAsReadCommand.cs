// SP.Application/Notifications/Commands/MarkNotificationAsRead/MarkNotificationAsReadCommand.cs

using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Notifications.Repositories;

namespace SP.Application.Notifications.Commands.MarkNotificationAsRead;

public sealed record MarkNotificationAsReadCommand(Guid NotificationId, Guid UserId) : ICommand;

public sealed class MarkNotificationAsReadCommandHandler : ICommandHandler<MarkNotificationAsReadCommand>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MarkNotificationAsReadCommandHandler(
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(MarkNotificationAsReadCommand command, CancellationToken cancellationToken = default)
    {
        var notification = await _notificationRepository.GetByIdAsync(command.NotificationId, cancellationToken);
        if (notification is null)
            return Result.Failure(new Error("Notification.NotFound", "Notification not found."));

        if (notification.UserId != command.UserId)
            return Result.Failure(new Error("Notification.Unauthorized", "You cannot mark this notification as read."));

        notification.MarkAsRead();
        _notificationRepository.Update(notification);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
