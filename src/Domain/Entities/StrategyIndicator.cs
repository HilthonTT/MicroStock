using Domain.Enums;
using SharedKernel;
using SharedKernel.Auditing;

namespace Domain.Entities;

[Auditable]
public sealed class StrategyIndicator : Entity
{
    public Guid Id { get; private set; }

    public IndicatorType Type { get; private set; }

    public Guid StrategyId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Navigation properties
    /// </summary>
    public InvestmentStrategy Strategy { get; private set; } = default!;

    /// <summary>
    /// Navigation properties
    /// </summary>
    public ICollection<StrategyIndicatorProperty> Properties { get; private set; } = [];

    public static StrategyIndicator Create(IndicatorType type, Guid strategyId)
    {
        if (!Enum.IsDefined(type))
        {
            throw new ArgumentException("Invalid indicator type", nameof(type));
        }

        return new StrategyIndicator
        {
            Id = Guid.CreateVersion7(),
            Type = type,
            StrategyId = strategyId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void UpdateType(IndicatorType type)
    {
        if (!Enum.IsDefined(type))
        {
            throw new ArgumentException("Invalid indicator type", nameof(type));
        }

        Type = type;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
