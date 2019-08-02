using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Microsoft.AspNetCore.JsonPatch;
using AutoScaler.Domain;
using System;

namespace AutoScaler.Infrastructure
{
    public class KubernetesDeploymentScaleController : IScaleController
    {
        public class Configuration
        {
            public KubernetesConnectionType ConnectionType { get; set; }
            public string Host { get; set; } = "http://127.0.0.1:8001";
            public string Namespace { get; set; }
            public string DeploymentName { get; set; }
        }

        public enum KubernetesConnectionType
        {
            InCluster,
            ThroughProxy
        }

        private Kubernetes client;
        private Configuration configuration;
        public KubernetesDeploymentScaleController(Configuration configuration)
        {
            this.configuration = configuration;
            KubernetesClientConfiguration config;
            if (configuration.ConnectionType == KubernetesConnectionType.InCluster)
            {
                Console.WriteLine($"Loading in-cluster config");
                config = KubernetesClientConfiguration.InClusterConfig();
            }
            else
            {
                config = new KubernetesClientConfiguration { Host = configuration.Host };
            }
            client = new Kubernetes(config);
            if (client.ReadNamespacedDeployment(
                configuration.DeploymentName,
                configuration.Namespace) != null)
            {
                Console.WriteLine($"Successfuly connected to deployment {configuration.DeploymentName}");
            }
            else
            {
                Console.Error.WriteLine($"Error: Couldn't retrieve status for deployment {configuration.DeploymentName}");
            }
        }
        public async Task<int> GetCurrentSizeAsync()
        {
            var scale = await client.ReadNamespacedDeploymentScaleAsync(
                configuration.DeploymentName,
                configuration.Namespace);
            return scale.Spec.Replicas.Value;
        }

        public async Task ResizeAsync(int instances)
        {
            var patch = new JsonPatchDocument<V1Deployment>();
            patch.Replace((e) => e.Spec.Replicas, instances);
            var body = new V1Patch(patch);
            await client.PatchNamespacedDeploymentScaleAsync(
                body,
                configuration.DeploymentName,
                configuration.Namespace);
        }
    }
}