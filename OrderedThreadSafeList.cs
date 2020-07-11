using System;
using System.Collections.Generic;
using System.Linq;

namespace ProducerConsumer
{
    class OrderedThreadSafeList<T> where T : IComparable<T>
    {
        LinkedList<T> itemList = new LinkedList<T>();
        LinkedListNode<T> headNode = null;

        public int Count
        {
            get
            {
                //do we need a lock here???
                lock (itemList)
                    return itemList.Count;
            }
        }

        public bool Complete { get; set; }

        public void Add(T item)
        {
            if (Complete)
                throw new InvalidOperationException("Marked as complete");

            lock (itemList)
            {
                var newNode = new LinkedListNode<T>( item );
                LinkedListNode<T> curr = itemList.First;
                LinkedListNode<T> previous = null;

                while ( curr != null && curr.Value.CompareTo(item) == -1 )
                {
                    previous = curr;
                    curr = curr.Next;
                }

                if (previous == null)
                {
                    itemList.AddFirst( newNode );
                }
                else
                {
                    itemList.AddAfter( previous, newNode );
                }
            }
        }

        public bool TryGetNext(out T item)
        {
            item = default( T );

            if (itemList.Count == 0)
                return false;

            lock (itemList)
            {
                if (itemList.Count == 0)
                    return false;

                item = itemList.First.Value;
                itemList.RemoveFirst();
                return true;
            }
        }


    }
}
