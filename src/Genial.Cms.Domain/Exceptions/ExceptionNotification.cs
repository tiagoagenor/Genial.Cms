using System.Text.Json.Serialization;
using Genial.Cms.Domain.Events;

namespace Genial.Cms.Domain.Exceptions;

public class ExceptionNotification : Event
{
    public string Code { get; }
    public string Message { get; }
    public string ParamName { get; }
    [JsonIgnore]
    public ExceptionType Type { get; }

    public ExceptionNotification(string code, string message, ExceptionType exceptionType, string paramName = null)
    {
        Code = code;
        Message = message;
        ParamName = paramName;
        Type = exceptionType;
    }
}
