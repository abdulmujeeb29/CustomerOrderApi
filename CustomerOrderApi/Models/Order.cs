namespace CustomerOrderApi.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime CreatedDate { get; set; }   
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

    }
}
