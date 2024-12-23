using Frontline.Options;
using Microsoft.Extensions.Options;

namespace Frontline.Services;

public interface IToyService
{
    Task<bool> VerifyUserAsync(long id, string password);
}

public class ToyService : IToyService
{
    private readonly HttpClient _httpClient;

    public ToyService(HttpClient httpClient, IOptions<UrlOptions> urlOptions)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(urlOptions.Value.ToyUrl);
    }

    public async Task<bool> VerifyUserAsync(long id, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("gameserver/verifyUser",
            new { Id = id, Password = password });
        
        return response.IsSuccessStatusCode;
    }
}