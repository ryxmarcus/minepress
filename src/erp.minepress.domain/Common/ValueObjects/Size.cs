namespace erp.minepress.domain.Common.ValueObjects;

public record Size(decimal WidthMm, decimal HeightMm)
{
    public decimal WidthInch => Math.Round(WidthMm / 25.4m, 2);
    public decimal HeightInch => Math.Round(HeightMm / 25.4m, 2);
    public decimal AreaSqMm => WidthMm * HeightMm;
}
