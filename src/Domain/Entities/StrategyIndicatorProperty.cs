using SharedKernel;
using SharedKernel.Auditing;

namespace Domain.Entities;

[Auditable]
public sealed class StrategyIndicatorProperty : Entity
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public int Value { get; private set; }

    public Guid IndicatorId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Navigation property
    /// </summary>
    public StrategyIndicator Indicator { get; private set; } = default!;

    public static StrategyIndicatorProperty Create(string name, int value, Guid indicatorId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new StrategyIndicatorProperty
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Value = value,
            IndicatorId = indicatorId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Update(string name, int value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Value = value;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
