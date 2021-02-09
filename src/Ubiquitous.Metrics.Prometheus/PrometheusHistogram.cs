using System;
using System.Diagnostics;
using System.Linq;
using Prometheus;
using Ubiquitous.Metrics.Internals;
using Ubiquitous.Metrics.Labels;

namespace Ubiquitous.Metrics.Prometheus {
    class PrometheusHistogram : PrometheusMetric, IHistogramMetric {
        readonly Histogram _histogram;

        static readonly double[] DefaultBounds = {.002, .005, .01, .025, .05, .075, .1, .25, .5, .75, 1, 2.5, 5, 7.5, 10, 30, 60, 120, 180, 240, 300};

        public PrometheusHistogram(MetricDefinition metricDefinition, Label[]? defaultLabels, double[]? bounds = null) : base(defaultLabels)
            => _histogram = global::Prometheus.Metrics.CreateHistogram(
                metricDefinition.Name,
                metricDefinition.Description,
                new HistogramConfiguration {
                    Buckets    = bounds ?? DefaultBounds,
                    LabelNames = metricDefinition.LabelNames.SafeUnion(defaultLabels.GetLabelNames()).ToArray()
                }
            );

        public double Sum => _histogram.Sum;

        public long Count => _histogram.Count;

        public void Observe(Stopwatch stopwatch, LabelValue[]? labels = null, int count = 1)
            => CombineLabels(_histogram, labels).Observe(stopwatch.WatchSum(), count);

        public void Observe(DateTimeOffset when, params LabelValue[] labels)
            => CombineLabels(_histogram, labels).Observe((DateTimeOffset.UtcNow - when).TotalSeconds);
    }
}
