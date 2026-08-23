using InfiniteCoffee2.Data;
using InfiniteCoffee2.Services;

namespace InfiniteCoffee2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddHostedService<GoogleDriveSnapshotHostedService>();
            builder.Services.AddCors(options =>
            {
                // Em desenvolvimento o app Flutter (Windows, mobile ou web) conversa com esta API.
                // Libera localhost, 127.0.0.1 e a faixa de IP de rede local (192.168./10.).
                options.AddPolicy("FlutterDevelopment", policy => policy
                    .SetIsOriginAllowed(origin =>
                        origin.StartsWith("http://localhost:", StringComparison.OrdinalIgnoreCase) ||
                        origin.StartsWith("http://127.0.0.1:", StringComparison.OrdinalIgnoreCase) ||
                        origin.Contains("192.168.") ||
                        origin.Contains("10.0.") ||
                        origin.Contains("10.0.2.2"))
                    .AllowAnyHeader()
                    .AllowAnyMethod());
            });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Infinite Coffee API",
                    Version = "v1",
                    Description = "API de controle de estoque da cafeteria."
                });
            });

            // Session é necessária para guardar os dados do atendimento entre as etapas
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            // A estrutura de sync e criada na primeira operacao de banco. Isso permite
            // que o servidor suba em CI/desenvolvimento mesmo sem SQL Server disponivel;
            // a inicializacao continua idempotente quando a API realmente e usada.

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors("FlutterDevelopment");

            // Swagger fica disponível para o grupo testar as APIs durante o desenvolvimento.
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Infinite Coffee API v1");
                options.RoutePrefix = "swagger";
            });

            app.UseAuthorization();

            // Habilita Session
            app.UseSession();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();

        }
    }
}
