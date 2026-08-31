using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using GestaoColaboradores.Api;
using GestaoColaboradores.Api.Middlewares;
using GestaoColaboradores.Application;
using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.Interfaces;
using GestaoColaboradores.Infrastructure;
using GestaoColaboradores.Infrastructure.Auth;
using GestaoColaboradores.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Normalização na borda: toda string do corpo chega sem espaços nas pontas, exceto senha.
builder.Services.AddControllers()
    .AddJsonOptions(o => Normalizacao.Configurar(o.JsonSerializerOptions));

builder.Services.AddFluentValidationAutoValidation(); // validators do Application rodando no pipeline MVC

// ---- Autenticação JWT (diferencial do enunciado) ----
var jwt = builder.Configuration.GetSection(JwtSettings.Secao).Get<JwtSettings>()
          ?? throw new InvalidOperationException("Seção 'Jwt' ausente na configuração.");

// Falha no arranque, não na primeira requisição: um segredo curto ou o placeholder do
// appsettings em produção deixariam a assinatura do token trivial de forjar.
jwt.Validar(builder.Environment.IsDevelopment());
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Emissor,
            ValidateAudience = true,
            ValidAudience = jwt.Emissor,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();

// ---- Rate limiting no login ----
// Só o endpoint de autenticação: é o único que um atacante repete milhares de vezes, e o
// BCrypt torna cada tentativa cara para o servidor também. A janela é por IP para que um
// atacante não consiga trancar a porta dos demais usuários.
var loginPorMinuto = builder.Configuration.GetValue("RateLimit:LoginPorMinuto", 5);

builder.Services.AddRateLimiter(opt =>
{
    opt.AddPolicy(PoliticasDeLimite.Login, contexto =>
        RateLimitPartition.GetFixedWindowLimiter(
            contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            _ => new FixedWindowRateLimiterOptions
            {
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = loginPorMinuto,
                QueueLimit = 0
            }));

    // Mesmo formato dos demais erros da API.
    opt.OnRejected = async (contexto, ct) =>
    {
        contexto.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        await contexto.HttpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Tentativas demais.",
                Detail = "Aguarde um minuto antes de tentar novamente.",
                Instance = contexto.HttpContext.Request.Path
            },
            options: null,
            contentType: "application/problem+json",
            cancellationToken: ct);
    };
});

// ---- Health check ----
// Verifica também o banco: uma API que responde mas não alcança o PostgreSQL está fora do ar
// para qualquer efeito prático, e um orquestrador precisa saber disso.
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>("banco");

// ---- Swagger com esquema Bearer (testável pela UI, com cadeado) ----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt =>
{
    opt.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Gestão de Colaboradores e Unidades",
        Version = "v1",
        Description = "Cadastro de usuários, unidades e colaboradores. Autenticação por Bearer token: "
                    + "obtenha o token em /api/v1/auth/login e informe-o no botão Authorize."
    });

    // Faz os <summary> dos controllers aparecerem como descrição de cada endpoint.
    var xml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xml))
        opt.IncludeXmlComments(xml);
    opt.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token obtido em /api/v1/auth/login"
    });
    opt.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", doc), [] }
    });
});

builder.Services.AddCors(opt => opt.AddDefaultPolicy(p =>
    p.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// ---- Migrations + seed no startup: "docker compose up" → sistema pronto ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    var gerador = scope.ServiceProvider.GetRequiredService<IGeradorCodigo>();
    await DbSeeder.SeedAsync(db, hasher, gerador);
}

app.UseMiddleware<ExcecaoMiddleware>(); // ProblemDetails para qualquer exceção não tratada

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();

app.Run();

public partial class Program { } // exposto para os testes de integração (WebApplicationFactory)
