using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProducerConsumer
{
    class Order
    {
        const int timeToWait = 2500;

        CancellationTokenSource cts = new CancellationTokenSource();
        List<Store> stores;
        List<OrderTask> orderTasks = new List<OrderTask>();
        OrderedThreadSafeList<OrderTask> quotedOrderTasks = new OrderedThreadSafeList<OrderTask>();
        Task timerTask;

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
            orderTasks = stores.Select( s => s.NewOrderTask() ).ToList();
            timerTask = Task.Delay( timeToWait );
            var quoteTasks = orderTasks.Select( oT => LaunchQuoteTask( oT ) ).ToList();

            //TODO: need to return an awaitable Task that represents ongoing order
            //not sure how
        }

        public async Task Stop()
        {
            //TODO:: implement cancelation tokens
            //need to be passed through to quoting and placing
            cts.Cancel();
        }

        async Task LaunchQuoteTask( OrderTask orderTask )
        {
            await orderTask.GetOffer();
            quotedOrderTasks.Add( orderTask );
            await timerTask;

            LaunchAllPlacementTasks();
        }

        void LaunchAllPlacementTasks()
        {

            while ((AmountQueued + AmountAccumulated) < AmountRequested)
            {
                int orderAmount;
                OrderTask orderTask;

                lock (stores)
                {
                    //Order is done
                    if (AmountAccumulated >= AmountRequested)
                    {
                        Console.WriteLine( "Done" );
                        return;
                    }

                    //Cannot dispatch any at this time. Need to wait for orders to finish
                    if ((AmountQueued + AmountAccumulated) >= AmountRequested)
                    {
                        Console.WriteLine( "No amount availability" );
                        return;
                    }

                    //Get next order task. 
                    //If this fails, i think it will only be when its called after a completed order and quote tasks are either pending, or are done
                    //either way, more fancy logic wont solve anything. 
                    if (!quotedOrderTasks.GetNext( out orderTask ))
                    {
                        Console.WriteLine( "No quoted tasks available" );
                        return;
                    }

                    // calculate total residual available.
                    var amountAvailable = AmountRequested - (AmountQueued + AmountAccumulated);
                    orderAmount = amountAvailable > orderTask.AmountAvailable ? orderTask.AmountAvailable : amountAvailable;

                    AmountQueued += orderAmount; //reserve amount
                    Console.WriteLine( $"Placing order: {orderTask}\t\tAccumulated:{AmountAccumulated}\tQueued:{AmountQueued}" );
                }

                LaunchPlacementTask( orderTask, orderAmount );
            }
        }

        async Task LaunchPlacementTask(OrderTask orderTask, int amount)
        {
            await orderTask.PlaceOrder( amount );
            //update all order state vars
            lock (stores)
            {
                AmountQueued -= orderTask.AmountAttempted;
                if (orderTask.Success)
                    AmountAccumulated += orderTask.AmountAttempted;

                Console.WriteLine( $"Order finished: {orderTask}\t{ (orderTask.Success ? "SUCCESS" : "FAILED") }\tAccumulated:{AmountAccumulated}\tQueued:{AmountQueued}" );
            }

            LaunchAllPlacementTasks();
        }


    }
}
