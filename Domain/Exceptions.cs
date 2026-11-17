namespace InventoryLab3.Domain;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}

public class InsufficientStockException : ConflictException
{
    public InsufficientStockException(int productId, int warehouseId, int available, int requested)
        : base($"Недостатньо товару {productId} на складі {warehouseId}: є {available}, потрібно {requested}")
    {
    }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}
