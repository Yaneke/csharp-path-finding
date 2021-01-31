using System;
using BenchmarkDotNet.Attributes;
using Priority_Queue;

namespace Benchmarks {
    /// <summary>
    /// PriorityQueue benchmarks from the official git repo.
    /// </summary>
    /// https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp/blob/master/Priority%20Queue%20Benchmarks/Benchmarks.cs
    public class FastPriorityQueueBenchmarks {
        [Params(100, 1000, 10000)]
        public int QueueSize;

        public FastPriorityQueueNode[] Nodes;
        public int[] RandomPriorities;
        public int[] RandomUpdatePriorities;

        private FastPriorityQueue<FastPriorityQueueNode> Queue;

        [GlobalSetup]
        public void GlobalSetup() {
            Queue = new FastPriorityQueue<FastPriorityQueueNode>(QueueSize);
            Nodes = new FastPriorityQueueNode[QueueSize];
            RandomPriorities = new int[QueueSize];
            RandomUpdatePriorities = new int[QueueSize];
            Random rand = new Random(34829061);
            for (int i = 0; i < QueueSize; i++) {
                Nodes[i] = new FastPriorityQueueNode();
                RandomPriorities[i] = rand.Next(16777216); // constrain to range float can hold with no rounding
                RandomUpdatePriorities[i] = rand.Next(16777216); // constrain to range float can hold with no rounding
            }
        }

        [IterationCleanup]
        public void IterationCleanup() {
            Queue.Clear();
        }

        [Benchmark]
        public void Enqueue() {
            Queue.Clear();
            for (int i = 0; i < QueueSize; i++) {
                Queue.Enqueue(Nodes[i], i);
            }
        }

        [Benchmark]
        public void EnqueueBackwards() {
            Queue.Clear();
            for (int i = QueueSize - 1; i >= 0; i--) {
                Queue.Enqueue(Nodes[i], i);
            }
        }


        [Benchmark]
        public void EnqueueDequeue() {
            Enqueue();

            for (int i = 0; i < QueueSize; i++) {
                Queue.Dequeue();
            }
        }

        [Benchmark]
        public void EnqueueBackwardsDequeue() {
            EnqueueBackwards();

            for (int i = 0; i < QueueSize; i++) {
                Queue.Dequeue();
            }
        }

        [Benchmark]
        public bool EnqueueContains() {
            Enqueue();
            bool ret = true; // to ensure the compiler doesn't optimize the contains calls out of existence

            for (int i = 0; i < 100; i++) {
                for (int j = 0; j < QueueSize; j++) {
                    ret &= Queue.Contains(Nodes[j]);
                }
            }

            return ret;
        }
    }
}