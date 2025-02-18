# Azure Container Apps

Azure Container Apps (ACA) is a managed container platform which abstracts away the infrastructure layer. You can run complex distributed applications with ACA with high-value features out of the box, including HTTPS provisioning, auto-scale, scale to zero, turnkey authentication, observability and more. ACA runs standard Docker container images - currently limited to Linux on Intel - and uses the power of Kubernetes under the hood, wrapped in a much simpler user experience.

In these exercises we'll start with a simple distributed app running across two containers and gradually extend it with ACA features, including end-user authentication, encrypted traffic between the components, and a simple approach to building and deploying apps from source.

There is **a lot** of content between this module and the [ACA - Security module](/modules/containers-aca-security/README.md). ACA is one of the more feature-rich services and you'll need to spend quite a bit of time with it to be confident of what it can do and how to use it.

## Reference

- [Implement Azure Container Apps](https://learn.microsoft.com/en-gb/training/modules/implement-azure-container-apps/) | Microsoft Learn

- [Container Apps documentation](https://docs.microsoft.com/en-gb/azure/container-apps/)

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

## Build and Deploy ACA from ACR

So far we've been using my public images to run containers. ACA also has support for building container images from source - like we did with Docker Compose in the [containers module](/modules/containers/README.md). 

ACA has Docker Compose support so you can run a cloud version of `docker compose build`. You can start from scratch to build and deploy an ACA app from source:

- the CLI creates an Azure Container Registry for the images
- ACR is used for the build - for each Compose service your local source is sent to Azure and the images are built and stored in ACR
- a Container Apps Environment is created
- a Container App for each service in the Compose file is created
- each Container App has a Secret created which contains a token to access ACR

This is a very quick way to prototype your app running in Azure. You can use the exact same Compose file you use locally:

- [docker-compose-build.yml](/src/rng/docker-compose-build.yml)

📋 Use a `containerapp compose` command to deploy a new version of the random number app from the Compose file `docker-compose-build.yml` in the folder `src/rng`. Make sure to use a new Container App Environment so you can run alongside your existing app.

<details>
  <summary>Not sure how?</summary>

There is only one `compose` command :)

```
az containerapp compose --help

az containerapp compose create --help

```

You need to be in the correct folder for the source, then run the `create` command with a new environment name, and the name of the Compose file:

```
cd src/rng

az containerapp compose create -g labs-aca --environment rng2 -f docker-compose-build.yml
```

</details><br/>

It will take a few minutes to create all the services, build the images and start the app. When it's done you can find the address of the web container:

```
az containerapp show -g labs-aca -n numbers-web --query  'properties.configuration.ingress.fqdn'
```

You can browse to check out the app - but think about the extra configuration we did for the previous deployment. Will the app work?

No, it doesn't work :)

---
🧭 Explore your new container apps to track down the problem:

- look at the ingress settings for the API container app - what is the URL?
- now check the environment variables for the web container app - does it match the API URL?
---

You'll see the web app has an environment variable set already which it copied from the Compose file, but the URL is wrong (it's configured for local running with Docker Compose). Also the API has external ingress set up which we don't want, it should be an internal component in ACA.


📋 Use `containerapp update` commands to get the new deployment working - set the API to use internal ingress, mapping HTTP port `80` to port `8080` inside the container, and change the environment variable to the correct URL in the web app.

<details>
  <summary>Not sure how?</summary>

These are the same steps we did with the other apps. Note the new names: `numbers-web` and `numbers-api`, which come from the Compose file service names.

```
az containerapp ingress update -g labs-aca -n numbers-api --type internal --transport http --target-port 8080 --allow-insecure true -o table

az containerapp update -g labs-aca -n numbers-web --set-env-vars "RngApi__Url=http://numbers-api/rng"
```

</details><br/>

Now your app is running correctly. This sort of post-deployment config cleanup is very common - but if your Compose model is configured differently you might be lucky and get your app working first time.

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