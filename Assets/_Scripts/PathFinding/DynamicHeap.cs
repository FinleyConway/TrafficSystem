using System.Collections.Generic;

namespace TrafficSystem
{
    public class DynamicHeap<T> where T : IHeapItem<T>
    {
        private readonly List<T> m_Items;
        public int Count { get; private set; }

        public DynamicHeap()
        {
            m_Items = new List<T>();
        }

        public void UpdateItem(T item)
        {
            SortUp(item);
        }

        public void Add(T item)
        {
            item.HeapIndex = Count;
            m_Items.Insert(Count, item);
            SortUp(item);
            Count++;
        }

        public T RemoveFirst()
        {
            T firstItem = m_Items[0];
            Count--;
            m_Items[0] = m_Items[Count];
            m_Items[0].HeapIndex = 0;
            SortDown(m_Items[0]);
            return firstItem;
        }

        public bool Contains(T item)
        {
            return m_Items.IndexOf(item) >= 0;
        }

        private void SortDown(T item)
        {
            while (true)
            {
                int childIndexLeft = item.HeapIndex * 2 + 1;
                int childIndexRight = item.HeapIndex * 2 + 2;

                if (childIndexLeft < Count)
                {
                    int swapIndex = childIndexLeft;
                    if (childIndexRight < Count)
                    {
                        if (m_Items[childIndexLeft].CompareTo(m_Items[childIndexRight]) < 0)
                        {
                            swapIndex = childIndexRight;
                        }
                    }

                    if (item.CompareTo(m_Items[swapIndex]) < 0)
                    {
                        Swap(item, m_Items[swapIndex]);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
        }

        private void SortUp(T item)
        {
            int parentIndex = (item.HeapIndex - 1) / 2;

            while (true)
            {
                T parentItem = m_Items[parentIndex];
                if (item.CompareTo(parentItem) > 0)
                {
                    Swap(item, parentItem);
                }
                else
                {
                    break;
                }

                parentIndex = (item.HeapIndex - 1) / 2;
            }
        }

        private void Swap(T itemA, T itemB)
        {
            m_Items[itemA.HeapIndex] = itemB;
            m_Items[itemB.HeapIndex] = itemA;
            (itemB.HeapIndex, itemA.HeapIndex) = (itemA.HeapIndex, itemB.HeapIndex);
        }
    }
}