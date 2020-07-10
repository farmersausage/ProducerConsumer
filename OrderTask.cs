using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace ProducerConsumer
{
    class OrderTask : IComparable<OrderTask>
    {
        Random rand = new Random();
        public Store store;
        public int AmountAvailable;
        public int AmountAttempted;
        public decimal Quote;
        public bool Success;

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
            Quote = rand.Next( 1, 5 );
            AmountAvailable = rand.Next( 1, 7 ) * 100;
            Log( $"Offer received.\t{Quote}\t{AmountAvailable}" );
        }

        public async Task<bool> PlaceOrder(int amount)
        {
            AmountAttempted = amount;
            Log( $"Placing order for {AmountAttempted} @ {Quote}" );
            await Task.Delay( rand.Next( 3000 ) );
            Log( "Wager placed" );
            if (rand.Next( 100 ) >= 50)
                return Success = true;
            else
                return Success = false;
        }

        public override string ToString()
        {
            return store.Name;
        }

        public int CompareTo([AllowNull] OrderTask other)
        {
            if (other is null)
                return 1;

            if (other.Quote > this.Quote)
                return 1;
            else if (other.Quote.Equals( this.Quote ))
                return 0;
            else
                return -1;
        }
    }
}
