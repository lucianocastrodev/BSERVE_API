using BSERVE_LIBRARY.Models;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Npgsql;

namespace BSERVE_API.Data;

public class UsuarioRepository
{
    private readonly string _connectionString;

    public UsuarioRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")!;
    }

    // 🔹 Obter usuário pelo email
    public async Task<Usuario?> ObterPorEmail(string email)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        var query = @"SELECT id, nome, email, senha_hash ""SenhaHash"", role, ativo 
                      FROM usuarios 
                      WHERE email = @Email";
        return await conn.QueryFirstOrDefaultAsync<Usuario>(query, new { Email = email });
    }

    // 🔹 Criar novo usuário
    public async Task Criar(Usuario usuario)
    {
        await using var conn = new NpgsqlConnection(_connectionString);

        // Gera UUID se não existir
        if (usuario.Id == Guid.Empty)
            usuario.Id = Guid.NewGuid();

        // Gera hash da senha (só se ainda não tiver)
        if (!string.IsNullOrEmpty(usuario.SenhaHash))
        {
            var hasher = new PasswordHasher<string>();
            usuario.SenhaHash = hasher.HashPassword(usuario.Email, usuario.SenhaHash);
        }

        var query = @"INSERT INTO usuarios (id, nome, email, senha_hash, role, ativo)
                      VALUES (@Id, @Nome, @Email, @SenhaHash, @Role, @Ativo)";

        await conn.ExecuteAsync(query, usuario);
    }

    // 🔹 Listar todos os usuários
    public async Task<List<Usuario>> ObterTodos()
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        var query = @"SELECT id, nome, email, senha_hash AS ""SenhaHash"", role, ativo 
                      FROM usuarios 
                      ORDER BY nome";
        var lista = await conn.QueryAsync<Usuario>(query);
        return lista.ToList();
    }

    // 🔹 Atualizar usuário
    public async Task<bool> Atualizar(Usuario usuario)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        var query = @"UPDATE usuarios
                      SET nome = @Nome,
                          email = @Email,
                          senha_hash = @SenhaHash,
                          role = @Role,
                          ativo = @Ativo
                      WHERE id = @Id";
        var linhasAfetadas = await conn.ExecuteAsync(query, usuario);
        return linhasAfetadas > 0;
    }

    // 🔹 Deletar usuário
    public async Task<bool> Deletar(Guid id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        var query = @"DELETE FROM usuarios WHERE id = @Id";
        var linhasAfetadas = await conn.ExecuteAsync(query, new { Id = id });
        return linhasAfetadas > 0;
    }
}