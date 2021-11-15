using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DataStructures
{
	public class PerformanceData
	{
		public class Metric
		{
			private readonly Dictionary<string, ulong> data = new();
			public IEnumerable<string> Labels => data.Keys;

			public ulong this[string label]
			{
				get => data[label];
				set => data[label] = value;
			}

			public Metric(IEnumerable<string> labels)
			{
				foreach (string label in labels)
				{
					data[label] = 0;
				}
			}
			
			public string ReportString() 
				=> Labels.Aggregate(string.Empty, (current, label) => current + $"{label}\t{this[label]}\n");
		}
		
		public Metric Counts { get; }
		public Metric TickCounts { get; }
		
		public PerformanceData(IEnumerable<string> countLabels, IEnumerable<string> tickCountLabels)
		{
			Counts = new Metric(countLabels);
			TickCounts = new Metric(tickCountLabels);
		}
		
		public PerformanceData(Metric counts, Metric tickCounts)
		{
			Counts = counts;
			TickCounts = tickCounts;
		}

		public string ReportString() => Counts.ReportString() + TickCounts.ReportString();
	}
	
	public static class PerformanceDataExtensions
	{
		public static PerformanceData.Metric Average(this IEnumerable<PerformanceData.Metric> metrics)
		{
			var metricArray = metrics as PerformanceData.Metric[] ?? metrics.ToArray();
			uint count = (uint) metricArray.Length;
			
			if (count == 0)
			{
				throw new InvalidOperationException("Cannot take the average of an empty list.");
			}
			
			
			var first = metricArray.First();
			var labels = first.Labels as string[] ?? first.Labels.ToArray();
			var sum = new PerformanceData.Metric(labels);

			foreach (var metric in metricArray)
			{
				foreach (string label in labels)
				{
					sum[label] += metric[label];
				}
			}
			
			foreach (string label in labels)
			{
				sum[label] = (ulong) (sum[label]  / count);
			}

			return sum;
		}
		
		public static PerformanceData Average(this IEnumerable<PerformanceData> data)
		{
			// ReSharper disable method PossibleMultipleEnumeration
			if (!data.Any())
			{
				throw new InvalidOperationException("Cannot take the average of an empty list.");
			}
			
			return new PerformanceData(
				data.Select(d => d.Counts).Average(), 
				data.Select(d => d.TickCounts).Average());
		}
	}

	public class PerformanceMonitor
	{
		private Dictionary<string, Stopwatch> watches = new();
		private string currentWatchLabel = null;
		private Stopwatch currentWatch = null;
		
		public PerformanceMonitor(IEnumerable<string> labels)
		{
			foreach (string label in labels)
			{
				watches[label] = new Stopwatch();
			}
		}

		public void Watch(string label)
		{
			if (currentWatch != null)
			{
				currentWatch.Stop();
			}

			currentWatchLabel = label;
			currentWatch = watches[label];
			currentWatch.Start();
		}

		public void Reset()
		{
			foreach (var watch in watches.Values)
			{
				watch.Reset();
			}

			currentWatchLabel = null;
			currentWatch = null;
		}

		public void SetPerformanceData(PerformanceData data)
		{
			foreach (string label in watches.Keys)
			{
				data.TickCounts[label] = (ulong) watches[label].ElapsedTicks;
			}
		}

		public void Stop()
		{
			if (currentWatch != null)
			{
				currentWatch.Stop();
				currentWatch = null;
				currentWatchLabel = null;
			}
		}
	}
}