// SP.Application/Bookings/Commands/CancelBooking/CancelBookingCommand.cs

using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Bookings.Errors;
using SP.Domain.Bookings.Repositories;

namespace SP.Application.Bookings.Commands.CancelBooking;

public sealed record CancelBookingCommand(Guid BookingId, Guid ClientId) : ICommand;

public sealed class CancelBookingCommandHandler : ICommandHandler<CancelBookingCommand>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelBookingCommandHandler(IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
    {
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(CancelBookingCommand command, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(command.BookingId, cancellationToken);
        if (booking is null)
            return Result.Failure(BookingErrors.NotFound);

        if (booking.ClientId != command.ClientId)
            return Result.Failure(BookingErrors.UnauthorizedAction);

        var result = booking.Cancel();
        if (result.IsFailure)
            return result;

        _bookingRepository.Update(booking);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
