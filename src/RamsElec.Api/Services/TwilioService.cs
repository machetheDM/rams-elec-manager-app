using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace RamsElec.Api.Services;

public class TwilioService
{
    private readonly IConfiguration _config;

    public TwilioService(IConfiguration config)
    {
        _config = config;
        var accountSid = _config["Twilio:AccountSid"];
        var authToken = _config["Twilio:AuthToken"];

        if (!string.IsNullOrEmpty(accountSid) && !string.IsNullOrEmpty(authToken))
            TwilioClient.Init(accountSid, authToken);
    }

    public bool IsConfigured => !string.IsNullOrEmpty(_config["Twilio:AccountSid"]) &&
                                !string.IsNullOrEmpty(_config["Twilio:AuthToken"]) &&
                                !string.IsNullOrEmpty(_config["Twilio:FromPhone"]);

    public async Task<string?> SendInvoiceLinkAsync(string toPhone, string presignedUrl, string invoiceNumber)
    {
        if (!IsConfigured)
            return "Twilio is not configured";

        try
        {
            var from = _config["Twilio:FromPhone"]!;
            var message = await MessageResource.CreateAsync(
                to: new PhoneNumber(toPhone),
                from: new PhoneNumber(from),
                body: $"Hi, your invoice {invoiceNumber} from RAMS@ELEC is ready: {presignedUrl}\n\nPlease use {invoiceNumber} as your EFT reference.");

            return message.Sid;
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}
