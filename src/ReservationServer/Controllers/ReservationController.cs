using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservationBL.Exceptions;
using ReservationBL.Models;
using ReservationBL.OutputPorts;
using ReservationServer.Models;

namespace ReservationServer.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/reservations/")]
public class ReservationController(IReservationManager reservationManager) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReservationResponse>>> GetReservationsAsync()
    {
        var username = User.Identity?.Name!;
        var reservations = await reservationManager.GetReservations(username);
        return Ok(reservations.Select(r => new ReservationResponse(r)));
    }

    [HttpPost]
    public async Task<ActionResult<ReservationResponse>> TakeBookAsync([FromBody] TakeBookRequest request)
    {
        var username = User.Identity?.Name!;
        var reservation = await reservationManager.TakeBook(username, request.LibraryUid, request.BookUid, request.TillDate);
        return Ok(new ReservationResponse(reservation));
    }

    [HttpPost("{reservationUid:guid}/return")]
    public async Task<ActionResult<ReservationResponse>> ReturnBookAsync(Guid reservationUid, [FromBody] ReturnBookRequest returnDate)
    {
        Reservation reservation;
        
        try
        {
            reservation = await reservationManager.ReturnBook(reservationUid, returnDate.Date);
        }
        catch (EntityNotFoundException)
        {
            return NotFound();
        }
        
        return Ok(new ReservationResponse(reservation));
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetReservationCountAsync()
    {
        var username = User.Identity?.Name!;
        var count = await reservationManager.GetRentedReservationCountAsync(username);
        return Ok(count);
    }
}
