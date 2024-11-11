using CustomerOrderApi.Data;
using CustomerOrderApi.Models;
using CustomerOrderApi.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
namespace CustomerOrderApi.Repositories
{
    public class OrderRepository:IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders.ToListAsync();
        }
        public async Task<Order> GetByIdAsync(int id)
        {
            return await _context.Orders.FindAsync(id);
        }
        public async Task AddAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
        }

    }
}
