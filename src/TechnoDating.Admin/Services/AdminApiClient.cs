using System.Net.Http.Json;
using TechnoDating.Contracts;

namespace TechnoDating.Admin.Services;

/// <summary>Server-side typed client for the API's admin endpoints. The X-Admin-Key header is
/// attached by the registered HttpClient, so it never reaches the browser.</summary>
public class AdminApiClient(HttpClient http)
{
    public async Task<IReadOnlyList<AdminUserDto>> GetUsersAsync()
    {
        return await http.GetFromJsonAsync<List<AdminUserDto>>("/api/admin/users") ?? [];
    }

    public async Task<IReadOnlyList<AdminMatchDto>> GetMatchesAsync()
    {
        return await http.GetFromJsonAsync<List<AdminMatchDto>>("/api/admin/matches") ?? [];
    }

    public async Task<string?> ForceMatchAsync(Guid userAId, Guid userBId)
    {
        var response = await http.PostAsJsonAsync("/api/admin/matches/force", new ForceMatchDto(userAId, userBId));
        if (response.IsSuccessStatusCode)
        {
            return null;
        }
        return $"{(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}";
    }

    public async Task CloseMatchAsync(Guid matchId)
    {
        await http.DeleteAsync($"/api/admin/matches/{matchId}");
    }

    public async Task<IReadOnlyList<ArtistDto>> GetArtistsAsync()
    {
        return await http.GetFromJsonAsync<List<ArtistDto>>("/api/admin/artists") ?? [];
    }

    public async Task<IReadOnlyList<AdminFestivalDto>> GetFestivalsAsync()
    {
        return await http.GetFromJsonAsync<List<AdminFestivalDto>>("/api/admin/festivals") ?? [];
    }

    public async Task CreateFestivalAsync(SaveFestivalDto festival)
    {
        var response = await http.PostAsJsonAsync("/api/admin/festivals", festival);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateFestivalAsync(Guid id, SaveFestivalDto festival)
    {
        var response = await http.PutAsJsonAsync($"/api/admin/festivals/{id}", festival);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteFestivalAsync(Guid id)
    {
        await http.DeleteAsync($"/api/admin/festivals/{id}");
    }
}
