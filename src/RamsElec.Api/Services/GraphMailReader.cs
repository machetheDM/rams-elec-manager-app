using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Me.MailFolders.Item.Messages;

namespace RamsElec.Api.Services;

public class GraphMailReader : IMailReader
{
    private readonly IConfiguration _config;
    private readonly ILogger<GraphMailReader> _logger;

    public GraphMailReader(IConfiguration config, ILogger<GraphMailReader> logger)
    {
        _config = config;
        _logger = logger;
    }

    public bool IsConfigured =>
        !string.IsNullOrEmpty(_config["Graph:TenantId"]) &&
        !string.IsNullOrEmpty(_config["Graph:ClientId"]) &&
        !string.IsNullOrEmpty(_config["Graph:ClientSecret"]);

    public async Task<List<EmailMessage>> GetUnreadPaymentNotificationsAsync(DateTime? since = null)
    {
        if (!IsConfigured)
        {
            _logger.LogWarning("Microsoft Graph is not configured");
            return [];
        }

        try
        {
            var clientId = _config["Graph:ClientId"]!;
            var tenantId = _config["Graph:TenantId"]!;
            var clientSecret = _config["Graph:ClientSecret"]!;

            var credentials = new ClientSecretCredential(tenantId, clientId, clientSecret);
            var graphClient = new GraphServiceClient(credentials);

            var mailbox = _config["Graph:Mailbox"] ?? "invoices@ramsatelec.co.za";
            var filter = since.HasValue
                ? $"receivedDateTime ge {since.Value:yyyy-MM-ddTHH:mm:ssZ}"
                : "isRead eq false";

            var response = await graphClient.Users[mailbox].Messages
                .GetAsync(req =>
                {
                    req.QueryParameters.Filter = filter;
                    req.QueryParameters.Select = ["id", "subject", "body", "receivedDateTime", "from"];
                    req.QueryParameters.Top = 50;
                });

            var messages = new List<EmailMessage>();
            if (response?.Value == null) return messages;

            foreach (var msg in response.Value)
            {
                messages.Add(new EmailMessage
                {
                    Id = msg.Id ?? string.Empty,
                    Subject = msg.Subject ?? string.Empty,
                    Body = msg.Body?.Content ?? string.Empty,
                    ReceivedDateTime = msg.ReceivedDateTime?.UtcDateTime ?? DateTime.UtcNow,
                    From = msg.From?.EmailAddress?.Address ?? string.Empty
                });
            }

            return messages;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read payment emails from Microsoft Graph");
            return [];
        }
    }
}
