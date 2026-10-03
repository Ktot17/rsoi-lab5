using System.Net.Http.Json;
using System.Text.Json;
using GatewayBL.InputPorts;
using GatewayHttp.Models;

namespace GatewayHttp.Clients;

public class RatingHttpClient(HttpClient client) : IRatingHttpClient
{
    public async Task<int> GetUserRatingAsync()
    {
        using var response = await client.GetAsync(new Uri("/api/v1/rating", UriKind.Relative));
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<UserRatingResponse>() ?? throw new JsonException()).Stars;
    }

    public async Task UpdateUserRatingAsync(int stars)
    {
        using var response = await client.PatchAsync(new Uri("/api/v1/rating?stars=" + stars, UriKind.Relative), null);
        response.EnsureSuccessStatusCode();
    }
}
