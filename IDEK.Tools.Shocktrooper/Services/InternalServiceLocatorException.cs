namespace IDEK.Tools.ShocktroopUtils.Services;

public class InternalServiceLocatorException : Exception
{
    public InternalServiceLocatorException(Type serviceType, string? message = null) 
        : base(message ?? $"Internal service locator exception regarding type: {serviceType.FullName}")
    {
        ServiceType = serviceType;
    }

    public Type ServiceType { get; }
}