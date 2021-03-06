// This file contains automatically generated unit tests.
// Do NOT modify this file manually.
// 
// When Pex is invoked again,
// it might remove or update any previously generated unit tests.
// 
// If the contents of this file becomes outdated, e.g. if it does not
// compile anymore, you may delete this file and invoke Pex again.
using System;
using System.Collections.Generic;
using QuickGraph.Unit;
using Microsoft.Pex.Framework.Generated;

namespace QuickGraph.Collections
{
    public partial class BinaryHeapTPriorityTValueEnumeratorTest
    {
        [Test]
        [PexGeneratedBy(typeof(BinaryHeapTPriorityTValueEnumeratorTest))]
        public void InsertManyAndEnumerateUntyped01()
        {
            BinaryHeap<int, int> binaryHeap;
            binaryHeap = BinaryHeapFactory.Create(0);
            KeyValuePair<int, int>[] keyValuePairs = new KeyValuePair<int, int>[0];
            this.InsertManyAndEnumerateUntyped<int, int>(binaryHeap, keyValuePairs);
        }

        [Test]
        [PexGeneratedBy(typeof(BinaryHeapTPriorityTValueEnumeratorTest))]
        public void InsertManyAndEnumerateUntyped02()
        {
            BinaryHeap<int, int> binaryHeap;
            binaryHeap = BinaryHeapFactory.Create(1);
            KeyValuePair<int, int>[] keyValuePairs = new KeyValuePair<int, int>[1];
            this.InsertManyAndEnumerateUntyped<int, int>(binaryHeap, keyValuePairs);
        }

        [Test]
        [PexGeneratedBy(typeof(BinaryHeapTPriorityTValueEnumeratorTest))]
        public void InsertManyAndEnumerateUntyped03()
        {
            BinaryHeap<int, int> binaryHeap;
            binaryHeap = BinaryHeapFactory.Create(0);
            KeyValuePair<int, int>[] keyValuePairs = new KeyValuePair<int, int>[1];
            this.InsertManyAndEnumerateUntyped<int, int>(binaryHeap, keyValuePairs);
        }

        [Test]
        [PexGeneratedBy(typeof(BinaryHeapTPriorityTValueEnumeratorTest))]
        public void InsertManyAndEnumerateUntyped04()
        {
            BinaryHeap<int, int> binaryHeap;
            binaryHeap = BinaryHeapFactory.Create(0);
            KeyValuePair<int, int>[] keyValuePairs = new KeyValuePair<int, int>[2];
            this.InsertManyAndEnumerateUntyped<int, int>(binaryHeap, keyValuePairs);
        }

        [Test]
        [PexGeneratedBy(typeof(BinaryHeapTPriorityTValueEnumeratorTest))]
        public void InsertManyAndEnumerateUntyped05()
        {
            BinaryHeap<int, int> binaryHeap;
            binaryHeap = BinaryHeapFactory.Create(2);
            KeyValuePair<int, int>[] keyValuePairs = new KeyValuePair<int, int>[2];
            this.InsertManyAndEnumerateUntyped<int, int>(binaryHeap, keyValuePairs);
        }

        [Test]
        [PexGeneratedBy(typeof(BinaryHeapTPriorityTValueEnumeratorTest))]
        public void InsertManyAndEnumerateUntyped06()
        {
            BinaryHeap<int, int> binaryHeap;
            binaryHeap = BinaryHeapFactory.Create(3);
            KeyValuePair<int, int>[] keyValuePairs = new KeyValuePair<int, int>[3];
            KeyValuePair<int, int> s0 = new KeyValuePair<int, int>(2119361876, default(int))
              ;
            keyValuePairs[0] = s0;
            KeyValuePair<int, int> s1 = new KeyValuePair<int, int>(2119361876, default(int))
              ;
            keyValuePairs[1] = s1;
            KeyValuePair<int, int> s2 = new KeyValuePair<int, int>(2119361876, default(int))
              ;
            keyValuePairs[2] = s2;
            this.InsertManyAndEnumerateUntyped<int, int>(binaryHeap, keyValuePairs);
        }

    }
}
