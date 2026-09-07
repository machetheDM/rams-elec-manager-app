using System.Net.Http.Headers;
using System.Net.Http.Json;
using RamsElec.Shared.DTOs;

namespace RamsElec.App.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private string? _token;

    public ApiClient()
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(GetBaseUrl())
        };
    }

    private static string GetBaseUrl()
    {
#if ANDROID
        return "http://10.0.2.2:5000/";
#else
        return "http://localhost:5000/";
#endif
    }

    public void SetToken(string token)
    {
        _token = token;
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public void ClearToken()
    {
        _token = null;
        _http.DefaultRequestHeaders.Authorization = null;
    }

    public bool HasToken => !string.IsNullOrEmpty(_token);

    // Auth
    public async Task<AuthResponseDto?> LoginAsync(LoginDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
    }

    // Invoices
    public async Task<List<InvoiceDto>> GetInvoicesAsync()
    {
        return await _http.GetFromJsonAsync<List<InvoiceDto>>("api/invoice") ?? [];
    }

    public async Task<InvoiceDto?> CreateInvoiceAsync(CreateInvoiceDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/invoice", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<InvoiceDto>();
    }

    public async Task<byte[]?> GetInvoicePdfAsync(string id)
    {
        var response = await _http.GetAsync($"api/invoice/{id}/pdf");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadAsByteArrayAsync();
    }

    // Sync
    public async Task<SyncResponseDto?> SyncAsync(SyncRequestDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/sync", dto);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<SyncResponseDto>();
    }
}
