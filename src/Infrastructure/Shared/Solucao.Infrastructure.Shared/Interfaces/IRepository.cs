namespace Solucao.Infrastructure.Shared.Interfaces;

/// <summary>
/// Interface genérica para repositório responsável pelas operações CRUD básicas em entidades.
/// </summary>
/// <typeparam name="TEntity">Tipo da entidade para a qual o repositório fornece operações CRUD.</typeparam>
public interface IRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// Obtém uma entidade por seu identificador único de forma assíncrona.
    /// </summary>
    /// <param name="id">Identificador único da entidade a ser obtida.</param>
    /// <param name="cancellationToken">Token de cancelamento (opcional).</param>
    /// <returns>A entidade correspondente ao identificador fornecido, se encontrada.</returns>
    Task<TEntity> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona uma entidade ao repositório de forma assíncrona.
    /// </summary>
    /// <param name="entity">Entidade a ser adicionada.</param>
    /// <param name="cancellationToken">Token de cancelamento (opcional).</param>
    /// <returns>Uma tarefa que representa a operação assíncrona de adição.</returns>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adiciona uma entidade ao repositório de forma assíncrona.
    /// </summary>
    /// <param name="entity">Entidade a ser adicionada.</param>
    /// <returns>Uma tarefa que representa a operação assíncrona de adição.</returns>
    Task AddAsync(TEntity entity);

    /// <summary>
    /// Atualiza uma entidade no repositório.
    /// </summary>
    /// <param name="entity">Entidade a ser atualizada.</param>
    void Update(TEntity entity);

    /// <summary>
    /// Remove uma entidade do repositório.
    /// </summary>
    /// <param name="entity">Entidade a ser removida.</param>
    void Remove(TEntity entity);
}