using BSERVE_API.Data;
using BSERVE_API.Hubs;
using BSERVE_API.Models;
using Microsoft.AspNetCore.SignalR;
using Npgsql;

namespace BSERVE_API.Endpoints;

public static class ProdutoEndpoints
{
    public static void MapProdutoEndpoints(this WebApplication app)
    {


        var group = app.MapGroup("/produtos").RequireAuthorization(); // 🔑 Apenas usuários logados podem acessar;

        // GET /produtos
        group.MapGet("/", async (ProdutoRepository repo) =>
        {
            var produtos = await repo.GetAll();
            //await Task.Delay(200);
            return Results.Ok(produtos);
        });

        // GET /produtos/{id}
        group.MapGet("/{id}", async (int id, ProdutoRepository repo) =>
        {
            var produto = await repo.GetById(id);

            return produto is not null
                ? Results.Ok(produto)
                : Results.NotFound(new { mensagem = "Produto não encontrado" });
        });

        // POST /produtos
        group.MapPost("/", async (Produto produto, ProdutoRepository repo, IHubContext<ProdutosHub> hub) =>
        {
            try
            {
                var id = await repo.Create(produto);
                produto.Id = id;

                // 🔴 Notificação para todos clientes conectados
                await hub.Clients.All.SendAsync("produtoCriado", produto);

                return Results.Created($"/produtos/{id}", new
                {
                    sucesso = true,
                    mensagem = "Produto cadastrado com sucesso",
                    dados = produto
                });

            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                return Results.BadRequest(new
                {
                    erro = "codigo_barras_duplicado",
                    mensagem = "Já existe um produto com este código de barras"
                });
            }
            catch (Exception)
            {
                return Results.Problem("Erro interno ao criar produto");
            }
        });

        // PUT /produtos/{id}
        group.MapPut("/{id}", async (int id, Produto produto, ProdutoRepository repo, IHubContext<ProdutosHub> hub) =>
        {
            try
            {
                produto.Id = id;

                var updated = await repo.Update(produto);

                if (!updated)
                {
                    return Results.NotFound(new { mensagem = "Produto não encontrado" });
                }

                // 🔴 Notificação para todos clientes conectados
                await hub.Clients.All.SendAsync("produtoAtualizado", produto);

                return Results.Ok(new
                {
                    sucesso = true,
                    mensagem = "Produto atualizado com sucesso",
                    dados = produto
                });
            }
            catch (PostgresException ex) when (ex.SqlState == "23505")
            {
                return Results.BadRequest(new
                {
                    erro = "codigo_barras_duplicado",
                    mensagem = "Já existe um produto com este código de barras"
                });
            }
            catch (Exception)
            {
                return Results.Problem("Erro interno ao atualizar produto");
            }
        });

        // DELETE /produtos/{id}
        group.MapDelete("/{id}", async (int id, ProdutoRepository repo, IHubContext<ProdutosHub> hub) =>
        {
            try
            {
                var deleted = await repo.Delete(id);

                // 🔴 Notificação para todos clientes conectados
                await hub.Clients.All.SendAsync("produtoRemovido", id);

                return deleted
                    ? Results.NoContent()
                    : Results.NotFound(new { mensagem = "Produto não encontrado" });
            }
            catch (Exception)
            {
                return Results.Problem("Erro interno ao deletar produto");
            }
        });
    }
}