using DAO;
using DAO.SAPLINKER;
using DAO.SAPLINKER.SAPB1;
using DTO;
using DTO.SAPB1;
using DTO.SAPB1ServiceLayer;
using Helpers;
using INTERFACES;
using INTERFACES.Repositories;
using INTERFACES.Repositories.SAPB1;
using INTERFACES.Services;
using INTERFACES.Services.SAPB1;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ProyectoBaseCore.BackgroundServices;
using ProyectoBaseCore.Middleware;
using SERVICES;
using SERVICES.SAPB1;
using System.Net;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection("DatabaseSettings"));


builder.Services.Configure<LIMSDatabaseSettings>(
    builder.Configuration.GetSection("LIMSDatabaseSettings"));

builder.Services.Configure<HANADatabaseSettings>(
    builder.Configuration.GetSection("HANADatabaseSettings"));

builder.Services.AddScoped<Conexion>();

builder.Services.Configure<GlobalParameters>(
    builder.Configuration.GetSection("GlobalParameters"));

builder.Services.AddScoped<Encrypter>();
builder.Services.AddScoped<ClaimHelper>();
builder.Services.AddScoped<MailHelper>();
builder.Services.AddScoped<IMenuRepository, MenuDAO>();
builder.Services.AddScoped<IMenuService, MenuService>();

builder.Services.AddScoped<IUsuarioRepository, UsuarioDAO>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

builder.Services.AddScoped<IPerfilRepository, PerfilDAO>();
builder.Services.AddScoped<IPerfilService, PerfilService>();

builder.Services.AddScoped<IConfiguracionRepository, ConfiguracionDAO>();
builder.Services.AddScoped<IConfiguracionService,ConfiguracionService>();

builder.Services.AddScoped<IEtapaAutorizacionRepository, EtapaAutorizacionDAO>();
builder.Services.AddScoped<IEtapaAutorizacionService, EtapaAutorizacionService>();

builder.Services.AddScoped<IModeloAutorizacionRepository, ModeloAutorizacionDAO>();
builder.Services.AddScoped<IModeloAutorizacionService, ModeloAutorizacionService>();

builder.Services.AddScoped<IChequeTestRepository, ChequeTestDAO>();
builder.Services.AddScoped<IChequeTestService, ChequeTestService>();


builder.Services.AddScoped<IAprobacionesRepository, AprobacionesDAO>();
builder.Services.AddScoped<IAprobacionesService, AprobacionesService>();

builder.Services.AddScoped<IEstadoMuestrasRepository, EstadoMuestrasDAO>();
builder.Services.AddScoped<IEstadoMuestrasService, EstadoMuestrasService>();
builder.Services.AddScoped<IAprobacionesService, AprobacionesService>();

builder.Services.AddScoped<IPedidosSAPRepository, PedidosDAO>();
builder.Services.AddScoped<ILogsAutomatizcacionRepository, LogsAutomatizacionDAO>();

builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();

builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddSingleton<BackgroundTaskQueue>(); 
builder.Services.AddHostedService<QueuedHostedService>();

builder.Services.Configure<ServiceLayerSettings>(builder.Configuration.GetSection("ServiceLayer"));

builder.Services.AddHttpClient("ServiceLayerClient", client =>
{
    var settings = builder.Configuration.GetSection("ServiceLayer").Get<ServiceLayerSettings>();
    client.BaseAddress = new Uri(settings?.BaseUrl ?? "");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
    CookieContainer = new CookieContainer(),
    UseCookies = true
});
builder.Services.AddScoped<IPedidosSAPService,PedidosSAPB1Service>();
builder.Services.AddSingleton<AdminAutomatizadorPedidoService>();
builder.Services.AddSingleton<AdminAutomatPFService>();
builder.Services.AddScoped<IFacturaSAPService,FacturaSAPB1Service>();
builder.Services.AddScoped<OCRDSAPDAO>();
builder.Services.AddScoped<IOCRDSAPService, OCRDSAPService>();

builder.Services.AddScoped<IEstadoMuestrasPreFTRepository, EstadoMuestrasPreFTDAO>();
builder.Services.AddScoped<IEstadoMuestrasPreFTService, EstadoMuestrasPreFTService>();
builder.Services.AddScoped<ILogsAutomatizacionPFRepository, LogsAutomatizacionPFDAO>();

builder.Services.AddHostedService<AutomatizacionBackgroundWorker>();


builder.Services.Scan(scan => scan
    .FromAssemblies(typeof(UsuarioDAO).Assembly)
    .AddClasses(classes => classes.Where(t => t.Name.EndsWith("DAO")))
    .AsSelf()
    .WithScopedLifetime()
);

builder.Services
    .AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        var projectHash = builder.Configuration["GlobalParameters:ProjectHash"] ?? throw new InvalidOperationException("ProjectHash no est� configurado");

        var key = Encoding.UTF8.GetBytes(projectHash);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (string.IsNullOrEmpty(context.Token))
                {
                    context.Token = context.Request.Cookies["JWTToken"];
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services
    .AddControllersWithViews()
    .AddRazorRuntimeCompilation();

builder.Services.AddControllers().AddJsonOptions(options => {
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
});
builder.Services.AddAuthorization();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
