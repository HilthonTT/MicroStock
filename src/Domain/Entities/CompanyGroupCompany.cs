using SharedKernel.Auditing;

namespace Domain.Entities;

[Auditable]
public sealed class CompanyGroupCompany
{
    public Guid CompanyId { get; private set; }

    public Guid CompanyGroupId { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Navigation properties
    /// </summary>
    public Company Company { get; private set; } = default!;

    /// <summary>
    /// Navigation properties
    /// </summary>
    public CompanyGroup CompanyGroup { get; private set; } = default!;

    private CompanyGroupCompany()
    {
    }

    public static CompanyGroupCompany Create(Guid companyId, Guid companyGroupId)
    {
        return new CompanyGroupCompany
        {
            CompanyId = companyId,
            CompanyGroupId = companyGroupId,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
