using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ProducerConsumer
{
    class Order
    {
        const int PLACEMENT_DELAY = 2000;

        readonly CancellationTokenSource cts = new CancellationTokenSource();
        readonly List<Store> stores;
        readonly OrderedThreadSafeList<OrderTask> quotedOrderTasks = new OrderedThreadSafeList<OrderTask>();
        readonly object contextLock = new object();

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
            LaunchQuoteTasks();
            await LaunchPlacementWorker( );
        }

        public async Task Stop()
        {
            //TODO:: implement cancelation tokens
            //need to be passed through to quoting and placing
            cts.Cancel();
        }

        async Task LaunchQuoteTasks()
        {
            var quoteTasks = stores
                .Select( s => s.NewOrderTask() )
                .Select( oT => LaunchQuoteTask(oT))
                .ToList();

            await Task.WhenAll( quoteTasks );
            quotedOrderTasks.Complete = true;
        }

        async Task LaunchQuoteTask( OrderTask orderTask)
        {
            await orderTask.GetOffer();
            quotedOrderTasks.Add( orderTask );
        }

        async Task LaunchPlacementWorker()
        {
            await Task.Delay(PLACEMENT_DELAY);
            await PlacementWorker();
        }

        async Task PlacementWorker()
        {
            var placeTasks = new List<Task>();
            while (!quotedOrderTasks.Complete || quotedOrderTasks.Count > 0)
            {
                if (quotedOrderTasks.TryGetNext( out var nextOrderTask ) == false)
                    continue;   //waiting on quotes to come in

                //we dont need a lock here since amountrequested is const
                if (AmountAccumulated > AmountRequested)
                {
                    //we're done placing orders
                    //might make more sense to more to allocateamount
                    continue;
                }

                var orderAmount = AllocateAmount( nextOrderTask );
                Console.WriteLine( $"Placing order: {nextOrderTask}\t\tAccumulated:{AmountAccumulated}\tQueued:{AmountQueued}" );
                var placeOrderTask = PlaceOrderTask( nextOrderTask, orderAmount );
                placeTasks.Add( placeOrderTask );
            }

            await Task.WhenAll( placeTasks );
        }

        async Task PlaceOrderTask(OrderTask orderTask, int amount)
        {
            if (amount != 0)
                await orderTask.Place( amount );

            //need to call this to pulse.
            DeallocateAmount( orderTask );
        }

        int AllocateAmount(OrderTask orderTask)
        {
            if (orderTask == null)
                throw new ArgumentNullException( nameof( orderTask ) );

            int orderAmount;
            lock (contextLock)
            {
                while ((AmountAccumulated + AmountQueued) >= AmountRequested)
                {
                    //Is the order done?
                    if (AmountAccumulated >= AmountRequested)
                        return 0; 

                    //We need to wait for the queue to free up
                    Monitor.Wait( contextLock );
                }

                // calculate total residual available.
                var amountAvailable = AmountRequested - (AmountQueued + AmountAccumulated);
                orderAmount = amountAvailable > orderTask.AmountAvailable ? orderTask.AmountAvailable : amountAvailable;

                AmountQueued += orderAmount; //reserve amount
            }

            return orderAmount;
        }

        void DeallocateAmount(OrderTask orderTask)
        {
            if (orderTask == null)
                throw new ArgumentNullException( nameof( orderTask ) );

            lock (contextLock)
            {
                AmountQueued -= orderTask.AmountAttempted;
                if (orderTask.Success)
                    AmountAccumulated += orderTask.AmountAttempted;

                Console.WriteLine( $"Order finished: {orderTask}\t{ (orderTask.Success ? "SUCCESS" : "FAILED") }\tAccumulated:{AmountAccumulated}\tQueued:{AmountQueued}" );
                Monitor.Pulse( contextLock );
            }
        }
    }
}
