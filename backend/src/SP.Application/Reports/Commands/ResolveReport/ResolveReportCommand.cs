// SP.Application/Reports/Commands/ResolveReport/ResolveReportCommand.cs

using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Bookings.Errors;
using SP.Domain.Bookings.Repositories;

namespace SP.Application.Reports.Commands.ResolveReport;

public sealed record ResolveReportCommand(Guid ReportId) : ICommand;

public sealed class ResolveReportCommandHandler : ICommandHandler<ResolveReportCommand>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ResolveReportCommandHandler(IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
    {
        _bookingRepository = bookingRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> HandleAsync(ResolveReportCommand command, CancellationToken cancellationToken = default)
    {
        var allBookings = await _bookingRepository.GetAllAsync(cancellationToken);
        var booking = allBookings.FirstOrDefault(b => b.Report?.Id == command.ReportId);

        if (booking?.Report is null)
            return Result.Failure(BookingErrors.NotFound);

        var result = booking.ResolveReport();
        if (result.IsFailure)
            return result;

        _bookingRepository.Update(booking);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
