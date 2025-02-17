# Azure Container Apps

Azure Container Apps (ACA) is a managed container platform which abstracts away the infrastructure layer. You can run complex distributed applications with ACA with high-value features out of the box, including HTTPS provisioning, auto-scale, scale to zero, turnkey authentication, observability and more. ACA runs standard Docker container images - currently limited to Linux on Intel - and uses the power of Kubernetes under the hood, wrapped in a much simpler user experience.

In thexe exercises we'll start with a simple distributed app running across two containers and gradually extend it with ACA features, including end-user authentication, encrypted traffic between the components, and a simple approach to building and deploying apps from source.

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

You'll see this also create a Log Analytics Workspace. This is a service for collecting, storing and querying log data and metrics. ACA automatically wires up container logs to write to Log Analytics.

---
🧭 Explore your environment in the Azure Portal - from the [ACA environments list](https://portal.azure.com/#browse/Microsoft.App%2FmanagedEnvironments). Here are some key points:

- _Overview_ shows the static IP address for the environment
- _Settings...Workload profiles_ to create a reusable compute profile
- _Ingress...Custom DNS suffix_ to have your own DNS name instead of a random one
- _Apps_ lists all the container apps in the environment

---

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
$RNG_API=$(az containerapp show -n rng-api -g labs-aca --query 'properties.configuration.ingress.fqdn' -o tsv)
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

---
🧭 Explore your container app in the Azure Portal - from the [ACA list](https://portal.azure.com/#browse/Microsoft.App%2FcontainerApps). Here are some key points:

- _Overview_ for a quick view of properties and monitoring graphs
- _Application...Revisions and replicas_ to drill into revisions and running containers
- _Application...Containers_ to see the container setup including environment variables and health probes
- _Settings...Deployment_ to configure CI/CD from a Git repo
- _Settings...Development stack_ to opt in to language-based features
- _Monitoring...Log stream_ to print live logs from a container
- _Monitoring...Console_ to connect to a shell session in a container

---

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


When you make a change to a container app it gets implemented as a new revision, and when the replica(s) in the revision are ready then ingress traffic is shifted to them. Check the revision list to see when the new revision is receiving all traffic:

```
az containerapp revision list -g labs-aca -n rng-web -o table
```

> Now if you try the web app again, it will connect to the API and work correctly.

---
🧭 Explore your web container app in the Azure Portal and open _Application...Revisions and replicas_

- you may see two active revisions, or one may be inactive
- switch the _Revision mode_ to _Multiple_ and you can activate both revisions
- with more than one active revision you can split the traffic between them
- switch back to _Single_ revision mode
---

## Enable authentication for the web app

ACA supports turnkey authentication and authorization. You can integrate with multiple identity providers - including Microsoft, GitHub, Apple, Google and OpenID Connect. 

You don't need any code changes to add authentication to your app; the auth flow sits before your application and only sends requests when users have a validated token from a supported identity provider.

Auth is not enabled by default:

```
az containerapp auth show -g labs-aca -n rng-web
```

We'll integrate with Microsoft's identity provider. To do that we need to create a few identity resources - these will get their own module later, so don't worry too much about what's happening here.l

```
# store your web app's callback URL:
$RNG_WEB=$(az containerapp show -n rng-web -g labs-aca --query 'properties.configuration.ingress.fqdn' -o tsv)
$WEB_CALLBACK_URL="https://$RNG_WEB/.auth/login/aad/callback"

# create an app registration to allow the web app to use Microsoft ID:
$APP_ID=$(az ad app create --display-name rng-web --enable-id-token-issuance true --web-redirect-uris $WEB_CALLBACK_URL --query id -o tsv)
```

Now we have the ID for the app registration, we need a couple of other bits of information:

```
# store the app client ID:
$CLIENT_ID = az ad app show --id $APP_ID --query appId -o tsv

# and the subscription's tenant ID:
$TENANT_ID = az account show --query tenantId -o tsv
```

That's everything we need, but this next bit might take some figuring out...

📋 Add Microsoft auth to the web container app, using the client ID from the app registration and the tenant ID as your token issuer. Then update the container app to require authentication.

<details>
  <summary>Not sure how?</summary>


I don't blame you :) 

You can start digging into the docs:

```
az containerapp auth --help

# which will lead you to:
az containerapp auth microsoft --help

# and:
az containerapp auth update --help
```

Put it all together:

```
# configure Microsoft as an identity provider:
az containerapp auth microsoft update -g labs-aca -n rng-web --client-id $CLIENT_ID --issuer "https://sts.windows.net/$TENANT_ID/"

# requre auth for the app:
az containerapp auth update -g labs-aca -n rng-web --redirect-provider azureactivedirectory --action RedirectToLoginPage
```

</details><br/>

---
🧭 Explore your web container app in the Azure Portal and open _Settings...Authentication_

- you should see that auth is required and unauthenticated users get redirected to the login page
- click _Edit_ to see how you can alter the auth requirements
- Microsoft is configured as an identity provider
- click _Add Provider_ and you get a guided experience for adding another identity provider
---

Browse to your app in a private browser window and you will be redirected to Microsoft's login page. When you authenticate you see the same app - there's nothing in the app code or config to support authentication.

## Restrict API access and use secrets for config

We're progressively making our app more production ready. Right now the UI requires authentication, but the API is publicly available. For this app we want the API to be an internal component, only accessible to the web app.

ACA lets you configure ingress to be internal so communication is restricted to apps in the same container environment.

Print the current ingress setup for the API app:

```
az containerapp ingress show -g labs-aca -n rng-api -o table
```

External ingress with secure transport is what gives the API a public HTTPS URL.

📋 Update the API container app to use internal ingress over plain HTTP. How does the FQDN change?

<details>
  <summary>Not sure?</summary>

The `ingress update` command is the one we need:

```
az containerapp ingress update --help
```

Setting ingress to internal doesn't automatically do the other things we need, so we need to explicitly set the transport, the security flag and the target port:

```
az containerapp ingress update -g labs-aca -n rng-api --type internal --transport http --target-port 8080 --allow-insecure true -o table
```

The DNS name changes to include `.internal`. Now we need to change the web app configuration again to use the new URL - but we don't need the FQDN. Container apps within the same environment can access each other using just the app name.

</details><br/>

We could update the environment variable we set earlier to use the new URL, but let's say this is sensitive data so we want to store it in a secret instead.

📋 Create a secret in the web container app, called `rng-api-url` with the value `http://rng-api/rng`.

<details>
  <summary>Not sure how?</summary>

Secrets have their own command group:

```
az containerapp secret --help

az containerapp secret set --help
```

Note that secret names are very strict, they can only include lowercase letters, numbers and hyphens:

```
az containerapp secret set -g labs-aca -n rng-web --secrets "rng-api-url=http://rng-api/rng"
```

The output state the app needs to be restarted - but we're going to make an update which will cause a new revision anyway.

</details><br/>

Secrets are created at the container app level and they live outside of any revisions (unlike environment variables which are part of the revision). But secrets aren't automatically mapped into containers, you need to update the container app to explicitly include the secret value as an environment variable.

📋 Update the container web app to use the value from the secret `rng-api-url` in the environment variable `RngApi__Url`. Does the change happen immediately?

<details>
  <summary>Not sure?</summary>

We've already run a similar command to set environment variables, but the documentation explains how to link a variable to a secret:

```
az containerapp update --help
```

You include `secretref:` to load a named environment variable from a secret in the container app:

```
az containerapp update -g labs-aca -n rng-web --set-env-vars "RngApi__Url=secretref:rng-api-url"
```

This creates a new revision so you need to wait for it to roll out before the change is ready.

</details><br/>

Try the app and it should be working again - but now the front end and back end are secured with appropriate controls.

---
🧭 Explore your container apps in the Azure Portal.

- in the web app check _Settings...Secrets_ - you can view and edit the secret
- in _Application...Containers_ you will see the environment variable references the secret
- in the API app check _Settings...Ingress_ to see how the internal setup is shown
---

## Dapr for mTLS

There's one other big feature of ACA: integration with the [Distributed Application Runtime (Dapr)](https://dapr.io). Dapr is a CNCF project which provides building blocks for implementing distributed applications, with features like service discovery, asynchronous messaging, workflows and more. 

>> HERE

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

## Lab 

scale settings so api is always available; test environment - max 3 containers, minimum cpu - api & web will work with 0.1 of each

also set scale so up is triggered with conc 20 for api and conc 5 for web - test to verify up & down


> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

You can delete the RG for this lab to remove all the resources, including the registry and containers:

```
az group delete -y --no-wait -n labs-aca
```