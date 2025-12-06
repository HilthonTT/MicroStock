using SharedKernel;
using SharedKernel.Auditing;

namespace Domain.Entities;

[Auditable]
public sealed class InvestmentStrategy : Entity
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public int? SignalDaysPerPeriod { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    public DateTime? DeletedAtUtc { get; private set; }

    /// <summary>
    /// Navigation Properties
    /// </summary>
    public User User { get; private set; } = default!;

    /// <summary>
    /// Navigation Properties
    /// </summary>
    public ICollection<StrategyIndicator> StrategyIndicators { get; private set; } = [];

    private InvestmentStrategy()
    {
    }

    public static InvestmentStrategy Create(Guid userId, string name, int? signalDaysPerPeriod = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (signalDaysPerPeriod.HasValue && signalDaysPerPeriod.Value <= 0)
        {
            throw new ArgumentException("Signal days per period must be positive", nameof(signalDaysPerPeriod));
        }

        return new InvestmentStrategy
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            Name = name,
            SignalDaysPerPeriod = signalDaysPerPeriod,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Update(string name, int? signalDaysPerPeriod)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (signalDaysPerPeriod.HasValue && signalDaysPerPeriod.Value <= 0)
        {
            throw new ArgumentException("Signal days per period must be positive", nameof(signalDaysPerPeriod));
        }

        Name = name;
        SignalDaysPerPeriod = signalDaysPerPeriod;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Delete()
    {
        IsDeleted = true;
        DeletedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAtUtc = null;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
