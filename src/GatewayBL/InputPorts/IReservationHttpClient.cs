using GatewayBL.Models;

namespace GatewayBL.InputPorts;

public interface IReservationHttpClient
{
    public Task<IEnumerable<Reservation>> GetReservationsAsync();
    public Task<Reservation> TakeBookAsync(Guid libraryUid, Guid bookUid, DateTime tillDate);
    public Task<Reservation> ReturnBookAsync(Guid reservationUid, DateTime returnDate);
    public Task<int> GetReservationCountAsync();
}