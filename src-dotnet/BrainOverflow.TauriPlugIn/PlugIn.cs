namespace BrainOverflow.TauriPlugIn;

using Microsoft.Extensions.DependencyInjection;
using TauriDotNetBridge.Contracts;

public class PlugIn : IPlugIn
{
    public void Initialize(IServiceCollection services)
    {
        services.AddSingleton<SnippetsController>();
        services.AddSingleton<Store>();
        services.AddSingleton<IHostedService, GitObserver>();
    }
}
