using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ProducerConsumer
{
    /// <summary>
    /// Place an order for a total amount across a collection of stores. 
    /// Process takes place as follows: 
    /// - each store creates an order task that holds all state and store-order functionality
    /// - for every order task, launch a get quote task
    /// - a get quote task attempts to get a store quote, then add it to the quote list, the check to see if the minimum amount of time has passed, then signal that storeorders should be attempted
    /// - store orders are attempted by checking if there is any amount left to fill in the order
    /// - if yes then pull the best quote, and attempt to place an order. all state variables need to be updated. 
    /// - after an order is completed signal that store orders should be attempted 
    /// </summary>
    class Program
    {
        
        static async Task Main(string[] args)
        {
            List<Store> stores = Enumerable.Range( 0, 10 ).Select( i => new Store( i.ToString() ) ).ToList();
            var order = new Order(stores, 800);

            await order.Start();

            Console.WriteLine( "Done" );
            Console.ReadKey();
        }
    }
}
