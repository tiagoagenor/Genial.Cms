using System;

namespace Genial.Cms.Domain.Exceptions;

public class DomainException : Exception
{
    public string Code { get; }
    public ExceptionType ExceptionType { get; }

    public DomainException(string code, string message, ExceptionType exceptionType) : base(message)
    {
        Code = code;
        ExceptionType = exceptionType;
    }

    public DomainException(string message) : base(message)
    {
    }
}
