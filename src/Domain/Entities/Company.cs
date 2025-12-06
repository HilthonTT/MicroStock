using SharedKernel;
using SharedKernel.Auditing;

namespace Domain.Entities;

[Auditable]
public sealed class Company : Entity
{
    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Navigation Properties
    /// </summary>
    public ICollection<StockPrice> StockPrices { get; private set; } = new List<StockPrice>();

    /// <summary>
    /// Navigation Properties
    /// </summary>
    public ICollection<CompanyGroupCompany> CompanyGroupCompanies { get; private set; } = [];

    private Company()
    {
    }

    public static Company Create(string code, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Company
        {
            Id = Guid.CreateVersion7(),
            Code = code,
            Name = name,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Update(string code, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Code = code;
        Name = name;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
