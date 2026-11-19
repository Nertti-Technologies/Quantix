using Microsoft.Extensions.DependencyInjection;
using Quantix;
using Xunit;

namespace Quantix.IntegrationTests;

/// <summary>Integration tests for routing edge cases in the generated dispatcher.</summary>
public class DispatchTests
{
    [Fact]
    public async Task Send_routes_a_derived_message_to_its_own_handler()
    {
        ServiceProvider provider = new ServiceCollection()
            .AddQuantix()
            .BuildServiceProvider();

        IMediator mediator = provider.GetRequiredService<IMediator>();

        Assert.Equal("base", await mediator.Send(new Shipment()));
        Assert.Equal("express", await mediator.Send(new ExpressShipment()));
    }

    [Fact]
    public async Task Publish_reaches_a_handler_that_handles_several_notifications()
    {
        var log = new List<string>();
        ServiceProvider provider = new ServiceCollection()
            .AddSingleton(log)
            .AddQuantix()
            .BuildServiceProvider();

        IMediator mediator = provider.GetRequiredService<IMediator>();

        await mediator.Publish(new AccountOpened());
        await mediator.Publish(new AccountClosed());

        Assert.Equal(new[] { "opened", "closed" }, log);
    }

    [Fact]
    public async Task Verbs_reject_a_null_message()
    {
        ServiceProvider provider = new ServiceCollection()
            .AddQuantix()
            .BuildServiceProvider();

        IMediator mediator = provider.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<ArgumentNullException>(() => mediator.Publish(null!).AsTask());
        await Assert.ThrowsAsync<ArgumentNullException>(() => mediator.Send((IQuery<string>)null!).AsTask());
    }

    [Fact]
    public async Task Send_does_not_allocate_for_a_reference_type_result()
    {
        ServiceProvider provider = new ServiceCollection()
            .AddQuantix(options => options.HandlerLifetime = ServiceLifetime.Singleton)
            .BuildServiceProvider();

        IMediator mediator = provider.GetRequiredService<IMediator>();
        var shipment = new Shipment();

        // Warm up so JIT tiering and first-call resolution are not measured.
        for (int i = 0; i < 1_000; i++)
        {
            await mediator.Send(shipment);
        }

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1_000; i++)
        {
            await mediator.Send(shipment);
        }

        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Equal(0, allocated);
    }
}

/// <summary>A query a derived message extends.</summary>
public class Shipment : IQuery<string>;

/// <summary>A message deriving from <see cref="Shipment"/>, with its own handler.</summary>
public sealed class ExpressShipment : Shipment;

/// <summary>Handles <see cref="Shipment"/>.</summary>
public sealed class ShipmentHandler : IQueryHandler<Shipment, string>
{
    /// <inheritdoc />
    public ValueTask<string> Handle(Shipment query, CancellationToken ct) => new("base");
}

/// <summary>Handles <see cref="ExpressShipment"/>.</summary>
public sealed class ExpressShipmentHandler : IQueryHandler<ExpressShipment, string>
{
    /// <inheritdoc />
    public ValueTask<string> Handle(ExpressShipment query, CancellationToken ct) => new("express");
}

/// <summary>A notification handled by <see cref="AccountAuditHandler"/>.</summary>
public sealed record AccountOpened : INotification;

/// <summary>A notification handled by <see cref="AccountAuditHandler"/>.</summary>
public sealed record AccountClosed : INotification;

/// <summary>One handler class that handles two notifications.</summary>
public sealed class AccountAuditHandler(List<string> log)
    : INotificationHandler<AccountOpened>, INotificationHandler<AccountClosed>
{
    /// <inheritdoc />
    public ValueTask Handle(AccountOpened notification, CancellationToken ct)
    {
        log.Add("opened");
        return default;
    }

    /// <inheritdoc />
    public ValueTask Handle(AccountClosed notification, CancellationToken ct)
    {
        log.Add("closed");
        return default;
    }
}
