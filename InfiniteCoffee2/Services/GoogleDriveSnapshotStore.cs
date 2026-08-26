using System.Text.Json;
using System.Text;
using System.Text.Json.Nodes;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;

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

    public async Task<bool> ApplyMovementAsync(int productId, int quantity, bool entry, CancellationToken cancellationToken = default)
    {
        if (quantity < 1) return false;
        var serviceAccountJson = Environment.GetEnvironmentVariable("GOOGLE_DRIVE_SERVICE_ACCOUNT_JSON");
        var fileId = Environment.GetEnvironmentVariable("GOOGLE_DRIVE_SNAPSHOT_FILE_ID");
        if (string.IsNullOrWhiteSpace(serviceAccountJson) || string.IsNullOrWhiteSpace(fileId))
            return false;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            var snapshot = await DownloadAsync(fileId, cancellationToken);
            var root = JsonNode.Parse(snapshot.GetRawText())?.AsObject();
            var products = root?["produtos"]?.AsArray();
            var product = products?.FirstOrDefault(item =>
                item?["id_produto"]?.GetValue<int>() == productId)?.AsObject();
            if (product is null) return false;

            var current = product["quantidade_estoque"]?.GetValue<int>() ?? 0;
            if (!entry && current < quantity) return false;
            product["quantidade_estoque"] = entry ? current + quantity : current - quantity;
            root!["atualizadoEm"] = DateTime.UtcNow;
            root["versao"] = $"drive-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
            await UploadAsync(fileId, serviceAccountJson, root.ToJsonString(), cancellationToken);
            _cached = JsonDocument.Parse(root.ToJsonString()).RootElement.Clone();
            _cachedAt = DateTimeOffset.UtcNow;
            return true;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(exception, "Nao foi possivel atualizar o snapshot no Google Drive.");
            return false;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<JsonElement> DownloadAsync(string fileId, CancellationToken cancellationToken)
    {
        var url = $"https://drive.google.com/uc?export=download&id={Uri.EscapeDataString(fileId)}";
        using var response = await _http.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        return document.RootElement.Clone();
    }

    private static async Task UploadAsync(string fileId, string serviceAccountJson, string json, CancellationToken cancellationToken)
    {
        var credential = GoogleCredential.FromJson(serviceAccountJson)
            .CreateScoped(DriveService.Scope.Drive);
        using var drive = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "Padaria Debortolo"
        });
        await using var content = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var metadata = new Google.Apis.Drive.v3.Data.File { MimeType = "application/json" };
        var request = drive.Files.Update(metadata, fileId, content, "application/json");
        request.Fields = "id, modifiedTime";
        var result = await request.UploadAsync(cancellationToken);
        if (result.Status != Google.Apis.Upload.UploadStatus.Completed)
            throw result.Exception ?? new InvalidOperationException("Upload do snapshot nao foi concluido.");
    }
}
