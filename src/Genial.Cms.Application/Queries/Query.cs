using FluentValidation.Results;
using MediatR;

namespace Genial.Cms.Application.Queries;

public abstract class Query<TResponse> : IRequest<TResponse>
{
    protected ValidationResult ValidationResult { get; set; }

    public ValidationResult GetValidationResult() => ValidationResult;

    public abstract bool IsValid();
}
