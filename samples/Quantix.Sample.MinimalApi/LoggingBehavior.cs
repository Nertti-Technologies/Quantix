using Quantix;

namespace Quantix.Sample;

/// <summary>
/// An open-generic pipeline behavior that logs every command and query flowing through the
/// mediator. Quantix discovers it, closes it over each message, and registers each closed
/// instantiation — no marker interface and no manual registration required.
/// </summary>
/// <remarks>
/// It logs the message <em>type</em> only. Message fields and results carry user input (here, a
/// visitor's name), so writing them to logs would leak personal data.
/// </remarks>
/// <typeparam name="TRequest">The request type being wrapped.</typeparam>
/// <typeparam name="TResult">The result type the request produces.</typeparam>
public sealed class LoggingBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResult>> _logger;

    /// <summary>Creates the behavior with a logger resolved by dependency injection.</summary>
    /// <param name="logger">The logger.</param>
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResult>> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async ValueTask<TResult> Handle(
        TRequest request,
        RequestHandlerDelegate<TResult> next,
        CancellationToken ct)
    {
        string requestType = typeof(TRequest).Name;
        _logger.LogInformation("Quantix: handling {RequestType}.", requestType);
        TResult result = await next(ct).ConfigureAwait(false);
        _logger.LogInformation("Quantix: handled {RequestType}.", requestType);
        return result;
    }
}
