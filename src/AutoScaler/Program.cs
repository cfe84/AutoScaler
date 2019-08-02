using System;
using System.Runtime.CompilerServices;
using AutoScaler.Domain;
using AutoScaler.Infrastructure;
using k8s;
using k8s.Models;
using Microsoft.AspNetCore.JsonPatch;

namespace StorageAutoScaler
{
    class Program
    {
        static void Main(string[] args)
        {
            string ENVIRONMENT = Environment.GetEnvironmentVariable("ENVIRONMENT");
            string CONNECTION_STRING = Environment.GetEnvironmentVariable("CONNECTION_STRING");
            string QUEUE_NAME = Environment.GetEnvironmentVariable("QUEUE_NAME");
            string REPLICAS_MIN = Environment.GetEnvironmentVariable("REPLICAS_MIN");
            string REPLICAS_MAX = Environment.GetEnvironmentVariable("REPLICAS_MAX");
            string TARGET_MAX = Environment.GetEnvironmentVariable("TARGET_MAX");
            string TARGET_MIN = Environment.GetEnvironmentVariable("TARGET_MIN");
            string NAMESPACE = Environment.GetEnvironmentVariable("NAMESPACE");
            string DEPLOYMENT = Environment.GetEnvironmentVariable("DEPLOYMENT");
            string POLLING_INTERVAL_S = Environment.GetEnvironmentVariable("POLLING_INTERVAL_S");
            string SCALE_INTERVAL_S = Environment.GetEnvironmentVariable("SCALE_INTERVAL_S");

            var metricAdapter = new StorageQueueMetricAdapter(CONNECTION_STRING, QUEUE_NAME);
            var k8sConnectionType = string.Equals("development", ENVIRONMENT, StringComparison.OrdinalIgnoreCase)
                ? KubernetesDeploymentScaleController.KubernetesConnectionType.ThroughProxy
                : KubernetesDeploymentScaleController.KubernetesConnectionType.InCluster;
            var k8sController = new KubernetesDeploymentScaleController(new KubernetesDeploymentScaleController.Configuration
            {
                ConnectionType = k8sConnectionType,
                DeploymentName = DEPLOYMENT,
                Namespace = NAMESPACE
            });

            var minTarget = string.IsNullOrWhiteSpace(TARGET_MIN) ? 10 : double.Parse(TARGET_MIN);
            var maxTarget = string.IsNullOrWhiteSpace(TARGET_MAX) ? 100 : double.Parse(TARGET_MAX);
            var minScale = string.IsNullOrWhiteSpace(REPLICAS_MIN) ? 2 : int.Parse(REPLICAS_MIN);
            var maxScale = string.IsNullOrWhiteSpace(REPLICAS_MAX) ? 10 : int.Parse(REPLICAS_MAX);

            var configuration = new ScaleConfiguration(
                minTarget, maxTarget,
                minScale, maxScale,
                ScaleConfiguration.MetricAggregationFunctionType.Average);

            if (!string.IsNullOrEmpty(POLLING_INTERVAL_S))
            {
                configuration.PollingInterval = TimeSpan.FromSeconds(int.Parse(POLLING_INTERVAL_S));
            }
            if (!string.IsNullOrEmpty(SCALE_INTERVAL_S))
            {
                configuration.ScaleAdjustmentInterval = TimeSpan.FromSeconds(int.Parse(SCALE_INTERVAL_S));
            }

            var scaler = new Scaler(configuration, metricAdapter, k8sController);
            scaler.MonitorAndAdjustAsync().Wait();
        }
    }
}
