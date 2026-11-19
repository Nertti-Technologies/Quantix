using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using BaselineMediator = MediatR.IMediator;
using QuantixMediator = Quantix.IMediator;

namespace Quantix.Benchmarks;

/// <summary>
/// Publish throughput and allocations — Quantix versus the reference baseline (plan L5-3).
/// Both notifications fan out to four handlers, each awaited in turn.
/// </summary>
[MemoryDiagnoser]
public class PublishBenchmarks
{
    private QuantixMediator _quantix = null!;
    private BaselineMediator _baseline = null!;

    /// <summary>Builds both dependency-injection containers once before the benchmarks run.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _quantix = new ServiceCollection()
            .AddQuantix()
            .BuildServiceProvider()
            .GetRequiredService<QuantixMediator>();

        _baseline = new ServiceCollection()
            .AddMediatR(configuration => configuration.RegisterServicesFromAssembly(typeof(BaselineCommand).Assembly))
            .BuildServiceProvider()
            .GetRequiredService<BaselineMediator>();
    }

    /// <summary>The reference baseline: publish a notification to four handlers.</summary>
    [Benchmark(Baseline = true)]
    public Task Baseline_Publish() => _baseline.Publish(new BaselineNotification());

    /// <summary>Quantix: publish a notification to four handlers.</summary>
    [Benchmark]
    public ValueTask Quantix_Publish() => _quantix.Publish(new BenchNotification());
}
