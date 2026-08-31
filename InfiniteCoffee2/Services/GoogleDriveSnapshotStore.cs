using System.Text.Json;

namespace InfiniteCoffee2.Services;

/// <summary>
/// Le o snapshot publico de estoque no modo cloud. O arquivo e somente leitura;
/// vendas e alteracoes continuam pertencendo ao desktop/SQL Server local.
/// </summary>
public sealed class GoogleDriveSnapshotStore
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private readonly HttpClient _http;
    private readonly ILogger<GoogleDriveSnapshotStore> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private JsonElement? _cached;
    private DateTimeOffset _cachedAt;

    public GoogleDriveSnapshotStore(HttpClient http, ILogger<GoogleDriveSnapshotStore> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<JsonElement> GetAsync(CancellationToken cancellationToken = default)
    {
        if (_cached is { } cached && DateTimeOffset.UtcNow - _cachedAt < CacheDuration)
            return cached;

        var fileId = Environment.GetEnvironmentVariable("GOOGLE_DRIVE_SNAPSHOT_FILE_ID");
        if (string.IsNullOrWhiteSpace(fileId))
            throw new InvalidOperationException("GOOGLE_DRIVE_SNAPSHOT_FILE_ID não foi configurado.");

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_cached is { } refreshed && DateTimeOffset.UtcNow - _cachedAt < CacheDuration)
                return refreshed;

            var url = $"https://drive.google.com/uc?export=download&id={Uri.EscapeDataString(fileId)}";
            using var response = await _http.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            _cached = document.RootElement.Clone();
            _cachedAt = DateTimeOffset.UtcNow;
            return _cached.Value;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Não foi possível baixar o snapshot do Google Drive.");
            if (_cached is { } stale)
                return stale;
            throw;
        }
        finally
        {
            _lock.Release();
        }
    }

}
