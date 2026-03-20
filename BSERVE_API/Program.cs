using BSERVE_API.Data;
using BSERVE_API.Endpoints;
using BSERVE_API.Hubs;
using BSERVE_API.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI nativo
builder.Services.AddOpenApi();

// Repositórios
builder.Services.AddScoped<ProdutoRepository>();
builder.Services.AddScoped<UsuarioRepository>();

// SignalR
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, UserIdProviderCustom>();

// Validãções
builder.Services.AddValidation();

// CORS — essencial para Blazor WASM + cookies
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.WithOrigins(
            "https://localhost:7153",
            "https://192.168.1.115:7153",
            "http://localhost:5263",
            "http://192.168.1.115:5263",
            "https://bserve.com.br"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials(); // 🔑 necessário para enviar cookies
    });
});

// Autenticação via cookie
builder.Services
    .AddAuthentication("BserveCookieAuth")
    .AddCookie("BserveCookieAuth", options =>
    {
        options.Cookie.Name = "BserveAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;

        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = 403;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Pipeline
app.UseHttpsRedirection();
app.UseCors("AllowBlazor");

app.UseAuthentication();
app.UseAuthorization();

// OpenAPI
app.MapOpenApi();

// Endpoints
app.MapProdutoEndpoints();
app.MapUsuarioEndpoints();
app.MapAuthEndpoints();

// SignalR

app.MapHub<ProdutosHub>("/hub/produtos").RequireCors("AllowBlazor");

//app.Run("http://*:3000");
app.Run();