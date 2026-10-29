using MediatR;

namespace EventAz.Application.Features.Categories.Queries.GetAllCategories;

public record GetAllCategoriesQuery : IRequest<List<CategoryDto>>;
