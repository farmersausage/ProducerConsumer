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

        bool quotingComplete = false;
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
            await Task.Delay( PLACEMENT_DELAY );
            await PlacementWorker( );
        }

        public async Task Stop()
        {
            //TODO:: implement cancelation tokens
            //need to be passed through to quoting and placing
            cts.Cancel();
        }

        #region QUOTES
        async Task LaunchQuoteTasks()
        {
            var quoteTasks = stores
                .Select( s => s.NewOrderTask() )
                .Select( oT => LaunchQuoteTask(oT))
                .ToList();

            await Task.WhenAll( quoteTasks );
            quotingComplete = true;
        }

        async Task LaunchQuoteTask( OrderTask orderTask)
        {
            await orderTask.GetOffer();
            quotedOrderTasks.Add( orderTask );
        }
        #endregion

        async Task PlacementWorker()
        {
            Console.WriteLine( "Starting Placement Worker" );
            var placeTasks = new List<Task>();
            while( !quotingComplete || quotedOrderTasks.Count > 0 )
            {
                //check if there is a quote available
                //if there is none available then we wait
                //TODO:: research if there is a more efficient way to continue/wait
                if (!quotedOrderTasks.TryGetNext( out var nextOrderTask ))
                {
                    await Task.Delay( 25 );
                    continue;
                }

                //try to place the order. 
                //if this fails then we put the order back in to quotedOrderTasks
                if (!TryPlace( nextOrderTask, out var placeTask ))
                {
                    quotedOrderTasks.Add( nextOrderTask );
                    await Task.Delay( 25 );
                    continue;
                }

                placeTasks.Add( placeTask );
            }

            await Task.WhenAll( placeTasks );
        }

        bool TryPlace(OrderTask orderTask, out Task placeTask)
        {
            //Console.WriteLine( $"Trying to place {orderTask}" );
            placeTask = null;
            if (!TryAllocateAmount( orderTask, out var amount ))
                return false;

            //save the task instantiation cost 
            //ma
            if (amount == 0)
            {
                placeTask = Task.CompletedTask;
                return true;
            }

            //need Task.Run to circumvent out parameter in async method restriction
            placeTask = Task.Run(
                async () =>
                {
                    await orderTask.Place( amount );
                    DeallocateAmount( orderTask );
                }
            );
            return true;
        }

        bool TryAllocateAmount(OrderTask orderTask, out int orderAmount)
        {
            if (orderTask == null)
                throw new ArgumentNullException( nameof( orderTask ) );

            orderAmount = 0;
            lock (contextLock)
            {
                if (AmountAccumulated >= AmountRequested)
                {
                    //we are done, treat it as a successful allocation of 0.
                    return true;
                }

                if ((AmountAccumulated + AmountQueued) >= AmountRequested)
                {
                    //we need to wait.
                    return false;
                }

                // calculate total residual available.
                var amountAvailable = AmountRequested - (AmountQueued + AmountAccumulated);
                orderAmount = amountAvailable > orderTask.AmountAvailable ? orderTask.AmountAvailable : amountAvailable;
                AmountQueued += orderAmount; //reserve amount
                Console.WriteLine( $"ALLOCATION: {orderTask}\t      \tAccumulated:{AmountAccumulated}\tQueued:{AmountQueued}" );
            }

            return true;
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

                Console.WriteLine( $"DEALLOCATION: {orderTask}\t{ (orderTask.Success ? "SUCCESS" : "FAILED") }\tAccumulated:{AmountAccumulated}\tQueued:{AmountQueued}" );
            }
        }
    }
}
