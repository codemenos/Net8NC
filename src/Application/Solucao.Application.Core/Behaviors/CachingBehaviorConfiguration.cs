namespace Solucao.Application.Core.Behaviors;

public class CachingBehaviorConfiguration
{
    public bool AtivarCaching { get; set; }
    public List<CachingCommand> CachingCommands { get; set; }
}