using CustomerOrderApi.Repositories.Interface;

namespace CustomerOrderApi.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public IUserRepository Users { get; }
        public IOrderRepository Orders { get; }
        public ICustomerRepository Customers { get; }

        public UnitOfWork(AppDbContext context, IUserRepository userRepository, IOrderRepository orderRepository, ICustomerRepository customerRepository)
        {
            _context = context;
            Users = userRepository;
            Orders = orderRepository;
            Customers = customerRepository;
        }

        public async Task SaveChangesAsync() =>
            await _context.SaveChangesAsync();
    }
}
