using Xunit;

namespace Quantix.Generator.Tests;

/// <summary>Tests asserting which handlers and messages the generator discovers.</summary>
public class DiscoveryTests
{
    [Fact]
    public void Discovers_every_handler_interface_a_class_implements()
    {
        const string source = """
            using Quantix;
            using System.Threading;
            using System.Threading.Tasks;

            namespace App;

            public sealed record Created : INotification;
            public sealed record Deleted : INotification;

            public sealed class AuditHandler : INotificationHandler<Created>, INotificationHandler<Deleted>
            {
                public ValueTask Handle(Created notification, CancellationToken ct) => default;
                public ValueTask Handle(Deleted notification, CancellationToken ct) => default;
            }
            """;

        GeneratorResult result = GeneratorTestHarness.Run(source);
        string mediator = result.GetGeneratedFile("QuantixMediator.g.cs");

        Assert.False(result.HasDiagnostic("QTX0005"));
        Assert.Contains("notification is global::App.Created", mediator);
        Assert.Contains("notification is global::App.Deleted", mediator);
    }

    [Fact]
    public void Discovers_every_behavior_interface_a_class_implements()
    {
        const string source = """
            using Quantix;
            using System.Collections.Generic;
            using System.Threading;
            using System.Threading.Tasks;

            namespace App;

            public sealed record Lookup : IQuery<int>;
            public sealed record Feed : IStreamRequest<int>;

            public sealed class LookupHandler : IQueryHandler<Lookup, int>
            {
                public ValueTask<int> Handle(Lookup query, CancellationToken ct) => new(1);
            }

            public sealed class FeedHandler : IStreamRequestHandler<Feed, int>
            {
                public IAsyncEnumerable<int> Handle(Feed request, CancellationToken ct) => throw null!;
            }

            public sealed class AuditBehavior : IPipelineBehavior<Lookup, int>, IStreamPipelineBehavior<Feed, int>
            {
                public ValueTask<int> Handle(Lookup request, RequestHandlerDelegate<int> next, CancellationToken ct) => next(ct);
                public IAsyncEnumerable<int> Handle(Feed request, StreamHandlerDelegate<int> next, CancellationToken ct) => next(ct);
            }
            """;

        GeneratorResult result = GeneratorTestHarness.Run(source);
        string mediator = result.GetGeneratedFile("QuantixMediator.g.cs");

        Assert.Empty(result.Diagnostics);
        Assert.Contains("global::Quantix.IPipelineBehavior<global::App.Lookup, int> behavior0", mediator);
        Assert.Contains("global::Quantix.IStreamPipelineBehavior<global::App.Feed, int> behavior0", mediator);
    }

    [Fact]
    public void Keeps_the_behavior_role_of_a_class_that_is_also_a_handler()
    {
        const string source = """
            using Quantix;
            using System.Threading;
            using System.Threading.Tasks;

            namespace App;

            public sealed record Ping : ICommand;

            public sealed class PingHandlerAndBehavior : ICommandHandler<Ping>, ICommandPipelineBehavior<Ping>
            {
                public ValueTask Handle(Ping command, CancellationToken ct) => default;
                public ValueTask Handle(Ping command, CommandHandlerDelegate next, CancellationToken ct) => next(ct);
            }
            """;

        GeneratorResult result = GeneratorTestHarness.Run(source);
        string mediator = result.GetGeneratedFile("QuantixMediator.g.cs");

        Assert.Empty(result.Diagnostics);
        Assert.Contains("global::Quantix.ICommandPipelineBehavior<global::App.Ping> behavior0", mediator);
    }

    [Fact]
    public void Excludes_a_handler_marked_QuantixIgnore()
    {
        const string source = """
            using Quantix;
            using System.Threading;
            using System.Threading.Tasks;

            namespace App;

            public sealed record Charge : ICommand<int>;

            public sealed class ChargeHandler : ICommandHandler<Charge, int>
            {
                public ValueTask<int> Handle(Charge command, CancellationToken ct) => new(1);
            }

            [QuantixIgnore]
            public sealed class FakeChargeHandler : ICommandHandler<Charge, int>
            {
                public ValueTask<int> Handle(Charge command, CancellationToken ct) => new(2);
            }
            """;

        GeneratorResult result = GeneratorTestHarness.Run(source);

        Assert.Empty(result.Diagnostics);
        Assert.DoesNotContain("FakeChargeHandler", result.AllGeneratedSource);
    }

