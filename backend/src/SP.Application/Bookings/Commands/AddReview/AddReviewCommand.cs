// SP.Application/Bookings/Commands/AddReview/AddReviewCommand.cs

using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Bookings.Errors;
using SP.Domain.Bookings.Repositories;

namespace SP.Application.Bookings.Commands.AddReview;

public sealed record AddReviewCommand(
    Guid BookingId,
    Guid ClientId,
    int Rating,
    string Comment) : ICommand;

public sealed class AddReviewCommandHandler : ICommandHandler<AddReviewCommand>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddReviewCommandHandler(IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
    {
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(AddReviewCommand command, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(command.BookingId, cancellationToken);
        if (booking is null)
            return Result.Failure(BookingErrors.NotFound);

        if (booking.ClientId != command.ClientId)
            return Result.Failure(BookingErrors.UnauthorizedAction);

        var result = booking.AddReview(command.ClientId, command.Rating, command.Comment);
        if (result.IsFailure)
            return result;

        _bookingRepository.Update(booking);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
