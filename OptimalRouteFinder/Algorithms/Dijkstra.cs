using OptimalRouteFinder.Models;

namespace OptimalRouteFinder.Algorithms
{
    public static class Dijkstra
    {
        public static (Dictionary<City,double> dist, Dictionary<City,City?> prev) Run(MapGraph g, City source)
        {
            var dist = g.Cities.ToDictionary(c => c, c => double.PositiveInfinity);
            var prev = g.Cities.ToDictionary(c => c, c => (City?)null);
            var pq = new SimplePriorityQueue<City>();
            dist[source] = 0;
            pq.Enqueue(source, 0);

            while (pq.Count > 0)
            {
                var u = pq.Dequeue();
                var du = dist[u];
                foreach (var (v, w) in g.Roads[u])
                {
                    double alt = du + w;
                    if (alt < dist[v])
                    {
                        dist[v] = alt;
                        prev[v] = u;
                        pq.EnqueueOrUpdate(v, alt);
                    }
                }
            }
            return (dist, prev);
        }
    }

    // Minimal priority queue (binary heap) for City with double priority
    class SimplePriorityQueue<T> where T : class
    {
        private List<(T item, double priority)> _heap = new();

        public int Count => _heap.Count;
        public void Enqueue(T item, double priority)
        {
            _heap.Add((item, priority));
            HeapifyUp(_heap.Count - 1);
        }
        public T Dequeue()
        {
            var res = _heap[0].item;
            _heap[0] = _heap[^1];
            _heap.RemoveAt(_heap.Count - 1);
            HeapifyDown(0);
            return res;
        }
        public void EnqueueOrUpdate(T item, double priority)
        {
            for (int i=0;i<_heap.Count;i++)
            {
                if (ReferenceEquals(_heap[i].item, item))
                {
                    if (priority < _heap[i].priority)
                    {
                        _heap[i] = (item, priority);
                        HeapifyUp(i);
                    }
                    return;
                }
            }
            Enqueue(item, priority);
        }
        private void HeapifyUp(int i)
        {
            while (i>0)
            {
                int p = (i-1)/2;
                if (_heap[p].priority <= _heap[i].priority) break;
                var tmp = _heap[p]; _heap[p] = _heap[i]; _heap[i] = tmp;
                i = p;
            }
        }
        private void HeapifyDown(int i)
        {
            while (true)
            {
                int l = 2*i+1, r = 2*i+2, smallest = i;
                if (l < _heap.Count && _heap[l].priority < _heap[smallest].priority) smallest = l;
                if (r < _heap.Count && _heap[r].priority < _heap[smallest].priority) smallest = r;
                if (smallest == i) break;
                var tmp = _heap[smallest]; _heap[smallest] = _heap[i]; _heap[i] = tmp;
                i = smallest;
            }
        }
    }
}
