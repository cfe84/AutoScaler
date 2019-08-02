using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AutoScaler.Domain
{
    public class Scaler
    {
        IMetricAdapter metric;
        IScaleController controller;
        ScaleConfiguration configuration;
        ScaleCalculator calculator;
        public Scaler(
            ScaleConfiguration configuration,
            IMetricAdapter metric,
            IScaleController controller)
        {
            this.configuration = configuration;
            this.metric = metric;
            this.controller = controller;
            this.calculator = new ScaleCalculator(configuration);
        }

        private List<double> measurements = new List<double>();
        private object measurementsLock = new object();

        private async Task MonitorAsync()
        {
            while (true)
            {
                var measurement = await metric.GetMetricAsync();
                lock (measurementsLock)
                    measurements.Add(measurement);
                Thread.Sleep(configuration.PollingInterval);
            }
        }

        private async Task AdjustAsync()
        {
            while (true)
            {
                try
                {

                    Thread.Sleep(configuration.ScaleAdjustmentInterval);
                    var currentScale = await controller.GetCurrentSizeAsync();
                    var desiredScale = currentScale;
                    if (measurements.Count > 0)
                    {
                        lock (measurementsLock)
                        {
                            desiredScale = calculator.CalculateScale(measurements, currentScale);
                            measurements.Clear();
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine("No measurement found, skipping resizing");
                    }
                    await AdjustSize(currentScale, desiredScale);
                }
                catch (Exception exc)
                {
                    Console.Error.WriteLine($"Error in the adjustment loop: {exc.ToString()}");
                }
            }
        }

        private async Task AdjustSize(int currentScale, int desiredScale)
        {
            // This could be much smarter (e.g. using a PID) - that's not
            // too bad to begin with.
            if (desiredScale != currentScale)
            {
                Console.WriteLine($"Scaling from {currentScale} to {desiredScale}");
                await controller.ResizeAsync(desiredScale);
            }
        }

        public Task MonitorAndAdjustAsync()
        {
            return Task.WhenAll(MonitorAsync(), AdjustAsync());
        }
    }
}