// SP.Application/Bookings/Commands/CompleteBooking/CompleteBookingCommand.cs

using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Bookings.Errors;
using SP.Domain.Bookings.Repositories;

namespace SP.Application.Bookings.Commands.CompleteBooking;

public sealed record CompleteBookingCommand(Guid BookingId, Guid ProviderId) : ICommand;

public sealed class CompleteBookingCommandHandler : ICommandHandler<CompleteBookingCommand>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteBookingCommandHandler(IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
    {
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(CompleteBookingCommand command, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(command.BookingId, cancellationToken);
        if (booking is null)
            return Result.Failure(BookingErrors.NotFound);

        if (booking.ProviderId != command.ProviderId)
            return Result.Failure(BookingErrors.UnauthorizedAction);

        var result = booking.Complete();
        if (result.IsFailure)
            return result;

        _bookingRepository.Update(booking);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
