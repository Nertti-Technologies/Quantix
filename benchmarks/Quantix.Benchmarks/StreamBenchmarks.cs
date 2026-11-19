using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using BaselineMediator = MediatR.IMediator;
using QuantixMediator = Quantix.IMediator;

namespace Quantix.Benchmarks;

/// <summary>
/// Stream enumeration throughput and allocations — Quantix versus the reference baseline
/// (plan L5-4). Each benchmark fully enumerates a stream of <see cref="ItemCount"/> items.
/// </summary>
[MemoryDiagnoser]
public class StreamBenchmarks
{
    private const int ItemCount = 100;

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

    /// <summary>The reference baseline: enumerate a stream to completion.</summary>
    [Benchmark(Baseline = true)]
    public async Task<int> Baseline_Stream()
    {
        int sum = 0;
        await foreach (int value in _baseline.CreateStream(new BaselineStream(ItemCount)).ConfigureAwait(false))
        {
            sum += value;
        }

        return sum;
    }

    /// <summary>Quantix: enumerate a stream to completion.</summary>
    [Benchmark]
    public async ValueTask<int> Quantix_Stream()
    {
        int sum = 0;
        await foreach (int value in _quantix.Stream(new BenchStream(ItemCount)).ConfigureAwait(false))
        {
            sum += value;
        }

        return sum;
    }
}
