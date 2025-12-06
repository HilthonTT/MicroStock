using SharedKernel;
using SharedKernel.Auditing;

namespace Domain.Entities;

[Auditable]
public sealed class StockPrice : Entity
{
    public Guid Id { get; private set; }

    public Guid CompanyId { get; private set; }

    public DateTime Date { get; private set; }

    public decimal OpenPrice { get; private set; }

    public decimal ClosePrice { get; private set; }

    public decimal HighPrice { get; private set; }

    public decimal LowPrice { get; private set; }

    public int Volume { get; private set; }

    /// <summary>
    /// Navigation property
    /// </summary>
    public Company Company { get; private set; } = default!;

    public static StockPrice Create(
       Guid companyId,
       DateTime date,
       decimal openPrice,
       decimal closePrice,
       decimal highPrice,
       decimal lowPrice,
       int volume)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(openPrice, nameof(openPrice));
        ArgumentOutOfRangeException.ThrowIfNegative(closePrice, nameof(closePrice));
        ArgumentOutOfRangeException.ThrowIfNegative(highPrice, nameof(highPrice));
        ArgumentOutOfRangeException.ThrowIfNegative(lowPrice, nameof(lowPrice));
        ArgumentOutOfRangeException.ThrowIfNegative(volume, nameof(volume));

        if (highPrice < lowPrice)
        {
            throw new ArgumentException("High price cannot be less than low price");
        }
        if (highPrice < openPrice || highPrice < closePrice)
        {
            throw new ArgumentException("High price must be the highest price");
        }
        if (lowPrice > openPrice || lowPrice > closePrice)
        {
            throw new ArgumentException("Low price must be the lowest price");
        }

        return new StockPrice
        {
            Id = Guid.CreateVersion7(),
            CompanyId = companyId,
            Date = date.Date,
            OpenPrice = openPrice,
            ClosePrice = closePrice,
            HighPrice = highPrice,
            LowPrice = lowPrice,
            Volume = volume
        };
    }

    public decimal GetPriceChange()
    {
        return ClosePrice - OpenPrice;
    }

    public decimal GetPriceChangePercentage()
    {
        return OpenPrice != 0 ? (ClosePrice - OpenPrice) / OpenPrice * 100 : 0; 
    }
}
