// SP.Application/Bookings/Commands/AddReport/AddReportCommand.cs

using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Bookings.Errors;
using SP.Domain.Bookings.Repositories;

namespace SP.Application.Bookings.Commands.AddReport;

public sealed record AddReportCommand(
    Guid BookingId,
    Guid ReporterId,
    string Reason) : ICommand;

public sealed class AddReportCommandHandler : ICommandHandler<AddReportCommand>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddReportCommandHandler(IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
    {
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(AddReportCommand command, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.GetByIdAsync(command.BookingId, cancellationToken);
        if (booking is null)
            return Result.Failure(BookingErrors.NotFound);

        if (booking.ClientId != command.ReporterId && booking.ProviderId != command.ReporterId)
            return Result.Failure(BookingErrors.UnauthorizedAction);

        var result = booking.AddReport(command.ReporterId, command.Reason);
        if (result.IsFailure)
            return result;

        _bookingRepository.Update(booking);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
