// ReSharper disable UnusedAutoPropertyAccessor.Global - Config
namespace Platform.Integrations.RpsListener.Configurations;

public sealed class RelayOptions
{
    public string RelayNamespace { get; init; }
    public string ConnectionName { get; init; }
    public string KeyName { get; init; }
    public string Key { get; init; }
}