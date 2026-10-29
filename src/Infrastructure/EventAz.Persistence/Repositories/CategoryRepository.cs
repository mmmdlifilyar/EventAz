using EventAz.Application.Interfaces;
using EventAz.Domain.Entities;

namespace EventAz.Persistence.Repositories;

public class CategoryRepository : RepositoryBase<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context) { }
}
