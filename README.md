This is an autoscaler for Kubernetes deployments built in C#. Based
on a metric that can be extended (out of the box, it's using Azure Storage Queues
message count), it will scale the deployment replica count up or down.

It's mainly intended for demo purposes and can be used
at your own risk.

# Deployment

Build the docker image and push it to a container registry.

There is a chart to deploy the service, use the following command to deploy, look at the `values.yaml` file in the chart to see the values you can override

```sh
helm template chart --set connectionString="$CONNECTION_STRING" --set namespace="messages" --set deployment="messages-consumer" --set imageName="chfevcr.azurecr.io/autoscaler" | kubectl apply -f -
```


# Configuration

The application is using the following environment variable to run:

- `ENVIRONMENT`: `Development` will use a channel opened by 
    `kubectl proxy`, `Production` will use in-cluster connection.
- `NAMESPACE`: Namespace in which the deployment is provisioned
- `DEPLOYMENT`: Name of the deployment to scale
- `REPLICAS_MIN`: Minimum number of replicas. Auto-scaler will never 
    attempt to scale the deployment below that number.
- `REPLICAS_MAX`: Maximum number of replicas. Auto-scaler will never 
    attempt to scale the deployment above that number.
- `TARGET_MAX`: Maximum value for the target range. When the actual
    measurement is above this value, the autoscaler will attempt to 
    scale up.
- `TARGET_MIN`: Minimum value for the target range. When the actual
    measurement is below this value, the autoscaler will attempt to 
    scale down.
- `POLLING_INTERVAL_S`: Interval at which the auto-scaler will poll
    the metric used for scaling, measured in seconds. The default
    is 10 seconds
- `SCALE_INTERVAL_S`: Interval at which the auto-scaler will decide
    to scale up or down, measured in seconds. Default is 1 minute.
    By default auto-scaler uses an average of all the metrics collected
    since the last iteration (so, by default, it will average the
    last 6 values).

Specific to Azure Storage Queues:
- `CONNECTION_STRING`: Connection string to the storage account in
    which the queue to monitor is created.
- `QUEUE_NAME`: Name of the queue to monitor