using System.Collections.Generic;
// Простая очередь с приоритетом для алгоритма A*
public class PriorityQueue<T>
{
    private List<(T item, int priority)> elements = new List<(T, int)>();
    public int Count => elements.Count;
    public void Enqueue(T item, int priority)
    {
        elements.Add((item, priority));
    }
    public T Dequeue()
    {
        int bestIndex = 0;
        for (int i = 1; i < elements.Count; i++)
            if (elements[i].priority < elements[bestIndex].priority)
                bestIndex = i;
        var bestItem = elements[bestIndex].item;
        elements.RemoveAt(bestIndex);
        return bestItem;
    }
    public bool Contains(T item)
    {
        foreach (var e in elements)
            if (EqualityComparer<T>.Default.Equals(e.item, item))
                return true;
        return false;
    }
}
