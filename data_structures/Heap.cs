using System;
using System.Collections.Generic;

namespace data_structures {
    /// The heap data structure always has the lowest item on the top.
    public class Heap<T> where T : IComparable<T> {
        private List<T> items;
        private Dictionary<T, int> itemIndices;

        public int Count {
            get {
                return this.items.Count;
            }
        }

        public Heap() {
            this.items = new List<T>();
            this.itemIndices = new Dictionary<T, int>();
        }

        /// Add the item to the heap, or update the value if the item already exists.
        public void Add(T item) {
            if (this.Contains(item)) {
                this.Update(item);
            } else {
                this.items.Add(item);
                this.itemIndices.Add(item, this.Count - 1);
                this.SortUp(this.Count - 1);
            }
        }

        public bool Contains(T item) {
            return itemIndices.ContainsKey(item);
        }

        private int GetIndex(T item) {
            for (int i = 0; i < this.items.Count; i++) {
                if (this.items[i].Equals(item)) {
                    return i;
                }
            }
            return -1;
        }

        /// Update an item's value
        public bool Update(T item) {
            if (this.Contains(item)) {
                int index = this.itemIndices[item];
                if (item.CompareTo(this.items[index]) < 0) {
                    this.items[index] = item;
                    this.SortUp(index);
                } else {
                    this.items[index] = item;
                    this.SortDown(index);
                }
                return true;
            }
            return false;
        }

        private int GetParentIndex(int pos) {
            return (pos - 1) / 2;
        }

        private int GetLeftChildIndex(int pos) {
            return pos * 2 + 1;
        }

        private int GetRightChildIndex(int pos) {
            return pos * 2 + 2;
        }

        /// Removes and returns the first item of the heap.
        public T Pop() {
            if (this.Count <= 0) {
                return default(T);
            }
            T res = this.items[0];
            this.itemIndices.Remove(res);
            if (this.Count > 0) {
                int lastIdx = this.Count - 1;
                this.items[0] = this.items[lastIdx];
                this.items.RemoveAt(lastIdx);
                this.SortDown(0);
            }
            return res;
        }

        /// Sorts the item up (compare it with its parent) and returns the its new index.
        private int SortUp(int itemIndex) {
            T item = this.items[itemIndex];
            int parentIndex = this.GetParentIndex(itemIndex);
            // Push the element up in the heap while it is lower than its parent
            while (item.CompareTo(this.items[parentIndex]) < 0) {
                // Swap the parent and the child
                this.Swap(parentIndex, itemIndex);
                itemIndex = parentIndex;
                parentIndex = GetParentIndex(itemIndex);
            }
            return itemIndex;
        }

        /// Sorts the item down (compare it with its children) and returns the its new index.
        private int SortDown(int itemIndex) {
            if (itemIndex < this.Count) {
                T item = this.items[itemIndex];
                int leftChildIndex = this.GetLeftChildIndex(itemIndex);
                int rightChildIndex = this.GetRightChildIndex(itemIndex);
                bool gtRight = rightChildIndex < this.Count && item.CompareTo(this.items[rightChildIndex]) > 0;
                bool gtLeft = leftChildIndex < this.Count && item.CompareTo(this.items[leftChildIndex]) > 0;
                while (gtLeft || gtRight) {
                    // Possibilities
                    //  - left and right child are lower -> compare those and swap with the lowest
                    //  - left is lower
                    //  - right is lower
                    if (gtLeft && gtRight) {
                        if (this.items[leftChildIndex].CompareTo(this.items[rightChildIndex]) < 0) {
                            // left child is lower
                            this.Swap(itemIndex, leftChildIndex);
                            itemIndex = leftChildIndex;
                        } else {
                            // right child is lower
                            this.Swap(itemIndex, rightChildIndex);
                            itemIndex = rightChildIndex;
                        }
                    } else if (gtLeft) {
                        this.Swap(itemIndex, leftChildIndex);
                        itemIndex = leftChildIndex;
                    } else if (gtRight) {
                        this.Swap(itemIndex, rightChildIndex);
                        itemIndex = rightChildIndex;
                    }
                    leftChildIndex = this.GetLeftChildIndex(itemIndex);
                    rightChildIndex = this.GetRightChildIndex(itemIndex);
                    gtRight = rightChildIndex < this.Count && item.CompareTo(this.items[rightChildIndex]) > 0;
                    gtLeft = leftChildIndex < this.Count && item.CompareTo(this.items[leftChildIndex]) > 0;
                }
            }
            return itemIndex;
        }

        /// Swap two items at the specified indices.
        private void Swap(int index1, int index2) {
            T item1 = this.items[index1];
            T item2 = this.items[index2];
            this.itemIndices[item1] = index2;
            this.itemIndices[item2] = index1;
            this.items[index1] = item2;
            this.items[index2] = item1;
        }

        public override string ToString() {
            var res = "TODO";
            return res + "\n";//String.Join(", ", this.items);
        }
    }

}