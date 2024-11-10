using CustomerOrderApi.Repositories.Interface;

namespace CustomerOrderApi.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public IUserRepository Users { get; }

        public UnitOfWork(AppDbContext context, IUserRepository userRepository)
        {
            _context = context;
            Users = userRepository;
        }

        public async Task SaveChangesAsync() =>
            await _context.SaveChangesAsync();
    }
}
