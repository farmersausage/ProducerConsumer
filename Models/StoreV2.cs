using System;
using System.Collections.Generic;
using System.Text;

namespace ProducerConsumer.Models
{
    public readonly struct StoreV2
    {
        public string Name { get; }

        public StoreV2(string name)
        {
            Name = name;
        }
    }
}
