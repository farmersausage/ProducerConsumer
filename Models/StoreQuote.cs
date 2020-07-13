using System;
using System.Collections.Generic;
using System.Text;

namespace ProducerConsumer.Models
{
    public readonly struct StoreQuote : IComparable<StoreQuote>
    {
        public string StoreId { get; }//In this case == store name'
        public decimal Price { get; }
        public int Quantity { get; }

        public StoreQuote(string storeId, decimal price, int quantity)
        {
            StoreId = storeId;
            Price = price;
            Quantity = quantity;
        }

        public int CompareTo(StoreQuote other)
        {
            if (other.Price > Price)
            {
                return 1;
            }
            else if (other.Price == Price)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }
    }
}
