using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ProducerConsumer
{
    class Program
    {
        
        static void Main(string[] args)
        {
            List<Store> stores = Enumerable.Range( 0, 5 ).Select( i => new Store( i.ToString() ) ).ToList();
            var order = new Order(stores, 1000);
            Console.WriteLine( "Hello World!" );
        }
    }

    class Order
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        List<Store> stores; 

        public int AmountRequested { get; }
        public int AmountAccumulated { get; private set; }
        public int AmountQueued { get; private set; }

        public Order(List<Store> stores, int amountRequested)
        {
            this.stores = stores;
            AmountRequested = amountRequested;
        }

        public async Task Start()
        {
            var orderTasks = stores.Select( s => s.NewOrderTask() ).ToList();
            var transformBlock = new TransformBlock<OrderTask, OrderTask>(
                async oT =>
                {
                    await oT.GetOffer();
                    return oT;
                },
                new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = -1 }
            );
            orderTasks.ForEach( oT => transformBlock.Post( oT ) );


        }

        public async Task Stop()
        {
            cts.Cancel();
        }
    }
}
