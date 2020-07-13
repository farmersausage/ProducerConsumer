using ProducerConsumer.Models;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ProducerConsumer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //Keep old store generation..
            var stores = Enumerable.Range(0, 15).Select(i => new StoreV2(i.ToString())).ToList();

            var engine = new Engine(stores);
            await engine.Start();

            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}
