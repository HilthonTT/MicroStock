using SharedKernel;
using SharedKernel.Auditing;

namespace Domain.Entities;

[Auditable]
public sealed class CompanyGroup : Entity
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Navigation properties
    /// </summary>
    public ICollection<CompanyGroupCompany> CompanyGroupCompanies { get; private set; } = [];

    public static CompanyGroup Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new CompanyGroup
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void UpdateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
