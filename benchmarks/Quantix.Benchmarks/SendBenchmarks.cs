using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using BaselineMediator = MediatR.IMediator;
using QuantixMediator = Quantix.IMediator;

namespace Quantix.Benchmarks;

/// <summary>
/// Send throughput and allocations — Quantix versus the reference baseline (plan L5-2, L5-5).
/// Both mediators dispatch a behavior-free command and query, so this isolates raw dispatch.
/// </summary>
[MemoryDiagnoser]
public class SendBenchmarks
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

    /// <summary>The reference baseline: send a command.</summary>
    [Benchmark(Baseline = true)]
    public Task<int> Baseline_SendCommand() => _baseline.Send(new BaselineCommand(1));

    /// <summary>Quantix: send a command.</summary>
    [Benchmark]
    public ValueTask<int> Quantix_SendCommand() => _quantix.Send(new BenchCommand(1));

    /// <summary>The reference baseline: send a query.</summary>
    [Benchmark]
    public Task<int> Baseline_SendQuery() => _baseline.Send(new BaselineQuery(1));

    /// <summary>Quantix: send a query.</summary>
    [Benchmark]
    public ValueTask<int> Quantix_SendQuery() => _quantix.Send(new BenchQuery(1));
}
