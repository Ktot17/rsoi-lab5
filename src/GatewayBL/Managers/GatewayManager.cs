using GatewayBL.Enums;
using GatewayBL.Exceptions;
using GatewayBL.InputPorts;
using GatewayBL.Models;
using GatewayBL.OutputPorts;

namespace GatewayBL.Managers;

public class GatewayManager(
    ILibraryHttpClient libraryHttpClient,
    IRatingHttpClient ratingHttpClient,
    IReservationHttpClient reservationHttpClient) : IGatewayManager
{
    public async Task<(int, IEnumerable<Library>)> GetLibrariesAsync(string city, int page, int size) => 
        await libraryHttpClient.GetLibraries(city, page, size);
    
    public async Task<(int, IEnumerable<Book>)> GetBooksAsync(Guid libraryUid, int page, int size, bool showAll) => 
        await libraryHttpClient.GetBooks(libraryUid, page, size, showAll);

    public async Task<IEnumerable<FullInfo>> GetReservationsAsync()
    {
        var reservations = (await reservationHttpClient.GetReservationsAsync()).ToList();

        var libraryUids = reservations.Select(r => r.LibraryUid).Distinct().ToList();
        var bookUids = reservations.Select(r => r.BookUid).Distinct().ToList();

        var (libraries, books) = await libraryHttpClient.GetLibrariesAndBooksAsync(libraryUids, bookUids);

        var librariesById = libraries.ToDictionary(l => l.LibraryUid);
        var booksById = books.ToDictionary(b => b.BookUid);
        
        return reservations.Select(r => new FullInfo(r, booksById[r.BookUid], librariesById[r.LibraryUid]));
    }

    public async Task<FullInfo> TakeBookAsync(Guid libraryUid, Guid bookUid, DateTime tillDate)
    {
        var rentedCountTask = reservationHttpClient.GetReservationCountAsync();
        var ratingTask = ratingHttpClient.GetUserRatingAsync();
        
        await Task.WhenAll(rentedCountTask, ratingTask);
        
        var rentedCount = await rentedCountTask;
        var rating = await ratingTask;

        if (rentedCount >= rating)
            throw new TooManyRentedBooksException();
        
        var reservation = await reservationHttpClient.TakeBookAsync(libraryUid, bookUid, tillDate);
        await libraryHttpClient.ChangeAvailableCountAsync(libraryUid, bookUid, -1);
        var (libraries, books) = 
            await libraryHttpClient.GetLibrariesAndBooksAsync([libraryUid], [bookUid]);
        
        return new FullInfo(reservation, books.First(), libraries.First(), rating);
    }

    public async Task ReturnBookAsync(Guid reservationUid, Condition condition, DateTime returnDate)
    {
        var reservation = await reservationHttpClient.ReturnBookAsync(reservationUid, returnDate);
        await libraryHttpClient.ChangeAvailableCountAsync(reservation.LibraryUid, reservation.BookUid, 1);
        var conditionMismatch = condition is Condition.BAD;
        var isExpired = reservation.Status is Status.EXPIRED;

        var count = (conditionMismatch, isExpired) switch
        {
            (true, true) => 2,
            (true, false) or (false, true) => 1,
            _ => 0
        };

        var ratingChange = count == 0 ? 1 : -10 * count;
        await ratingHttpClient.UpdateUserRatingAsync(ratingChange);
    }

    public async Task<int> GetUserRatingAsync() => 
        await ratingHttpClient.GetUserRatingAsync();
}
