# Azure Container Apps

Azure Container Apps (ACA) is a managed container platform which abstracts away the infrastructure layer. You can run complex distributed applications with ACA with high-value features out of the box, including HTTPS provisioning, auto-scale, scale to zero, turnkey authentication, observability and more. ACA runs standard Docker container images - currently limited to Linux on Intel - and uses the power of Kubernetes under the hood, wrapped in a much simpler user experience.

## Reference

- [Implement Azure Container Apps](https://learn.microsoft.com/en-gb/training/modules/implement-azure-container-apps/) | Microsoft Learn

- [Container Apps documentation](https://docs.microsoft.com/en-gb/azure/container-apps/)

- [Container Apps YAML specification](https://learn.microsoft.com/en-us/azure/container-apps/azure-resource-manager-api-spec?tabs=yaml)

- [`az containerapp` commands](https://learn.microsoft.com/en-us/cli/azure/containerapp?view=azure-cli-latest)


## Explore Azure Container Apps

In the Portal create a new Container App. Explore the tabs for the new service; you can:

- select or create a new Container App Environment
- set the container details - the registry and image, amount of compute
- configure ingress - network communication into the app - can be internal or external

## Create an ACA container with the CLI

Create a new Resource Group for the lab, change the region if you like:

```
az group create -n labs-aca --tags course=az204 -l eastus
```

ACA is a relatively new serivce - ensure it's enabled for your subscription, and update your command line extension:

```
az provider register -n Microsoft.App --wait
az provider register -n Microsoft.OperationalInsights --wait
az extension add --name containerapp --upgrade
```

Now create a Container App _Environment_. This is a grouping of Container Apps which can communicate internally and share common features like secrets:

```
az containerapp env create --name rng -g labs-aca
```

> You'll see this also create a Log Analytics Workspace. This is a service for collecting, storing and querying log data and metrics. ACA automatically wires up container logs to write to Log Analytics.

Now you can use `az containerapp create` to create a new Container App in the environment.

📋 Create a Container App called `rng-api` using the image `ghcr.io/eltons-academy/rng-api:2025`. This is a REST API - you should create it to be externally available for HTTP traffic, mapping to port `8080` inside the container.

<details>
  <summary>Not sure how?</summary>

Start with the help:

```
az containerapp create --help
```

For a new Container App you need to specify the environment and image. To get public networking, specify external ingress and set the target port:

```
az containerapp create --name rng-api --environment rng -g labs-aca --image ghcr.io/eltons-academy/rng-api:2025 --target-port 8080 --ingress external
```

</details><br/>

When your Container App is running it will be allocated a public DNS name. You can query the resource to print just the domain name, and store the result in a variable:

```
$RNG_API = az containerapp show -n rng-api -g labs-aca --query 'properties.configuration.ingress.fqdn' -o tsv
```

This is a REST API which generates a random number. Test it with an HTTP request and you should see a number:

```
curl "https://$RNG_API/rng"
```

> Note the HTTPS - ACA provisions and applies a TLS cert along with the domain entry

The ACA archiecture has many layers - your container is one replica running in one revision in one app in the container app environment. But there are simple commands for everyday management tasks.

📋 Print the logs from your API container.

<details>
  <summary>Not sure how?</summary>

Start with the help to see what commands are available:

```
az containerapp --help
```

There is a `logs` command group:
```
az containerapp logs --help
```

You can specify the revision and replica to fetch the logs from, but the default will pick one replica from the active revision:

```
az containerapp logs show -n rng-api -g labs-aca
```

</details><br/>

This is just one part of the full solution, next we'll add a Web UI container.

## Connect Container Apps in an Environment

ACA is designed for distributed applications where each component runs in its own Container App in the same Environment. The service has features to ensure containers can communicate securely.


📋 Create a new Container App in the same Environment to run the web UI. Call it `rng-web` and use the image `ghcr.io/eltons-academy/rng-web:2025`. The container listens on port 8080 and it should be publicly available. Print the domain address so you can try the app.

<details>
  <summary>Not sure how?</summary>

This is pretty much the same command - only the Docker image and the name changes. If you add the `--query` parameter to the create command, the output will just be the field(s) you specify.

```
az containerapp create --name rng-web --environment rng -g labs-aca --image ghcr.io/eltons-academy/rng-web:2025 --target-port 8080 --ingress external --query properties.configuration.ingress.fqdn
```

</details><br/>

Browse to the domain and you should see a simple web page with a _Go_ button. Click the button and the web app tries to fetch a random number from the API.

> When you try it you'll see an error message. The URL the web app is using for the API is incorrect.

We need to change the config setting in the web app. These components are both .NET apps, using JSON files and enviroment variables for configuration. In a real app the configuration options would be documented somewhere, or you'd have to ask developers or dig into the source code. In this case there is a default setting in the Docker image.

```
# get your own copy of the image:
docker pull ghcr.io/eltons-academy/rng-web:2025

# inspect it to show the details
docker image inspect ghcr.io/eltons-academy/rng-web:2025
```

> The API URL which the web app is trying to use is set in the `RngApi__Url` environment variable.

📋 Print the details of the web Container App and look at the environment variables which have been set.

<details>
  <summary>Not sure how?</summary>

The usual `show` command prints all the resource details:

```
az containerapp show -g labs-aca -n rng-web
```

Environment variables are set in the templated container, in the `env` field:

```
az containerapp show -g labs-aca -n rng-web --query 'properties.template.containers[0].env'
```

</details><br/>

There are no environment variables set in the container, which means it's using the default from the Docker image.


📋 Update the web Container App to set an environment variable called `RngApi__Url` with the full path to the RNG API endpoint - including the HTTPS scheme and the `/rng` path.

<details>
  <summary>Not sure how?</summary>

Environment variables can be set when you create a Container App, or when you update it.

The `update` command help shows some useful examples:

```
az containerapp update --help
```

To set the environment variable run:

```
az containerapp update -g labs-aca -n rng-web --set-env-vars "RngApi__Url=https://$RNG_API/rng"
```

</details><br/>


wait:

```
az containerapp revision list -g labs-aca -n rng-web -o table
```

> try web again, OK

> browse to portal, check aca env & apps; revisions, scale & log stream

## Require auth

```
az containerapp auth show -g labs-aca -n rng-web
```

MSID needs app registration - more info to come in auth modules:

```
$APP_ID = az ad app create --display-name rng-web --enable-id-token-issuance true --web-redirect-uris https://rng-web.prouddesert-e1fd4b6f.westus2.azurecontainerapps.io/.auth/login/aad/callback
--query id -o tsv
```

get app reg client id:

```
az ad app show --id <app-id> # 14569933-000c-41b4-a7cc-e114cae7c9ca

$CLIENT_ID = az ad app show --id $APP_ID --query appId -o tsv
```

get tenant id:

```
$TENANT_ID = az account show --query tenantId -o tsv
```

register & require auth

```
az containerapp auth show -g labs-aca -n rng-web

az containerapp auth microsoft update -g labs-aca -n rng-web --client-id $CLIENT_ID --issuer "https://sts.windows.net/$TENANT_ID/"

az containerapp auth update -g labs-aca -n rng-web --redirect-provider azureactivedirectory --action RedirectToLoginPage
```


## Hide the API and use secrets for config

```
az containerapp ingress show -g labs-aca -n rng-api -o table

az containerapp ingress update -g labs-aca -n rng-api --type internal --transport http --target-port 8080 --allow-insecure true -o table
```

> fqdn changes to .internal. but we can use simple name

wait:

```
az containerapp revision list -g labs-aca -n rng-api -o table
```

> try app, fails again

```
az containerapp secret set -g labs-aca -n rng-web --secrets "rng-api-url=http://rng-api/rng"
```

> secret name != env var name, strict names can be remapped

> would need to do revision restart but we need to update which will do that anyway

```
az containerapp update -g labs-aca -n rng-web --set-env-vars "RngApi__Url=secretref:rng-api-url"

az containerapp revision list -g labs-aca -n rng-web -o table
```


## Dapr for mTLS 

- add mtls & service discovery with no code changes

register components:

```
az containerapp env dapr-component set -g labs-aca -n rng --dapr-component-name rng-web --yaml modules/containers-aca/dapr/rng-web-component.yaml
az containerapp env dapr-component set -g labs-aca -n rng --dapr-component-name rng-api --yaml modules/containers-aca/dapr/rng-api-component.yaml
```

enable dapr:

```
az containerapp dapr enable -g labs-aca -n rng-api --dapr-app-id rng-api --dapr-app-port 8080

az containerapp dapr enable -g labs-aca -n rng-web --dapr-app-id rng-web --dapr-app-port 8080 --dapr-enable-api-logging
```

set web to use dapr sidecar:

```
az containerapp update -g labs-aca -n rng-web --set-env-vars "RngApi__Url=http://localhost:3500/v1.0/invoke/rng-api/method/rng"

az containerapp revision list -g labs-aca -n rng-web -o table
```

- test

remove ingress from api - all coms via dapr:

```
az containerapp ingress disable -g labs-aca -n rng-api 
```

> can add resiliency policy for retries, timeout etc https://learn.microsoft.com/en-us/azure/container-apps/dapr-component-resiliency?tabs=cli



## Build and Deploy ACA from ACR


Compose integration:

- [docker-compose-build.yml](/src/rng/docker-compose-build.yml)

```
cd src/rng

az containerapp compose create -g labs-aca --environment rng2 -f docker-compose-build.yml
```

> creates loga, acr etc.

> ACR credentials stored as secret

```
az containerapp secret list -g labs-aca -n numbers-web 
```

```
az containerapp show -g labs-aca -n numbers-web --query 'properties.template.containers[0].env'
```

> already set from Compose file

```
az containerapp show -g labs-aca -n numbers-web --query  'properties.configuration.ingress.fqdn'
```

> try app, api call fails - defaults to external ingress

```
az containerapp ingress update -g labs-aca -n numbers-api --type internal --transport http --target-port 8080 --allow-insecure true -o table

az containerapp update -g labs-aca -n numbers-web --set-env-vars "RngApi__Url=http://numbers-api/rng"
```

# lab 

scale settings so api is always available; test environment - max 3 containers, minimum cpu - api & web will work with 0.1 of each

also set scale so up is triggered with conc 20 for api and conc 5 for web - test to verify up & down


> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

You can delete the RG for this lab to remove all the resources, including the registry and containers:

```
az group delete -y --no-wait -n labs-aca
```