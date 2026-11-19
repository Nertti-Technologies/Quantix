using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using BaselineMediator = MediatR.IMediator;
using QuantixMediator = Quantix.IMediator;

namespace Quantix.Benchmarks;

/// <summary>
/// Registration / cold-start cost — Quantix versus the reference baseline (plan L5-7). Each
/// benchmark registers the mediator and builds the container from scratch. Quantix registers a
/// generated, closed set of services with no assembly scan; the baseline scans the assembly.
/// </summary>
[MemoryDiagnoser]
public class ColdStartBenchmarks
{
    /// <summary>The reference baseline: scan the assembly, register, and build the container.</summary>
    [Benchmark(Baseline = true)]
    public BaselineMediator Baseline_Register()
        => new ServiceCollection()
            .AddMediatR(configuration => configuration.RegisterServicesFromAssembly(typeof(BaselineCommand).Assembly))
            .BuildServiceProvider()
            .GetRequiredService<BaselineMediator>();

    /// <summary>Quantix: register the generated, closed service set and build the container.</summary>
    [Benchmark]
    public QuantixMediator Quantix_Register()
        => new ServiceCollection()
            .AddQuantix()
            .BuildServiceProvider()
            .GetRequiredService<QuantixMediator>();
}
