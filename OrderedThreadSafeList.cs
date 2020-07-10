using System;
using System.Collections.Generic;
using System.Linq;

namespace ProducerConsumer
{
    class OrderedThreadSafeList<T>
    {
        //TODO:: convert this all to a linked list 
        //add it in sorted way, and then just pop the top node off on getnext
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

                //itemList.Sort();
                //item = itemList[0];
                var maxItem = itemList.Max();
                itemList.Remove( maxItem );
                //Console.WriteLine( $"Getting next item: {item}" );
                return true;
            }
        }


    }
}
