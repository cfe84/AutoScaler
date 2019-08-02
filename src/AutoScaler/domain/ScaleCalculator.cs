using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoScaler.Domain
{
    public class ScaleCalculator
    {
        ScaleConfiguration configuration;
        public ScaleCalculator(ScaleConfiguration configuration)
        {
            this.configuration = configuration;
        }

        private double calculateReferenceValue(IEnumerable<double> measurements)
        {
            switch (configuration.MetricAggregationFunction)
            {
                case ScaleConfiguration.MetricAggregationFunctionType.Average:
                    return measurements.Average();
                case ScaleConfiguration.MetricAggregationFunctionType.Max:
                    return measurements.Max();
                case ScaleConfiguration.MetricAggregationFunctionType.Min:
                    return measurements.Min();
                case ScaleConfiguration.MetricAggregationFunctionType.Total:
                    return measurements.Sum();
                default:
                    throw new System.Exception("Unknown aggregation function: " + configuration.MetricAggregationFunction);
            }
        }

        private int CalculateTargetScale()
        {
            throw new NotImplementedException("Scaling to target is not supported yet");
        }
        private int IncrementScale(int currentScale)
        {
            if (configuration.ScaleStrategy == ScaleConfiguration.ScaleStrategyType.ScaleByStep)
            {
                return currentScale + 1;
            }
            else
            {
                return CalculateTargetScale();
            }
        }
        private int DecrementScale(int currentScale)
        {
            if (configuration.ScaleStrategy == ScaleConfiguration.ScaleStrategyType.ScaleByStep)
            {
                return currentScale - 1;
            }
            else
            {
                return CalculateTargetScale();
            }
        }

        public int CalculateScale(IEnumerable<double> measurements, int currentScale)
        {
            var referenceValue = calculateReferenceValue(measurements);
            var target = currentScale;

            if (referenceValue > configuration.TargetValueRange.Max
                && currentScale < configuration.ScaleRange.Max)
                target = IncrementScale(currentScale);

            if (referenceValue < configuration.TargetValueRange.Min
                && currentScale > configuration.ScaleRange.Min)
                target = DecrementScale(currentScale);
            return target;
        }
    }
}