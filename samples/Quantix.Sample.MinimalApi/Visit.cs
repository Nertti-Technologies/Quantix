using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Quantix;

namespace Quantix.Sample;

/// <summary>
/// A command that records a page visit and returns that page's running visit count, or
/// <c>0</c> when the visit was not recorded because the page limit is reached.
/// </summary>
/// <param name="Page">The page that was visited.</param>
public sealed record RecordVisit(string Page) : ICommand<int>;

/// <summary>
/// Handles <see cref="RecordVisit"/> — keeps an in-memory counter per page. The number of
/// distinct pages is capped so that requests for ever-new page names cannot grow memory
/// without bound; once the cap is reached, visits to pages not yet seen are not recorded.
/// </summary>
public sealed class RecordVisitHandler : ICommandHandler<RecordVisit, int>
{
    /// <summary>The most distinct pages the sample tracks (a sample-sized limit, not a tuned figure).</summary>
    public const int MaxPages = 1_000;

    private static readonly ConcurrentDictionary<string, StrongBox<int>> Counts = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public ValueTask<int> Handle(RecordVisit command, CancellationToken ct)
    {
        // The count check and the insert are not atomic, so concurrent first visits can
        // overshoot MaxPages by a few entries; the bound still holds to within the request
        // concurrency, which is all the sample needs.
        if (!Counts.TryGetValue(command.Page, out StrongBox<int>? box))
        {
            if (Counts.Count >= MaxPages)
            {
                return new(0);
            }

            box = Counts.GetOrAdd(command.Page, static _ => new StrongBox<int>());
        }

        return new(Interlocked.Increment(ref box.Value));
    }
}

/// <summary>
/// A constrained open-generic behavior that audits commands. The
/// <c>where TCommand : ICommand&lt;TResult&gt;</c> constraint scopes it to commands: Quantix
/// closes it over every command at compile time and never over a query, so the unrelated
/// <see cref="GetGreeting"/> query is left untouched. This is constraint-aware scoping
/// (design D11) — no marker interface and no manual registration. Like
/// <see cref="LoggingBehavior{TRequest, TResult}"/>, it records the command type, never its fields.
/// </summary>
/// <typeparam name="TCommand">The command being audited.</typeparam>
/// <typeparam name="TResult">The result the command produces.</typeparam>
[PipelineOrder(20)]
public sealed class CommandAuditBehavior<TCommand, TResult> : IPipelineBehavior<TCommand, TResult>
    where TCommand : ICommand<TResult>
{
    private readonly ILogger<CommandAuditBehavior<TCommand, TResult>> _logger;

    /// <summary>Creates the behavior with a logger resolved by dependency injection.</summary>
    /// <param name="logger">The logger.</param>
    public CommandAuditBehavior(ILogger<CommandAuditBehavior<TCommand, TResult>> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async ValueTask<TResult> Handle(
        TCommand command,
        RequestHandlerDelegate<TResult> next,
        CancellationToken ct)
    {
        string commandType = typeof(TCommand).Name;
        _logger.LogInformation("Quantix: auditing command {CommandType}.", commandType);
        TResult result = await next(ct).ConfigureAwait(false);
        _logger.LogInformation("Quantix: audited command {CommandType}.", commandType);
        return result;
    }
}
