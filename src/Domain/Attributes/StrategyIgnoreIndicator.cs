namespace Domain.Attributes;

/// <summary>
/// Marks an indicator that is not available in strategy and simulation mode.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field)]
public sealed class StrategyIgnoreIndicator : Attribute;

