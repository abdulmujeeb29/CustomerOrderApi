namespace CustomerOrderApi.Repositories.Interface
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        ICustomerRepository Customers { get; }
        IOrderRepository Orders { get; }
        Task SaveChangesAsync();
    }
}
