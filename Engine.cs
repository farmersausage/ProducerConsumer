using ProducerConsumer.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProducerConsumer
{
    public class Engine
    {
        private readonly IEnumerable<StoreV2> _stores;
        private readonly OrderedThreadSafeList<StoreQuote> _resultsBin;
        private readonly StoreQuoteProducer _quoteProducer;
        private readonly StoreOrderHandler _orderHandler;

        public Engine(IEnumerable<StoreV2> stores)
        {
            _stores = stores;
            _resultsBin = new OrderedThreadSafeList<StoreQuote>();
            _quoteProducer = new StoreQuoteProducer(_stores, _resultsBin);
            //1100 copied from old Program..
            _orderHandler = new StoreOrderHandler(1100, _resultsBin);
        }

        public async Task Start()
        {
            Console.WriteLine("----Engine Start----");
            var cts = new CancellationTokenSource();
            var getQuotesTask = _quoteProducer.Produce(cts.Token);
            var processOrdersTask = _orderHandler.PlaceOrders(() => !getQuotesTask.IsCompletedSuccessfully);

            await getQuotesTask;
            await processOrdersTask;
        }
    }
}
