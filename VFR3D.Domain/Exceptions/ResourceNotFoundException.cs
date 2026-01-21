namespace VFR3D.Domain.Exceptions;

/// <summary>
/// Exception thrown when a requested resource cannot be found.
/// </summary>
public class ResourceNotFoundException : Exception
{
    public ResourceNotFoundException()
    {
    }

    public ResourceNotFoundException(string message)
        : base(message)
    {
    }

    public ResourceNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
