using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace RustServerManager.Services
{
    public class PluginProfiler
    {
        private readonly ConcurrentDictionary<string, PluginStats> _stats = new();
        private readonly Stopwatch _globalWatch = Stopwatch.StartNew();

        public void BeginSample(string pluginName)
        {
            if (!_stats.ContainsKey(pluginName))
                _stats[pluginName] = new PluginStats(pluginName);

            var stat = _stats[pluginName];
            stat._localWatch.Restart();
            stat._startMem = GC.GetTotalMemory(false);
        }

        public void EndSample(string pluginName)
        {
            if (!_stats.TryGetValue(pluginName, out var stat)) return;

            stat._localWatch.Stop();
            var duration = stat._localWatch.Elapsed.TotalMilliseconds;
            var endMem = GC.GetTotalMemory(false);
            var memDelta = endMem - stat._startMem;

            Interlocked.Increment(ref stat.Calls);
            Interlocked.Add(ref stat.TotalMs, (long)(duration * 1000));
            Interlocked.Add(ref stat.TotalMemory, memDelta);
        }

        public PluginStats[] GetTopPluginsByCpu(int top = 5)
        {
            return _stats.Values
                .OrderByDescending(s => s.AverageMs)
                .Take(top)
                .ToArray();
        }

        public PluginStats[] GetTopPluginsByMemory(int top = 5)
        {
            return _stats.Values
                .OrderByDescending(s => s.AverageMemoryKB)
                .Take(top)
                .ToArray();
        }

        public class PluginStats
        {
            public string Name { get; }
            public long Calls;
            public long TotalMs;
            public long TotalMemory;

            internal Stopwatch _localWatch = new();
            internal long _startMem;

            public double AverageMs => Calls == 0 ? 0 : (TotalMs / 1000.0) / Calls;
            public double AverageMemoryKB => Calls == 0 ? 0 : (TotalMemory / 1024.0) / Calls;

            public PluginStats(string name) => Name = name;
        }
    }
}
