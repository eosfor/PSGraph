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
    public partial class BinaryHeapTPriorityTValueTest
    {
        [Test]
        [PexGeneratedBy(typeof(BinaryHeapTPriorityTValueTest))]
        public void InsertAndEnumerate01()
        {
            BinaryHeap<int, int> binaryHeap;
            binaryHeap = BinaryHeapFactory.Create(0);
            KeyValuePair<int, int>[] keyValuePairs = new KeyValuePair<int, int>[0];
            this.InsertAndEnumerate<int, int>(binaryHeap, keyValuePairs);
        }

        [Test]
        [PexGeneratedBy(typeof(BinaryHeapTPriorityTValueTest))]
        public void InsertAndEnumerate02()
        {
            BinaryHeap<int, int> binaryHeap;
            binaryHeap = BinaryHeapFactory.Create(1);
            KeyValuePair<int, int>[] keyValuePairs = new KeyValuePair<int, int>[1];
            this.InsertAndEnumerate<int, int>(binaryHeap, keyValuePairs);
        }

        [Test]
        [PexGeneratedBy(typeof(BinaryHeapTPriorityTValueTest))]
        public void InsertAndEnumerate03()
        {
            BinaryHeap<int, int> binaryHeap;
            binaryHeap = BinaryHeapFactory.Create(0);
            KeyValuePair<int, int>[] keyValuePairs = new KeyValuePair<int, int>[1];
            this.InsertAndEnumerate<int, int>(binaryHeap, keyValuePairs);
        }

        [Test]
        [PexGeneratedBy(typeof(BinaryHeapTPriorityTValueTest))]
        public void InsertAndEnumerate04()
        {
            BinaryHeap<int, int> binaryHeap;
            binaryHeap = BinaryHeapFactory.Create(0);
            KeyValuePair<int, int>[] keyValuePairs = new KeyValuePair<int, int>[2];
            KeyValuePair<int, int> s0 = new KeyValuePair<int, int>(57409538, default(int));
            keyValuePairs[0] = s0;
            KeyValuePair<int, int> s1 = new KeyValuePair<int, int>(57409538, default(int));
            keyValuePairs[1] = s1;
            this.InsertAndEnumerate<int, int>(binaryHeap, keyValuePairs);
        }

        [Test]
        [PexGeneratedBy(typeof(BinaryHeapTPriorityTValueTest))]
        public void InsertAndEnumerate05()
        {
            BinaryHeap<int, int> binaryHeap;
            binaryHeap = BinaryHeapFactory.Create(0);
            KeyValuePair<int, int>[] keyValuePairs = new KeyValuePair<int, int>[3];
            KeyValuePair<int, int> s0 = new KeyValuePair<int, int>(3158017, default(int));
            keyValuePairs[0] = s0;
            KeyValuePair<int, int> s1 = new KeyValuePair<int, int>(871627024, default(int));
            keyValuePairs[1] = s1;
            KeyValuePair<int, int> s2 = new KeyValuePair<int, int>(3158017, default(int));
            keyValuePairs[2] = s2;
            this.InsertAndEnumerate<int, int>(binaryHeap, keyValuePairs);
        }

        [Test]
        [PexGeneratedBy(typeof(BinaryHeapTPriorityTValueTest))]
        public void InsertAndEnumerate06()
        {
            BinaryHeap<int, int> binaryHeap;
            binaryHeap = BinaryHeapFactory.Create(3);
            KeyValuePair<int, int>[] keyValuePairs = new KeyValuePair<int, int>[2];
            KeyValuePair<int, int> s0 = new KeyValuePair<int, int>(17422370, default(int));
            keyValuePairs[0] = s0;
            KeyValuePair<int, int> s1 = new KeyValuePair<int, int>(17422370, default(int));
            keyValuePairs[1] = s1;
            this.InsertAndEnumerate<int, int>(binaryHeap, keyValuePairs);
        }

    }
}
