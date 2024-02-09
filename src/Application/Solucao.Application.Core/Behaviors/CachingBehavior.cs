namespace Solucao.Application.Core.Behaviors;

using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Solucao.Infrastructure.Shared.Common;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IDistributedCache _cache;
    private readonly CachingBehaviorConfiguration _config;

    public CachingBehavior(IDistributedCache cache, IOptions<CachingBehaviorConfiguration> config)
    {
        _cache = cache;
        _config = config.Value;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestType = request.GetType().Name;
        var comandoDeCaching = _config.CachingCommands.FirstOrDefault(c => c.Commando.Equals(requestType, StringComparison.OrdinalIgnoreCase));

        if (comandoDeCaching == null || !comandoDeCaching.AtivarCaching || !_config.AtivarCaching)
        {
            return await next();
        }

        var cacheKey = $"{requestType}_{request.GetHashCode()}";

        var cachedResponse = await GetFromCacheAsync<TResponse>(cacheKey);

        if (cachedResponse != null)
        {
            return cachedResponse;
        }

        var response = await next();

        if (response is ResultadoOperacao<object> resultadoResponse && !resultadoResponse.ComSucesso)
        {
            return response;
        }

        await CacheResponseAsync(cacheKey, response, comandoDeCaching);

        return response;
    }

    private async Task<T> GetFromCacheAsync<T>(string key)
    {
        var cachedData = await _cache.GetStringAsync(key);
        if (!string.IsNullOrEmpty(cachedData))
        {
            return JsonConvert.DeserializeObject<T>(cachedData);
        }
        return default;
    }

    private async Task CacheResponseAsync<T>(string key, T response, CachingCommand config)
    {
        var cacheEntryOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(config.TempoDeCacheEmSegundos),
            SlidingExpiration = TimeSpan.FromSeconds(config.TempoDeCacheEmSegundosQueSeRenovamComOUso)
        };

        var serializedData = JsonConvert.SerializeObject(response);

        await _cache.SetStringAsync(key, serializedData, cacheEntryOptions);
    }
}
