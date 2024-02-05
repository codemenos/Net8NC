namespace Solucao.Infrastructure.Shared.Interfaces;

/// <summary>
/// Define a interface para o padrão de Unidade de Trabalho (Unit of Work).
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Obtém um repositório para a entidade especificada.
    /// </summary>
    /// <typeparam name="TEntity">O tipo da entidade.</typeparam>
    /// <returns>O repositório correspondente à entidade.</returns>
    IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;

    /// <summary>
    /// Salva as alterações assincronamente no contexto de banco de dados.
    /// </summary>
    /// <returns>O número de entradas adicionadas, modificadas ou excluídas.</returns>
    Task<int> SaveChangesAsync();

    /// <summary>
    /// Inicia uma nova transação.
    /// </summary>
    void BeginTransaction();

    /// <summary>
    /// Confirma a transação assincronamente.
    /// </summary>
    /// <returns>O número de entradas afetadas.</returns>
    Task<int> CommitAsync();

    /// <summary>
    /// Desfaz a transação atual.
    /// </summary>
    void Rollback();
}