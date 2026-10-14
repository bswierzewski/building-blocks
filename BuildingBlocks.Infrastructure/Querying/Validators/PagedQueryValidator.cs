using FluentValidation;

namespace BuildingBlocks.Infrastructure.Querying.Validators;

/// <summary>Validates the paging of a list request; derive from it to add the rules of the filters of the list.</summary>
public abstract class PagedQueryValidator<T> : AbstractValidator<T> where T : PagedQuery
{
    protected PagedQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1).WithName("Strona");
        RuleFor(x => x.PageSize).InclusiveBetween(1, PagedQuery.MaxPageSize).WithName("Rozmiar strony");
    }
}
