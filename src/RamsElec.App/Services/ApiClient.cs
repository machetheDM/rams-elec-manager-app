using System.Net.Http.Headers;
using System.Net.Http.Json;
using RamsElec.Shared.DTOs;
using RamsElec.Shared.Enums;
using RamsElec.Shared.Models;

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

    public async Task<InvoiceDto?> GetInvoiceAsync(string id)
    {
        var response = await _http.GetAsync($"api/invoice/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<InvoiceDto>();
    }

    public async Task<bool> SendInvoiceAsync(string id, DeliveryChannel channel, string customerId)
    {
        var dto = new SendInvoiceDto { InvoiceId = id, Channel = channel };
        var response = await _http.PostAsJsonAsync($"api/invoice/{id}/send", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RecordPaymentAsync(string id, decimal amount)
    {
        var dto = new RecordPaymentDto
        {
            InvoiceId = id,
            Amount = amount,
            Method = PaymentMethod.Eft
        };
        var response = await _http.PostAsJsonAsync($"api/invoice/{id}/pay", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<CompanyInfo?> GetCompanyInfoAsync()
    {
        return await _http.GetFromJsonAsync<CompanyInfo>("api/companyinfo");
    }

    public async Task<CompanyInfo?> SaveCompanyInfoAsync(CompanyInfo company)
    {
        var response = await _http.PutAsJsonAsync("api/companyinfo", company);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<CompanyInfo>();
    }

    public async Task<DashboardAnalyticsDto?> GetDashboardAnalyticsAsync()
    {
        return await _http.GetFromJsonAsync<DashboardAnalyticsDto>("api/analytics/dashboard");
    }

    // Payments
    public async Task<List<Payment>> GetPaymentsForInvoiceAsync(string invoiceId)
    {
        return await _http.GetFromJsonAsync<List<Payment>>($"api/payment/invoice/{invoiceId}") ?? [];
    }

    public async Task<bool> RecordPaymentAsync(RecordPaymentDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/payment", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RecordFnbPaymentAsync(FnbPaymentDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/payment/fnb", dto);
        return response.IsSuccessStatusCode;
    }
}
