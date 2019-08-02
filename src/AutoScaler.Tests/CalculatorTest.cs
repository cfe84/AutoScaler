using System;
using System.Linq;
using AutoScaler.Domain;
using Xunit;

namespace AutoScaler.Tests
{
    public class ScaleCalculatorTests
    {
        [Theory()]
        [InlineData(new[] { 2, 2, 3 }, 3)]
        [InlineData(new[] { 1, 2, 3 }, 2)]
        [InlineData(new[] { 3, 2, 3 }, 3)]
        [InlineData(new[] { 1, 2, 1 }, 2)]
        [InlineData(new[] { 0, 1, 1 }, 1)]
        public void ScaleCalculator_should_average(int[] input, int expectedResult)
        {
            // prepare
            var configuration = new ScaleConfiguration(
                minTargetValue: 1,
                maxTargetValue: 2,
                minScale: 1,
                maxScale: 3,
                ScaleConfiguration.MetricAggregationFunctionType.Average)
            {
                ScaleStrategy = ScaleConfiguration.ScaleStrategyType.ScaleByStep
            };
            var calculator = new ScaleCalculator(configuration);

            // execute
            var targetScale = calculator.CalculateScale(input.Select(intVal => (double)intVal), 2);

            // assess
            Assert.Equal(expectedResult, targetScale);
        }

        [Theory]
        [InlineData(2, new[] { 3.0, 3.0, 3.0 }, 2)]
        [InlineData(1, new[] { .0, .0, 1.0 }, 1)]
        public void ScaleCalculator_should_notgobeyondLimits(
            int currentScale,
            double[] measurements,
            int expected)
        {
            // prepare
            var minScale = 1;
            var maxScale = 2;

            var configuration = new ScaleConfiguration(
                minTargetValue: 1,
                maxTargetValue: 2,
                minScale: minScale,
                maxScale: maxScale,
                ScaleConfiguration.MetricAggregationFunctionType.Average)
            {
                ScaleStrategy = ScaleConfiguration.ScaleStrategyType.ScaleByStep
            };
            var calculator = new ScaleCalculator(configuration);

            // execute
            var targetScale = calculator.CalculateScale(measurements, currentScale);

            // assess
            Assert.Equal(expected, targetScale);
        }
    }
}
