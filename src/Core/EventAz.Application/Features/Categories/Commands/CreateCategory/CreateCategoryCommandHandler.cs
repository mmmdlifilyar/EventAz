using EventAz.Application.Interfaces;
using EventAz.Domain.Entities;
using MediatR;

namespace EventAz.Application.Features.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Guid>
{
    private readonly ICategoryRepository _categoryRepository;
    public CreateCategoryCommandHandler(ICategoryRepository categoryRepository)
        => _categoryRepository = categoryRepository;

    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken ct)
    {
        var category = new Category { Name = request.Name };
        await _categoryRepository.AddAsync(category, ct);
        return category.Id;
    }
}
