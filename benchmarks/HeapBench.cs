using System;
using BenchmarkDotNet.Attributes;
using data_structures;
using Priority_Queue;

namespace Benchmarks {
    public class HeapBenchmarks {
        [Params(100, 1000, 10000)]
        public int QueueSize;

        private Heap<int> heap;
        private int[] randomOrder;
        private int[] items;

        [GlobalSetup]
        public void GlobalSetup() {
            items = new int[QueueSize];
            Random rand = new Random(34829061);
            for (int i = 0; i < QueueSize; i++) {
                items[i] = rand.Next(16777216); // constrain to range float can hold with no rounding
            }
        }


        [Benchmark]
        public void Enqueue() {
            heap = new Heap<int>();
            for (int i = 0; i < QueueSize; i++) {
                heap.Add(items[i]);
            }
        }

        [Benchmark]
        public void EnqueueBackwards() {
            heap = new Heap<int>();
            for (int i = QueueSize - 1; i >= 0; i--) {
                heap.Add(items[i]);
            }
        }

        [Benchmark]
        public void EnqueueDequeue() {
            Enqueue();
            for (int i = 0; i < QueueSize; i++) {
                heap.Pop();
            }
        }

        [Benchmark]
        public void EnqueueBackwardsDequeue() {
            EnqueueBackwards();

            for (int i = 0; i < QueueSize; i++) {
                heap.Pop();
            }
        }

        [Benchmark]
        public bool EnqueueContains() {
            Enqueue();
            bool ret = true; // to ensure the compiler doesn't optimize the contains calls out of existence

            for (int i = 0; i < 100; i++) {
                for (int j = 0; j < QueueSize; j++) {
                    ret &= heap.Contains(i);
                }
            }

            return ret;
        }
    }
}