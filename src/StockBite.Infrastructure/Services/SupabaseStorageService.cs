using Microsoft.Extensions.Configuration;
using StockBite.Application.Common.Interfaces;

namespace StockBite.Infrastructure.Services;

public class SupabaseStorageService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    : IStorageService
{
    private const string Bucket = "menu-images";

    private string SupabaseUrl => configuration["SupabaseUrl"]
        ?? throw new InvalidOperationException("SupabaseUrl is not configured.");

    private string ServiceKey => configuration["SupabaseServiceKey"]
        ?? throw new InvalidOperationException("SupabaseServiceKey is not configured.");

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default)
    {
        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ServiceKey}");
        client.DefaultRequestHeaders.Add("apikey", ServiceKey);

        var uniqueName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var url = $"{SupabaseUrl}/storage/v1/object/{Bucket}/{uniqueName}";

        using var content = new StreamContent(fileStream);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

        var response = await client.PostAsync(url, content, ct);
        response.EnsureSuccessStatusCode();

        return $"{SupabaseUrl}/storage/v1/object/public/{Bucket}/{uniqueName}";
    }

    public async Task DeleteAsync(string fileUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(fileUrl)) return;

        // Extract the file name from the public URL
        var prefix = $"{SupabaseUrl}/storage/v1/object/public/{Bucket}/";
        if (!fileUrl.StartsWith(prefix)) return;

        var objectName = fileUrl[prefix.Length..];
        var url = $"{SupabaseUrl}/storage/v1/object/{Bucket}/{objectName}";

        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ServiceKey}");
        client.DefaultRequestHeaders.Add("apikey", ServiceKey);

        await client.DeleteAsync(url, ct);
    }
}
