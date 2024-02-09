namespace Solucao.Application.Core.Behaviors;

public class CachingCommand
{
    public bool AtivarCaching { get; set; }
    public string Commando { get; set; }
    public int TempoDeCacheEmSegundos { get; set; } = 360;
    public int TempoDeCacheEmSegundosQueSeRenovamComOUso { get; set; } = 60;
}
