namespace Domain.Exceptions;

/// <summary>
/// Exception thrown when a business rule is violated in the domain
/// </summary>
public class BusinessRuleViolationException : DomainException
{
    public BusinessRuleViolationException(string message) : base(message)
    {
    }

    public BusinessRuleViolationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
} 