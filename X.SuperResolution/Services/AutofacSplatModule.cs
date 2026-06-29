// -- Function: AutofacSplatModule.cs
// --- Project: X.SuperResolution
// ---- Remark:
// ---- Author: Lucifer
// ------ Date: 2026-01-24 03:01:43

using Microsoft.Extensions.DependencyInjection;
using Splat;
using Splat.Builder;

namespace X.SuperResolution.Services;

public class AutofacSplatModule : IModule
{
    /// <inheritdoc />
    public void Configure(IMutableDependencyResolver resolver)
    {
        resolver.Register<IServiceCollection>(() => new ServiceCollection());
    }
}