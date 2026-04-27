using OptimalRouteFinder.Models;

namespace OptimalRouteFinder.Algorithms
{
    public static class HeldKarpTSP
    {       
        public static (List<City> path, double cost) Solve(double[,] dist, List<City> nodes, int startIndex, int endIndex)
        {
            int n = nodes.Count;
            int ALL = 1 << n;
            var dp = new double[ALL, n];
            var parent = new int[ALL, n];
            for (int i = 0; i < ALL; i++) for (int j = 0; j < n; j++) { dp[i, j] = double.PositiveInfinity; parent[i, j] = -1; }

            int startMask = 1 << startIndex;
            dp[startMask, startIndex] = 0;

            for (int mask = 0; mask < ALL; mask++)
            {
                for (int u = 0; u < n; u++)
                {
                    if ((mask & (1 << u)) == 0) continue;
                    if (double.IsPositiveInfinity(dp[mask, u])) continue;
                    for (int v = 0; v < n; v++)
                    {
                        if ((mask & (1 << v)) != 0) continue;
                        int newMask = mask | (1 << v);
                        double cand = dp[mask, u] + dist[u, v];
                        if (cand < dp[newMask, v])
                        {
                            dp[newMask, v] = cand;
                            parent[newMask, v] = u;
                        }
                    }
                }
            }

            int fullMask = (1 << n) - 1;
            double best = double.PositiveInfinity; int last = -1;
            for (int u = 0; u < n; u++)
            {
                double candidate = dp[fullMask, u] + dist[u, endIndex];
                if (candidate < best)
                {
                    best = candidate;
                    last = u;
                }
            }

            var path = new List<City>();
            if (last == -1) return (path, double.PositiveInfinity);

            int curMask = fullMask;
            int cur = last;
            while (cur != -1)
            {
                path.Add(nodes[cur]);
                int p = parent[curMask, cur];
                curMask ^= 1 << cur;
                cur = p;
            }
            path.Reverse();
            if (path.Last() != nodes[endIndex]) path.Add(nodes[endIndex]);

            return (path, best);
        }
    }
}
