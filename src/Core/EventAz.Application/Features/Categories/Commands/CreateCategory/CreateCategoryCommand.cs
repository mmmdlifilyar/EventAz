using MediatR;

namespace EventAz.Application.Features.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(string Name) : IRequest<Guid>;
