// The reference-baseline messages and handlers the benchmark suite dispatches. The baseline
// library uses one IRequest marker for both commands and queries; the pairs below mirror the Quantix
// workload one-for-one so the comparison rows line up.
//
// Every baseline-library interface is fully qualified: this project's namespace is Quantix.Benchmarks,
// so the enclosing Quantix namespace would otherwise shadow its same-named interfaces
// (INotification, IStreamRequest, and so on) and bind these types to Quantix by mistake.

using System.Runtime.CompilerServices;

namespace Quantix.Benchmarks;

/// <summary>The reference baseline for <see cref="BenchCommand"/>.</summary>
/// <param name="Value">The value the handler echoes back.</param>
public sealed record BaselineCommand(int Value) : MediatR.IRequest<int>;

/// <summary>Handles <see cref="BaselineCommand"/>.</summary>
public sealed class BaselineCommandHandler : MediatR.IRequestHandler<BaselineCommand, int>
{
    /// <inheritdoc />
    public Task<int> Handle(BaselineCommand request, CancellationToken cancellationToken)
        => Task.FromResult(request.Value);
}

/// <summary>The reference baseline for <see cref="BenchQuery"/>.</summary>
/// <param name="Value">The value the handler echoes back.</param>
public sealed record BaselineQuery(int Value) : MediatR.IRequest<int>;

/// <summary>Handles <see cref="BaselineQuery"/>.</summary>
public sealed class BaselineQueryHandler : MediatR.IRequestHandler<BaselineQuery, int>
{
    /// <inheritdoc />
    public Task<int> Handle(BaselineQuery request, CancellationToken cancellationToken)
        => Task.FromResult(request.Value);
}

/// <summary>The reference baseline for <see cref="BenchNotification"/>.</summary>
public sealed record BaselineNotification : MediatR.INotification;

/// <summary>The first handler of <see cref="BaselineNotification"/>.</summary>
public sealed class BaselineNotificationHandlerA : MediatR.INotificationHandler<BaselineNotification>
{
    /// <inheritdoc />
    public Task Handle(BaselineNotification notification, CancellationToken cancellationToken)
        => Task.CompletedTask;
}

/// <summary>The second handler of <see cref="BaselineNotification"/>.</summary>
public sealed class BaselineNotificationHandlerB : MediatR.INotificationHandler<BaselineNotification>
{
    /// <inheritdoc />
    public Task Handle(BaselineNotification notification, CancellationToken cancellationToken)
        => Task.CompletedTask;
}

/// <summary>The third handler of <see cref="BaselineNotification"/>.</summary>
public sealed class BaselineNotificationHandlerC : MediatR.INotificationHandler<BaselineNotification>
{
    /// <inheritdoc />
    public Task Handle(BaselineNotification notification, CancellationToken cancellationToken)
        => Task.CompletedTask;
}

/// <summary>The fourth handler of <see cref="BaselineNotification"/>.</summary>
public sealed class BaselineNotificationHandlerD : MediatR.INotificationHandler<BaselineNotification>
{
    /// <inheritdoc />
    public Task Handle(BaselineNotification notification, CancellationToken cancellationToken)
        => Task.CompletedTask;
}

/// <summary>The reference baseline for <see cref="BenchStream"/>.</summary>
/// <param name="Count">The number of integers to yield.</param>
public sealed record BaselineStream(int Count) : MediatR.IStreamRequest<int>;

/// <summary>Handles <see cref="BaselineStream"/>.</summary>
public sealed class BaselineStreamHandler : MediatR.IStreamRequestHandler<BaselineStream, int>
{
    /// <inheritdoc />
    public async IAsyncEnumerable<int> Handle(
        BaselineStream request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.CompletedTask.ConfigureAwait(false);
        for (int i = 0; i < request.Count; i++)
        {
            yield return i;
        }
    }
}
