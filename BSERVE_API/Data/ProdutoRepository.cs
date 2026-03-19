using Dapper;
using Npgsql;
using BSERVE_API.Models;
using System.Data;

namespace BSERVE_API.Data;

public class ProdutoRepository
{
    private readonly string _connectionString;

    public ProdutoRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    private IDbConnection Connection()
        => new NpgsqlConnection(_connectionString);

    // Retorna todos os produtos
    public async Task<IEnumerable<Produto>> GetAll()
    {
        using var db = Connection();
        var sql = @"
            SELECT 
                id,
                titulo,
                descricao,
                preco_compra AS ""PrecoCompra"",
                preco_venda AS ""PrecoVenda"",
                estoque,
                codigo_barras AS ""CodigoBarras""
            FROM produtos
            ORDER BY id DESC";

        return await db.QueryAsync<Produto>(sql);
    }

    // Retorna um produto pelo ID
    public async Task<Produto?> GetById(int id)
    {
        using var db = Connection();
        var sql = @"
            SELECT 
                id,
                titulo,
                descricao,
                preco_compra AS ""PrecoCompra"",
                preco_venda AS ""PrecoVenda"",
                estoque,
                codigo_barras AS ""CodigoBarras""
            FROM produtos
            WHERE id = @Id";

        return await db.QueryFirstOrDefaultAsync<Produto>(sql, new { Id = id });
    }

    // Cria um novo produto e retorna o ID
    public async Task<int> Create(Produto produto)
    {
        using var db = Connection();
        var sql = @"
            INSERT INTO produtos 
                (titulo, descricao, preco_compra, preco_venda, estoque, codigo_barras)
            VALUES 
                (@Titulo, @Descricao, @PrecoCompra, @PrecoVenda, @Estoque, @CodigoBarras)
            RETURNING id;";

        return await db.ExecuteScalarAsync<int>(sql, produto);
    }

    // Atualiza um produto existente
    public async Task<bool> Update(Produto produto)
    {
        using var db = Connection();
        var sql = @"
            UPDATE produtos SET
                titulo = @Titulo,
                descricao = @Descricao,
                preco_compra = @PrecoCompra,
                preco_venda = @PrecoVenda,
                estoque = @Estoque,
                codigo_barras = @CodigoBarras
            WHERE id = @Id;";

        var rows = await db.ExecuteAsync(sql, produto);
        return rows > 0;
    }

    // Remove um produto pelo ID
    public async Task<bool> Delete(int id)
    {
        using var db = Connection();
        var sql = "DELETE FROM produtos WHERE id = @Id";
        var rows = await db.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }
}