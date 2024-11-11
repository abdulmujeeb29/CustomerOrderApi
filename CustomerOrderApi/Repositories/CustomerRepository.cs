using CustomerOrderApi.Data;
using CustomerOrderApi.Models;
using CustomerOrderApi.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
namespace CustomerOrderApi.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _context;
        public CustomerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync ()
        {
            return await _context.Customers.ToListAsync();
        }
        public async Task<Customer> GetByIdAsync(int id)
        {
            return await _context.Customers.FindAsync(id);
        }
        public async Task AddAsync(Customer customer)
        {
            await _context.Customers.AddAsync(customer);
        }
        public async Task UpdateAsync(Customer customer)
        {
            _context.Customers.Update(customer);
        }

        public async Task DeleteAsync(Customer customer)
        {
            _context.Customers.Remove(customer);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Customers.AnyAsync(e => e.Id == id);
        }
    }
}
