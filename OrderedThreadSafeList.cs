using System;
using System.Collections.Generic;

namespace ProducerConsumer
{
    class OrderedThreadSafeList<T>
    {
        //object obj = new object();
        List<T> itemList = new List<T>();

        public bool Add(T item)
        {
            lock (itemList)
                itemList.Add( item );

            //Console.WriteLine( $"Adding: {item}" );
            return true;
        }

        public bool GetNext(out T item)
        {
            item = default( T );

            if (itemList.Count == 0)
                return false;

            lock (itemList)
            {
                if (itemList.Count == 0)
                    return false;

                itemList.Sort();
                item = itemList[0];
                itemList.Remove( item );
                //Console.WriteLine( $"Getting next item: {item}" );
                return true;
            }
        }
    }
}
