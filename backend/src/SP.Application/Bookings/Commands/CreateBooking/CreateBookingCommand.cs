// SP.Application/Bookings/Commands/CreateBooking/CreateBookingCommand.cs

using SP.Application.Abstractions.Messaging;
using SP.Domain.Abstractions;
using SP.Domain.Bookings;
using SP.Domain.Bookings.Errors;
using SP.Domain.Bookings.Repositories;
using SP.Domain.Services.Repositories;

namespace SP.Application.Bookings.Commands.CreateBooking;

public sealed record CreateBookingCommand(
    Guid ClientId,
    Guid ServiceId,
    DateTime ScheduledDate) : ICommand<Guid>;

public sealed class CreateBookingCommandHandler : ICommandHandler<CreateBookingCommand, Guid>
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBookingCommandHandler(
        IBookingRepository bookingRepository,
        IServiceRepository serviceRepository,
        IUnitOfWork unitOfWork)
    {
        _bookingRepository = bookingRepository;
        _serviceRepository = serviceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> HandleAsync(CreateBookingCommand command, CancellationToken cancellationToken = default)
    {
        var service = await _serviceRepository.GetByIdAsync(command.ServiceId, cancellationToken);
        if (service is null)
            return Result.Failure<Guid>(SP.Domain.Services.Errors.ServiceErrors.ServiceNotFound);

        var canBook = service.EnsureCanBeBooked();
        if (canBook.IsFailure)
            return Result.Failure<Guid>(canBook.Error);

        var bookingResult = Booking.Create(command.ClientId, service.ProviderId, command.ServiceId, command.ScheduledDate);
        if (bookingResult.IsFailure)
            return Result.Failure<Guid>(bookingResult.Error);

        await _bookingRepository.AddAsync(bookingResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(bookingResult.Value.Id);
    }
}
