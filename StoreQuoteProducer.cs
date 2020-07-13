using ProducerConsumer.Models;
using ProducerConsumer.Providers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ProducerConsumer
{
    public class StoreQuoteProducer
    {
        private readonly IEnumerable<StoreV2> _stores;
        private readonly OrderedThreadSafeList<StoreQuote> _resultsBin;

        /// <summary>
        /// Initialize StoreQuoteProducer
        /// </summary>
        /// <param name="stores">The list of stores we'll be generating quotes for</param>
        /// <param name="resultsBin">This in memory data structure is where we will asynchronously dump our results to be processed in real time</param>
        public StoreQuoteProducer(IEnumerable<StoreV2> stores, OrderedThreadSafeList<StoreQuote> resultsBin)
        {
            _stores = stores;
            _resultsBin = resultsBin;
        }

        /// <summary>
        /// Asynchronously produce Store Quotes that will be added to the results bin provided during object initialization.
        /// Task completes when there will be no more Store Quotes produced.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token that will become cancelled if our caller wants us to stop producing more quotes</param>
        /// <returns></returns>
        public Task Produce(CancellationToken cancellationToken)
        {
            var transformStore = new TransformBlock<StoreV2, StoreQuote>(async s =>
            {
                var quote = await StoreQuoteProvider.GetOfferByStore(s);
                return quote;
            },
            new ExecutionDataflowBlockOptions { EnsureOrdered = false, MaxDegreeOfParallelism = 10, CancellationToken = cancellationToken });
            var quoteBuffer = new BufferBlock<StoreQuote>(new ExecutionDataflowBlockOptions { EnsureOrdered = false, CancellationToken = cancellationToken });

            transformStore.LinkTo(quoteBuffer, new DataflowLinkOptions { PropagateCompletion = true });

            //Start the consumer, hold the task.
            var returnVal = InternalConsume(quoteBuffer);

            foreach (var store in _stores)
            {
                transformStore.Post(store);
            }

            transformStore.Complete();
            return returnVal;
        }

        private async Task InternalConsume(ISourceBlock<StoreQuote> source)
        {
            while (await source.OutputAvailableAsync())
            {
                var quote = source.Receive();
                _resultsBin.Add(quote);
            }
        }
    }
}
