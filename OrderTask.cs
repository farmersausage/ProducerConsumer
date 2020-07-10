using System;
using System.Threading.Tasks;

namespace ProducerConsumer
{
    class OrderTask
    {
        Random rand = new Random();
        public Store store;
        public int offerAmount;
        public bool success;

        public OrderTask(Store @out)
        {
            this.store = @out;
        }

        void Log(string msg)
        {
            Console.WriteLine( $"{store.Name}: {msg}" );
        }

        public async Task GetOffer()
        {
            Log( "Getting offer" );
            await Task.Delay( rand.Next( 3000 ) );
            offerAmount = rand.Next( 1, 5 ) * 100;
            Log( "Offer received" );
        }

        public async Task<bool> PlaceOrder()
        {
            Log( $"Placing order for {offerAmount}" );
            await Task.Delay( rand.Next( 3000 ) );
            Log( "Wager placed" );
            if (rand.Next( 100 ) >= 50)
                return success = true;
            else
                return success = false;
        }
    }
}
