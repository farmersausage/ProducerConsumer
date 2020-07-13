namespace ProducerConsumer.Models
{
    public readonly struct StoreOrder
    {
        public bool Success { get; }
        public string StoreId { get; }
        public decimal Price { get; }
        public int Quantity { get; }

        public StoreOrder(bool success, string storeId, decimal price, int quantity)
        {
            Success = success;
            StoreId = storeId;
            Price = price;
            Quantity = quantity;
        }
    }
}
