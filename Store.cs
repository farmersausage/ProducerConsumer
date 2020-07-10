using System;

namespace ProducerConsumer
{
    class Store
    {
        public string Name;

        public Store(string name)
        {
            Name = name ?? throw new ArgumentNullException( nameof( name ) );
        }

        public OrderTask NewOrderTask() => new OrderTask( this );
    }
}
