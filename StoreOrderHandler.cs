using ProducerConsumer.Models;
using ProducerConsumer.Providers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProducerConsumer
{
    public class StoreOrderHandler
    {
        private readonly int _quantityRequired;
        private int _quantityInFlight;
        private int _quantityReceived;

        private readonly OrderedThreadSafeList<StoreQuote> _availableQuotes;

        public StoreOrderHandler(int quantityRequired, OrderedThreadSafeList<StoreQuote> availableQuotes)
        {
            _quantityRequired = quantityRequired;
            _availableQuotes = availableQuotes;
        }

        /// <summary>
        /// Place orders as necessary from the quotes available
        /// </summary>
        /// <returns>A collection of successfully placed orders</returns>
        public async Task<IEnumerable<StoreOrder>> PlaceOrders(Func<bool> quotesBeingGenerated)
        {
            var inFlightOrders = new List<Task>();
            var successfulOrders = new ConcurrentBag<StoreOrder>();
            while (_quantityReceived < _quantityRequired && (quotesBeingGenerated.Invoke() || QuotesAvailable || inFlightOrders.Count > 0))
            {
                if (_quantityInFlight + _quantityReceived < _quantityRequired)
                {


                    if (_availableQuotes.TryGetNext(out var quote))
                    {
                        //Atomic add
                        Interlocked.Add(ref _quantityInFlight, quote.Quantity);
                        var orderTask = StoreOrderProvider.PlaceOrder(quote, (x) =>
                        {
                            if (x.Success)
                            {
                                successfulOrders.Add(x);
                                Interlocked.Add(ref _quantityReceived, x.Quantity);
                                Console.WriteLine($"Order Successful for store: {x.StoreId}. Successully ordered quantity {x.Quantity}");
                            }
                            else
                            {
                                Console.WriteLine($"Order Failed for store: {x.StoreId}. Missed out on quantity {x.Quantity}");
                            }
                            Interlocked.Add(ref _quantityInFlight, (-1) * x.Quantity);
                        });
                        Console.WriteLine($"Placing order for store: {quote.StoreId} Price: {quote.Price} Quantity: {quote.Quantity}");
                        inFlightOrders.Add(orderTask);
                    }
                    else
                    {
                        //Comment out this log message because it spams the console while we're "waiting" for the first quote
                        //Console.WriteLine("Error retrieving quote from quote bin");
                    }
                }
                else
                {
                    Console.WriteLine($"Quantity Required: {_quantityRequired} Quantity Received: {_quantityReceived} Quantity Inflight: {_quantityInFlight}");
                    Console.WriteLine($"Waiting for at least one inflight request to finish to see if we should place another order. {inFlightOrders.Count} orders currently in process.");
                    await Task.WhenAny(inFlightOrders);
                    inFlightOrders.RemoveAll(x => x.IsCompleted);
                }
            }
            Console.WriteLine($"**Store Order Handler Complete**");
            Console.WriteLine($"Quantity Required: {_quantityRequired} Quantity Received: {_quantityReceived} Quantity Inflight: {_quantityInFlight}");
            return successfulOrders;
        }

        private bool QuotesAvailable => _availableQuotes.Count > 0;
    }
}
