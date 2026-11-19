// Quantix attributes — all optional; discovery works with none of them (design section 4.5).

namespace Quantix;

/// <summary>
/// Sets the position of a pipeline behavior within the behavior chain.
/// </summary>
/// <remarks>
/// Behaviors run from the lowest <see cref="Order"/> (outermost) to the highest (innermost,
/// closest to the handler). The default order is <c>0</c>. Ties are broken by fully-qualified
/// type name so the generated chain is stable across builds.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class PipelineOrderAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="PipelineOrderAttribute"/> class.</summary>
    /// <param name="order">The behavior's position in the chain; lower values run first.</param>
    public PipelineOrderAttribute(int order)
    {
        Order = order;
    }

    /// <summary>Gets the behavior's position in the chain; lower values run first.</summary>
    public int Order { get; }
}

/// <summary>
/// Sets the sequential execution order of a notification handler.
/// </summary>
/// <remarks>
/// When a notification is published its handlers run one at a time in ascending
/// <see cref="Order"/>. The default order is <c>0</c>; ties are broken by fully-qualified
/// type name.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class NotificationOrderAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="NotificationOrderAttribute"/> class.</summary>
    /// <param name="order">The handler's execution order; lower values run first.</param>
    public NotificationOrderAttribute(int order)
    {
        Order = order;
    }

    /// <summary>Gets the handler's execution order; lower values run first.</summary>
    public int Order { get; }
}

/// <summary>
/// Excludes a handler or behavior from Quantix discovery.
/// </summary>
/// <remarks>
/// Useful for a test double or an alternative implementation that should not be wired into
/// the generated dispatcher.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class QuantixIgnoreAttribute : Attribute
{
}
