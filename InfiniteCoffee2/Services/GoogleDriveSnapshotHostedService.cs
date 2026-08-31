using System.Text;
using System.Text.Json;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using InfiniteCoffee2.Data;

namespace InfiniteCoffee2.Services;

/// <summary>
/// Publica somente o snapshot operacional do estoque. O SQL Server continua sendo
/// a fonte oficial; o Google Drive armazena apenas uma cópia de consulta.
/// </summary>
public sealed class GoogleDriveSnapshotHostedService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(30);
    private readonly ILogger<GoogleDriveSnapshotHostedService> _logger;
    private readonly string? _clientSecretPath;
    private readonly string? _folderId;
    private readonly string _fileName;

    public GoogleDriveSnapshotHostedService(ILogger<GoogleDriveSnapshotHostedService> logger)
    {
        _logger = logger;
        _clientSecretPath = Environment.GetEnvironmentVariable("GOOGLE_DRIVE_OAUTH_CLIENT_PATH");
        _folderId = Environment.GetEnvironmentVariable("GOOGLE_DRIVE_FOLDER_ID");
        _fileName = Environment.GetEnvironmentVariable("GOOGLE_DRIVE_SNAPSHOT_NAME") ?? "estoque.json";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.Equals(Environment.GetEnvironmentVariable("PADARIA_SNAPSHOT_ONLY"), "true", StringComparison.OrdinalIgnoreCase))
            return;

        if (string.IsNullOrWhiteSpace(_clientSecretPath) || string.IsNullOrWhiteSpace(_folderId))
        {
            _logger.LogInformation("Upload Google Drive desativado: configure GOOGLE_DRIVE_OAUTH_CLIENT_PATH e GOOGLE_DRIVE_FOLDER_ID.");
            return;
        }

        if (!File.Exists(_clientSecretPath))
        {
            _logger.LogWarning("Credencial OAuth do Google Drive não encontrada em {Path}. Upload desativado.", _clientSecretPath);
            return;
        }

        await UploadSnapshotAsync(stoppingToken);
        using var timer = new PeriodicTimer(Interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await UploadSnapshotAsync(stoppingToken);
        }
    }

    public async Task<bool> PublicarAgoraAsync(CancellationToken cancellationToken = default)
    {
        if (string.Equals(Environment.GetEnvironmentVariable("PADARIA_SNAPSHOT_ONLY"), "true", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(_clientSecretPath) || string.IsNullOrWhiteSpace(_folderId) ||
            !File.Exists(_clientSecretPath))
            return false;
        return await UploadSnapshotAsync(cancellationToken);
    }

    private async Task<bool> UploadSnapshotAsync(CancellationToken cancellationToken)
    {
        try
        {
            var clientSecrets = GoogleClientSecrets.FromFile(_clientSecretPath!).Secrets;
            var tokenPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PadariaDebortolo", "GoogleDriveToken");
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets,
                new[] { DriveService.Scope.Drive },
                "padaria-debortolo",
                cancellationToken,
                new FileDataStore(tokenPath, true));
            using var drive = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "Padaria Debortolo"
            });

            var snapshot = new
            {
                versao = Banco.ObterVersaoEstoque(),
                atualizadoEm = DateTime.UtcNow,
                produtos = Banco.ListarEstoque()
            };
            var json = JsonSerializer.Serialize(snapshot);
            await using var content = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var existing = await FindSnapshotAsync(drive, cancellationToken);
            var metadata = new Google.Apis.Drive.v3.Data.File
            {
                Name = _fileName,
                Parents = existing is null ? new List<string> { _folderId! } : null,
                MimeType = "application/json"
            };

            Google.Apis.Upload.IUploadProgress result;
            if (existing is null)
            {
                var request = drive.Files.Create(metadata, content, "application/json");
                request.Fields = "id, name, modifiedTime";
                result = await request.UploadAsync(cancellationToken);
            }
            else
            {
                var request = drive.Files.Update(metadata, existing.Id, content, "application/json");
                request.Fields = "id, name, modifiedTime";
                result = await request.UploadAsync(cancellationToken);
            }

            if (result.Status != Google.Apis.Upload.UploadStatus.Completed)
                throw result.Exception ?? new InvalidOperationException("Upload do snapshot não foi concluído.");

            _logger.LogInformation("Snapshot do estoque enviado ao Google Drive em {Time}.", DateTimeOffset.Now);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return false;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Não foi possível enviar o snapshot do estoque ao Google Drive.");
            return false;
        }
    }

    private async Task<Google.Apis.Drive.v3.Data.File?> FindSnapshotAsync(
        DriveService drive,
        CancellationToken cancellationToken)
    {
        var request = drive.Files.List();
        request.Q = $"'{_folderId}' in parents and name = '{_fileName.Replace("'", "\\'")}' and trashed = false";
        request.Fields = "files(id, name)";
        request.PageSize = 10;
        var result = await request.ExecuteAsync(cancellationToken);
        return result.Files.FirstOrDefault();
    }
}
