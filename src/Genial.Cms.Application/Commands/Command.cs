using FluentValidation.Results;
using MediatR;

namespace Genial.Cms.Application.Commands;

public abstract class Command<T> : IRequest<T>
{
	protected ValidationResult ValidationResult { get; set; }

	public ValidationResult GetValidationResult()
	{
		return ValidationResult;
	}

	public abstract bool IsValid();
}
