using foll_care.Shared.Domain.Repositories;
using foll_care.Shared.Infrastructure.Persistence.EFC.Configuration;

namespace foll_care.Shared.Infrastructure.Persistence.EFC.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context) => _context = context;

    public async Task CompleteAsync() => await _context.SaveChangesAsync();
    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        await action();
        await transaction.CommitAsync();
    }
}
