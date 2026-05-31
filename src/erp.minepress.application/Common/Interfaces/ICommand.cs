namespace erp.minepress.application.Common.Interfaces;

public interface ICommand<TResponse>
{
}

public interface ICommand : ICommand<Unit>
{
}

public record Unit
{
    public static readonly Unit Value = new();
}
