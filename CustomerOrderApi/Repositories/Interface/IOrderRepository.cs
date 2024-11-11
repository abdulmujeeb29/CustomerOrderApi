using CustomerOrderApi.Models;

namespace CustomerOrderApi.Repositories.Interface
{
    public interface IOrderRepository
    {
        Task <IEnumerable<Order>> GetAllAsync ();

        Task <Order> GetByIdAsync(int id);
        

        Task AddAsync(Order order);
    }
}
