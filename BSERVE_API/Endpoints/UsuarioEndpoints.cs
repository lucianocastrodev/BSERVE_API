using BSERVE_API.Data;
using BSERVE_API.Models;
using Npgsql;

namespace BSERVE_API.Endpoints;

public static class UsuarioEndpoints
{
    public static void MapUsuarioEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/usuarios");

        // POST - criar usuário
        group.MapPost("/", async (Usuario usuario, UsuarioRepository repo) =>
        {
            try
            {
                await repo.Criar(usuario);
                return Results.Created($"/api/usuarios/{usuario.Id}", new
                {
                    sucesso = true,
                    mensagem = "Usuário cadastrado com sucesso",
                    dados = usuario
                });
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                return Results.BadRequest(new
                {
                    sucesso = false,
                    erro = "email_duplicado",
                    mensagem = "Email já cadastrado"
                });
            }
            catch (Exception ex)
            {
                return Results.Problem("Erro inesperado: " + ex.Message);
            }
        });

        // GET - todos usuários
        group.MapGet("/", async (UsuarioRepository repo) =>
        {
            var lista = await repo.ObterTodos();
            return Results.Ok(new
            {
                sucesso = true,
                dados = lista
            });
        });

        // GET - usuário por id
        group.MapGet("/{id:guid}", async (Guid id, UsuarioRepository repo) =>
        {
            var lista = await repo.ObterTodos(); // se quiser, pode criar repo.ObterPorId
            var u = lista.FirstOrDefault(u => u.Id == id);
            if (u == null) return Results.NotFound(new { mensagem = "Usuário não encontrado" });

            return Results.Ok(new
            {
                sucesso = true,
                dados = u
            });
        });

        // PUT - atualizar usuário
        group.MapPut("/{id:guid}", async (Guid id, Usuario usuario, UsuarioRepository repo) =>
        {
            if (id != usuario.Id)
                return Results.BadRequest(new { mensagem = "ID inválido" });

            var sucesso = await repo.Atualizar(usuario);
            if (!sucesso) return Results.NotFound(new { mensagem = "Usuário não encontrado" });

            return Results.Ok(new
            {
                sucesso = true,
                mensagem = "Usuário atualizado com sucesso",
                dados = usuario
            });
        });

        // DELETE - deletar usuário
        group.MapDelete("/{id:guid}", async (Guid id, UsuarioRepository repo) =>
        {
            var sucesso = await repo.Deletar(id);
            if (!sucesso) return Results.NotFound(new { mensagem = "Usuário não encontrado" });

            return Results.NoContent();
        });
    }
}