using CustomerOrderApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CustomerOrderApi.Models;
using CustomerOrderApi.Repositories.Interface;

namespace CustomerOrderApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController:ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrdersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _unitOfWork.Orders.GetAllAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return order;
        }

        [HttpPost]
        public async Task<ActionResult<Customer>> CreateOrder(Order order)
        {
            _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction("GetOrder", new { id = order.Id }, order);
        }

    }
}
