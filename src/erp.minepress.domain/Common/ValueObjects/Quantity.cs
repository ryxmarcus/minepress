namespace erp.minepress.domain.Common.ValueObjects;

public record Quantity(decimal Value, string Unit)
{
    public static Quantity Zero(string unit = "PCS") => new(0, unit);

    public Quantity Add(Quantity other)
    {
        if (Unit != other.Unit)
            throw new InvalidOperationException("Cannot add quantities with different units.");
        return new Quantity(Value + other.Value, Unit);
    }
}
