using EventAz.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventAz.Persistence.Repositories;

public class RepositoryBase<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    public RepositoryBase(ApplicationDbContext context) => _context = context;

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _context.Set<T>().FindAsync([id], ct);

    public async Task<List<T>> GetAllAsync(CancellationToken ct = default)
        => await _context.Set<T>().ToListAsync(ct);

    public async Task AddAsync(T entity, CancellationToken ct = default)
    {
        await _context.Set<T>().AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(T entity, CancellationToken ct = default)
    {
        _context.Set<T>().Remove(entity);
        await _context.SaveChangesAsync(ct);
    }
}
