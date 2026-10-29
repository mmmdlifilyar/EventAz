using EventAz.Application.Interfaces;
using MediatR;

namespace EventAz.Application.Features.Categories.Queries.GetAllCategories;

public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, List<CategoryDto>>
{
    private readonly ICategoryRepository _categoryRepository;
    public GetAllCategoriesQueryHandler(ICategoryRepository categoryRepository)
        => _categoryRepository = categoryRepository;

    public async Task<List<CategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken ct)
    {
        var categories = await _categoryRepository.GetAllAsync(ct);
        return categories.Select(c => new CategoryDto(c.Id, c.Name)).ToList();
    }
}
