namespace CustomerOrderApi.Repositories.Interface
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        Task SaveChangesAsync();
    }
}
