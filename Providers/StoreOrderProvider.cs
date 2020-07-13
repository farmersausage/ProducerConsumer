using ProducerConsumer.Models;
using System;
using System.Threading.Tasks;

namespace ProducerConsumer.Providers
{
    public class StoreOrderProvider
    {
        private static Random _rand = new Random();

        public static async Task PlaceOrder(StoreQuote quote, Action<StoreOrder> completeOrderCallback)
        {
            //Artifically delay the task simulating placing the order
            await Task.Delay(_rand.Next(2500, 5000));
            completeOrderCallback.Invoke(new StoreOrder(
                    _rand.Next(100) >= 25,
                    quote.StoreId,
                    quote.Price,
                    quote.Quantity));
        }
    }
}
