using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GatewayBL.Exceptions;
using GatewayBL.InputPorts;
using GatewayBL.Models;
using GatewayHttp.Models;

namespace GatewayHttp.Clients;

public class ReservationHttpClient(HttpClient client) : IReservationHttpClient
{
    public async Task<IEnumerable<Reservation>> GetReservationsAsync()
    {
        using var response = await client.GetAsync(new Uri("/api/v1/reservations", UriKind.Relative));
        response.EnsureSuccessStatusCode();

        var reservations = 
            await response.Content.ReadFromJsonAsync<IEnumerable<ReservationResponse>>() ?? throw new JsonException();
        return reservations.Select(r => r.ToBlModel());
    }

    public async Task<Reservation> TakeBookAsync(Guid libraryUid, Guid bookUid, DateTime tillDate)
    {
        using var response = await client.PostAsJsonAsync(new Uri("api/v1/reservations", UriKind.Relative),
            new TakeBookRequest(libraryUid, bookUid, tillDate));
        response.EnsureSuccessStatusCode();

        var reservation = await response.Content.ReadFromJsonAsync<ReservationResponse>() ?? throw new JsonException();
        return reservation.ToBlModel();
    }

    public async Task<Reservation> ReturnBookAsync(Guid reservationUid, DateTime returnDate)
    {
        using var response = await client.PostAsJsonAsync($"api/v1/reservations/{reservationUid}/return",
            new ReturnBookRequest(returnDate));

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw new EntityNotFoundException();
        
        response.EnsureSuccessStatusCode();
        
        var reservation = await response.Content.ReadFromJsonAsync<ReservationResponse>() ?? throw new JsonException();
        return reservation.ToBlModel();
    }

    public async Task<int> GetReservationCountAsync()
    {
        using var response = await client.GetAsync(new Uri("api/v1/reservations/count", UriKind.Relative));
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<int>();
    }
}
