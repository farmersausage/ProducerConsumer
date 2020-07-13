using ProducerConsumer.Models;
using System;
using System.Threading.Tasks;

namespace ProducerConsumer.Providers
{
    public class StoreQuoteProvider
    {
        private static Random _rand = new Random();

        public static async Task<StoreQuote> GetQuoteByStore(StoreV2 store)
        {
            //Our artifical delay "getting" the quote
            await Task.Delay(_rand.Next(5000));
            var price = _rand.Next(1, 10);
            var quantity = _rand.Next(1, 5) * 100;
            Console.WriteLine($"Generate quote for store: {store.Name} Price: {price} Quantity: {quantity}");
            return new StoreQuote(
                store.Name,
                price,
                quantity);
        }
    }
}