using System.IO;
using System.Linq;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace RamsElec.Api.Services;

public class S3Service
{
    private readonly IAmazonS3 _s3;
    private readonly IConfiguration _config;
    private readonly string _bucket;

    public S3Service(IConfiguration config)
    {
        _config = config;
        _bucket = config["S3:BucketName"] ?? "rams-elec-invoices";

        var accessKey = config["S3:AccessKey"];
        var secretKey = config["S3:SecretKey"];
        var region = config["S3:Region"] ?? "af-south-1";

        _s3 = !string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey)
            ? new AmazonS3Client(accessKey, secretKey, RegionEndpoint.GetBySystemName(region))
            : new AmazonS3Client(RegionEndpoint.GetBySystemName(region));
    }

    public async Task<string> UploadInvoicePdfAsync(string invoiceNumber, byte[] pdf, string contentType = "application/pdf")
    {
        var key = $"invoices/{DateTime.UtcNow.Year}/{invoiceNumber}.pdf";

        var request = new PutObjectRequest
        {
            BucketName = _bucket,
            Key = key,
            InputStream = new MemoryStream(pdf),
            ContentType = contentType,
            CannedACL = S3CannedACL.Private
        };

        await _s3.PutObjectAsync(request);
        return key;
    }

    public async Task<string> GetPresignedUrlAsync(string key, TimeSpan? expiry = null)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucket,
            Key = key,
            Expires = DateTime.UtcNow.Add(expiry ?? TimeSpan.FromHours(24))
        };

        return await _s3.GetPreSignedURLAsync(request);
    }

    public async Task<bool> BucketExistsAsync()
    {
        try
        {
            var response = await _s3.ListBucketsAsync();
            return response.Buckets.Any(b => string.Equals(b.BucketName, _bucket, StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return false;
        }
    }
}
