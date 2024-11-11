using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CustomerOrderApi.Data;
using CustomerOrderApi.Models;
using Microsoft.AspNetCore.Authorization;
using CustomerOrderApi.DTOs;
using CustomerOrderApi.Repositories.Interface;


namespace CustomerOrderApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/Customers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
        {
            var customers = await _unitOfWork.Customers.GetAllAsync();

            // Map Customer to CustomerDto
            var customerDtos = customers.Select(c => new CustomerDto
            {
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                Address = c.Address
            }).ToList();

            return Ok(customerDtos);
        }

        // GET: api/Customers/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            // Map Customer to CustomerDto
            var customerDto = new CustomerDto
            {
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                Address = customer.Address
            };

            return Ok(customerDto);
        }

        // PUT: api/Customers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomer(int id, CustomerDto customerDto)
        {

            if (! await CustomerExists(id))
            {
                return NotFound();
            }

            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            // Map CustomerDto to Customer
            customer.FirstName = customerDto.FirstName;
            customer.LastName = customerDto.LastName;
            customer.Email = customerDto.Email;
            customer.Address = customerDto.Address;

            try
            {
                _unitOfWork.Customers.UpdateAsync(customer);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict();
            }

            return NoContent();
        }

        // POST: api/Customers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Customer>> PostCustomer(CustomerDto customerDto)
        {
            // Map CustomerDto to Customer
            var customer = new Customer
            {
                FirstName = customerDto.FirstName,
                LastName = customerDto.LastName,
                Email = customerDto.Email,
                Address = customerDto.Address
            };

            await _unitOfWork.Customers.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction("GetCustomer", new { id = customer.Id }, customerDto);
        }

        // DELETE: api/Customers/5
        [HttpDelete("{id}")]
        [Authorize] 
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            await _unitOfWork.Customers.DeleteAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> CustomerExists(int id)
        {
            return await _unitOfWork.Customers.ExistsAsync(id);
        }
    }
}
