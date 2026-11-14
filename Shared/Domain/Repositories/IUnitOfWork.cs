namespace foll_care.Shared.Domain.Repositories;

public interface IUnitOfWork
{
    Task CompleteAsync();
    Task ExecuteInTransactionAsync(Func<Task> action);
}
