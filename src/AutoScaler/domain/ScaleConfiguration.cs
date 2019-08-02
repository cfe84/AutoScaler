using System;

namespace AutoScaler.Domain
{
    public class ScaleConfiguration
    {
        public class Range<T>
        {
            public Range(T min, T max)
            {
                Min = min;
                Max = max;
            }
            public T Min { get; set; }
            public T Max { get; set; }
        }
        public enum MetricAggregationFunctionType
        {
            Average,
            Min,
            Max,
            Total
        }

        public enum ScaleStrategyType
        {
            ScaleByStep,
            ScaleQuickly
        }

        public ScaleConfiguration(
            double minTargetValue,
            double maxTargetValue,
            int minScale,
            int maxScale,
            MetricAggregationFunctionType aggregationFunction)
        {
            TargetValueRange = new Range<double>(minTargetValue, maxTargetValue);
            ScaleRange = new Range<int>(minScale, maxScale);
            MetricAggregationFunction = aggregationFunction;
        }

        public IMetricAdapter Metric { get; set; }
        public Range<int> ScaleRange { get; set; }
        public IScaleController ScaleController { get; set; }
        public Range<double> TargetValueRange { get; set; }
        public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(10);
        public MetricAggregationFunctionType MetricAggregationFunction { get; set; }
        public TimeSpan ScaleAdjustmentInterval { get; set; } = TimeSpan.FromMinutes(1);
        public ScaleStrategyType ScaleStrategy { get; set; } = ScaleStrategyType.ScaleByStep;
    }
}