    [Fact]
    public void Excludes_a_behavior_marked_QuantixIgnore()
    {
        const string source = """
            using Quantix;
            using System.Threading;
            using System.Threading.Tasks;

            namespace App;

            public sealed record Charge : ICommand<int>;

            public sealed class ChargeHandler : ICommandHandler<Charge, int>
            {
                public ValueTask<int> Handle(Charge command, CancellationToken ct) => new(1);
            }

            [QuantixIgnore]
            public sealed class TraceBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
            {
                public ValueTask<TResult> Handle(TRequest request, RequestHandlerDelegate<TResult> next, CancellationToken ct)
                    => next(ct);
            }
            """;

        GeneratorResult result = GeneratorTestHarness.Run(source);

        Assert.DoesNotContain("TraceBehavior", result.AllGeneratedSource);
    }

    [Theory]
    [InlineData("Quantix.QuantixHandlerAttribute")]
    [InlineData("Quantix.QuantixModuleAttribute")]
    [InlineData("Quantix.ScanQuantixHandlersFromAttribute")]
    public void Abstractions_do_not_expose_attributes_the_generator_does_not_honour(string typeName)
    {
        // These attributes were removed (ADR 0001): the generator never read them, so offering
        // them promised behaviour that did not exist.
        Assert.Null(typeof(QuantixIgnoreAttribute).Assembly.GetType(typeName));
    }

    [Fact]
    public void Discovers_a_generic_message_created_with_target_typed_new()
    {
        const string source = """
            using Quantix;
            using System.Threading;
            using System.Threading.Tasks;

            namespace App;

            public sealed record GetById<TEntity>(int Id) : IQuery<TEntity?>;

            public sealed class GetByIdHandler<TEntity> : IQueryHandler<GetById<TEntity>, TEntity?>
            {
                public ValueTask<TEntity?> Handle(GetById<TEntity> query, CancellationToken ct) => new(default(TEntity));
            }

            public sealed class Customer;

            public static class Caller
            {
                public static GetById<Customer> Create()
                {
                    GetById<Customer> query = new(1);
                    return query;
                }
            }
            """;

        GeneratorResult result = GeneratorTestHarness.Run(source);

        Assert.False(result.HasDiagnostic("QTX0011"));
        Assert.Contains("global::App.GetByIdHandler<global::App.Customer>", result.AllGeneratedSource);
    }

    [Fact]
    public void Tests_a_derived_message_before_its_base_in_the_type_pattern_chain()
    {
        // 'Base' sorts before 'Derived', so name order alone would let the base branch's
        // 'is' test claim a Derived instance and route it to the wrong handler.
        const string source = """
            using Quantix;
            using System.Threading;
            using System.Threading.Tasks;

            namespace App;

            public class Base : ICommand<int>;
            public sealed class Derived : Base;

            public sealed class BaseHandler : ICommandHandler<Base, int>
            {
                public ValueTask<int> Handle(Base command, CancellationToken ct) => new(1);
            }

            public sealed class DerivedHandler : ICommandHandler<Derived, int>
            {
                public ValueTask<int> Handle(Derived command, CancellationToken ct) => new(2);
            }
            """;

        GeneratorResult result = GeneratorTestHarness.Run(source);
        string mediator = result.GetGeneratedFile("QuantixMediator.g.cs");

        Assert.Empty(result.Diagnostics);
        Assert.True(
            mediator.IndexOf("command is global::App.Derived", StringComparison.Ordinal)
                < mediator.IndexOf("command is global::App.Base", StringComparison.Ordinal),
            "The derived message must be tested before its base message.");
    }
}
