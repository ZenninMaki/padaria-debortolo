using InfiniteCoffee2.Data;
using InfiniteCoffee2.Services;
using Microsoft.AspNetCore.Mvc;

namespace InfiniteCoffee2.Controllers.Api;

[ApiController]
[Route("api/health")]
public sealed class HealthApiController : ControllerBase
{
    private readonly GoogleDriveSnapshotStore _snapshotStore;

    public HealthApiController(GoogleDriveSnapshotStore snapshotStore)
    {
        _snapshotStore = snapshotStore;
    }

    [HttpGet]
    public async Task<IActionResult> Check()
    {
        try
        {
            if (string.Equals(Environment.GetEnvironmentVariable("PADARIA_SNAPSHOT_ONLY"), "true", StringComparison.OrdinalIgnoreCase))
            {
                var snapshot = await _snapshotStore.GetAsync();
                var versao = snapshot.TryGetProperty("versao", out var value) ? value.GetString() : null;
                return Ok(new { status = "ok", fonte = "google-drive", versao, horario = DateTime.UtcNow });
            }

            var serverVersion = Banco.ObterVersaoEstoque();
            return Ok(new { status = "ok", banco = "ok", versao = serverVersion, horario = DateTime.UtcNow });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = "degradado",
                banco = "indisponivel",
                mensagem = "Não foi possível consultar o banco local."
            });
        }
    }
}
