namespace erp.minepress.domain.Common.ValueObjects;

public record Cost(decimal Amount, string Currency = "INR")
{
    public static Cost Zero => new(0);

    public Cost Add(Cost other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add costs with different currencies.");
        return new Cost(Amount + other.Amount, Currency);
    }

    public Cost Multiply(decimal factor) => new(Amount * factor, Currency);
}
