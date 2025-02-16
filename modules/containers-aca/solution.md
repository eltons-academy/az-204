# Lab Solution


Default settings:

- scale 0-10
- cpu 0.5
- memory 1GB

Only certain CPU & memory combinations are allowed. Try to set the API to 0.2:

```
az containerapp update -g labs-aca -n numbers-api --cpu 0.2 --memory 0.2
```

Fails. Needs to be (memory = 2* cpu):

```
az containerapp update -g labs-aca -n numbers-api --min-replicas 1 --max-replicas 2 --cpu 0.25 --memory 0.5 --scale-rule-http-concurrency 20
```

Update web:

```
az containerapp update -g labs-aca -n numbers-web --min-replicas 1 --max-replicas 3 --cpu 0.75 --memory 1.5 --scale-rule-http-concurrency 5
```

Throw in some load with [Fortio](https://fortio.org) (need Docker Desktop running):

```
docker run fortio/fortio load -qps 20 -c 10 -t 2m https://numbers-web.yellowwater-d00cd992.westus2.azurecontainerapps.io
```


Check metrics for replica count in Portal:

![](/img/containers-aca/lab-metrics-replicas.png)


> [Back to the lab](README.md#lab)
