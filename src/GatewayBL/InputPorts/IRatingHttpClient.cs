namespace GatewayBL.InputPorts;

public interface IRatingHttpClient
{
    public Task<int> GetUserRatingAsync();
    public Task UpdateUserRatingAsync(int stars);
}