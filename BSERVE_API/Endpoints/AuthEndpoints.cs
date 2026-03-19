using BSERVE_API.Data;
using BSERVE_API.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Npgsql;

namespace BSERVE_API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth");

        // POST /login
        group.MapPost("/login", async (LoginDto dto, UsuarioRepository repo, HttpContext context) =>
        {
            var usuario = await repo.ObterPorEmail(dto.Email);
            if (usuario == null || !usuario.Ativo)
                return Results.Unauthorized();

            var passwordHasher = new PasswordHasher<string>();
            var result = passwordHasher.VerifyHashedPassword(null!, usuario.SenhaHash, dto.Senha);
            if (result == PasswordVerificationResult.Failed)
                return Results.Unauthorized();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Role)
            };

            var identity = new ClaimsIdentity(claims, "BserveCookieAuth");
            var principal = new ClaimsPrincipal(identity);

            await context.SignInAsync(
                "BserveCookieAuth",
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true // mantém sessão mesmo após fechar o navegador
                });

            return Results.Ok(new { sucesso = true, mensagem = "Login realizado com sucesso" });
        }).RequireCors("AllowBlazor");

        // POST /logout
        group.MapPost("/logout", async (HttpContext context) =>
        {
            await context.SignOutAsync("BserveCookieAuth");
            return Results.Ok(new { sucesso = true, mensagem = "Logout realizado" });
        }).RequireCors("AllowBlazor");

        // GET /me
        group.MapGet("/me", (HttpContext context) =>
        {
            if (context.User.Identity?.IsAuthenticated ?? false)
            {
                var usuario = new Usuario
                {
                    Id = Guid.TryParse(
                            context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                            out var guid) ? guid : Guid.Empty,
                    Nome = context.User.Identity.Name ?? "",
                    Email = context.User.FindFirst(ClaimTypes.Email)?.Value ?? "",
                    Role = context.User.FindFirst(ClaimTypes.Role)?.Value ?? "",
                    SenhaHash = "" // nunca envie a senha real!
                };

                return Results.Ok(new SessionModel
                {
                    Logado = true,
                    Usuario = usuario
                });
            }

            return Results.Unauthorized();
        }).RequireCors("AllowBlazor");
    }
}

// DTO e modelo
public record LoginDto(string Email, string Senha);

public class SessionModel
{
    public bool Logado { get; set; }
    public Usuario Usuario { get; set; } = new Usuario();
}