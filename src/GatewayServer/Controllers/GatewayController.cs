using GatewayBL.Enums;
using GatewayBL.Exceptions;
using GatewayBL.Models;
using GatewayBL.OutputPorts;
using GatewayServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static System.Enum;

namespace GatewayServer.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/")]
public class GatewayController(IGatewayManager gatewayManager) : ControllerBase
{
    private const int DefaultPage = 1;
    private const int DefaultSize = 10;
    private const bool DefaultShowAll = false;

    [HttpGet("libraries")]
    public async Task<ActionResult<LibraryPaginationResponse>> GetLibraries(string city, int page = DefaultPage,
        int size = DefaultSize)
    {
        var (total, libraries) = await gatewayManager.GetLibrariesAsync(city, page, size);

        return Ok(new LibraryPaginationResponse(page, size, total, libraries.Select(l => new LibraryResponse(l.LibraryUid, l.Name, l.Address, l.City))));
    }

    [HttpGet("libraries/{libraryUid:guid}/books")]
    public async Task<ActionResult<LibraryBookPaginationResponse>> GetBooks(Guid libraryUid, int page = DefaultPage,
        int size = DefaultSize, bool showAll = DefaultShowAll)
    {
        var (total, books) = await gatewayManager.GetBooksAsync(libraryUid, page, size, showAll);

        return Ok(new LibraryBookPaginationResponse(page, size, total, books.Select(b => new LibraryBookResponse(b.BookUid, b.Name, b.Author, b.Genre, b.Condition.ToString(), b.AvailableCount))));
    }

    [HttpGet("reservations")]
    public async Task<ActionResult<IEnumerable<BookReservationResponse>>> GetReservations()
    {
        var reservations = await gatewayManager.GetReservationsAsync();

        return Ok(reservations.Select(r => new BookReservationResponse(r.Reservation.ReservationUid,
            r.Reservation.Status.ToString(), r.Reservation.StartDate.ToString("yyyy-MM-dd"), r.Reservation.TillDate.ToString("yyyy-MM-dd"),
            new BookInfo(r.Book.BookUid, r.Book.Name, r.Book.Author, r.Book.Genre),
            new LibraryResponse(r.Library.LibraryUid, r.Library.Name, r.Library.Address, r.Library.City))));
    }

    [HttpPost("reservations")]
    public async Task<ActionResult<TakeBookResponse>> TakeBook([FromBody] TakeBookRequest request)
    {
        FullInfo reservation;
        
        try
        {
            reservation =
                await gatewayManager.TakeBookAsync(request.LibraryUid, request.BookUid, request.TillDate);
        }
        catch (TooManyRentedBooksException)
        {
            return BadRequest(new ValidationErrorResponse("", []));
        }

        return Ok(new TakeBookResponse(reservation.Reservation.ReservationUid,
            reservation.Reservation.Status.ToString(), reservation.Reservation.StartDate.ToString("yyyy-MM-dd"),
            reservation.Reservation.TillDate.ToString("yyyy-MM-dd"),
            new BookInfo(reservation.Book.BookUid, reservation.Book.Name, reservation.Book.Author,
                reservation.Book.Genre),
            new LibraryResponse(reservation.Library.LibraryUid, reservation.Library.Name, reservation.Library.Address,
                reservation.Library.City), new UserRatingResponse(reservation.Rating)));
    }

    [HttpPost("reservations/{reservationUid:guid}/return")]
    public async Task<ActionResult> ReturnBook(Guid reservationUid,
        [FromBody] ReturnBookRequest request)
    {
        if (!TryParse(request.Condition, out Condition condition))
            return BadRequest(new ValidationErrorResponse("", [new ErrorDescription(nameof(request.Condition), "")]));
        try
        {
            await gatewayManager.ReturnBookAsync(reservationUid, condition, request.Date);
        }
        catch (EntityNotFoundException)
        {
            return NotFound(new ErrorResponse("No reservation found"));
        }
        
        return NoContent();
    }

    [HttpGet("rating")]
    public async Task<ActionResult<UserRatingResponse>> GetUserRating()
    {
        var rating = await gatewayManager.GetUserRatingAsync();
        return Ok(new UserRatingResponse(rating));
    }
}
