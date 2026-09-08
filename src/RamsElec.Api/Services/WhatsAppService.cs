using System.Net.Http.Json;

namespace RamsElec.Api.Services;

public class WhatsAppService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public WhatsAppService(IConfiguration config)
    {
        _config = config;
        _http = new HttpClient();
    }

    public bool IsConfigured => !string.IsNullOrEmpty(_config["N8N:WebhookUrl"]);

    public async Task<string?> SendInvoiceLinkAsync(string toPhone, string presignedUrl, string invoiceNumber, string? customerName = null)
    {
        if (!IsConfigured)
            return "n8n webhook is not configured";

        try
        {
            var payload = new
            {
                phone = toPhone,
                message = $"Hi {customerName ?? "there"}, your invoice {invoiceNumber} from RAMS@ELEC is ready: {presignedUrl}",
                invoiceNumber,
                reference = invoiceNumber
            };

            var response = await _http.PostAsJsonAsync(_config["N8N:WebhookUrl"], payload);
            if (response.IsSuccessStatusCode)
                return "sent";

            return $"n8n returned {(int)response.StatusCode}";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}
