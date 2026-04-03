namespace VFR3D.Domain.Exceptions;

/// <summary>
/// Base exception for resource conflicts (duplicate entries, resources in use, etc.).
/// </summary>
public class ConflictException : DomainException
{
    public ConflictException(string errorCode, string userMessage, string? devMessage = null)
        : base(errorCode, userMessage, devMessage)
    {
    }
}

/// <summary>
/// Exception thrown when attempting to create an aircraft with a duplicate tail number.
/// </summary>
public class DuplicateTailNumberException : ConflictException
{
    public DuplicateTailNumberException(string tailNumber)
        : base(ErrorCodes.AircraftDuplicateTailNumber, $"An aircraft with tail number '{tailNumber}' already exists.")
    {
    }
}

/// <summary>
/// Exception thrown when attempting to create a profile with a duplicate name.
/// </summary>
public class DuplicateProfileNameException : ConflictException
{
    public DuplicateProfileNameException(string profileType, string profileName)
        : base(
            profileType == "PerformanceProfile" ? ErrorCodes.PerformanceProfileDuplicateName : ErrorCodes.WeightBalanceProfileDuplicateName,
            $"A {profileType} with name '{profileName}' already exists.")
    {
    }

    public DuplicateProfileNameException(string profileType, string profileName, string aircraftId)
        : base(
            profileType == "PerformanceProfile" ? ErrorCodes.PerformanceProfileDuplicateName : ErrorCodes.WeightBalanceProfileDuplicateName,
            $"A {profileType} with name '{profileName}' already exists for aircraft '{aircraftId}'.")
    {
    }
}

/// <summary>
/// Exception thrown when attempting to delete or modify a resource that is currently in use.
/// </summary>
public class ResourceInUseException : ConflictException
{
    public ResourceInUseException(string resourceType, string resourceId, string reason)
        : base(ErrorCodes.Conflict, $"{resourceType} '{resourceId}' cannot be modified: {reason}")
    {
    }
}
