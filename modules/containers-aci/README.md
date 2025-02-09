# Azure Container Instances

The great thing about Docker containers is they're portable - your app runs in the same way on Docker Desktop as it does on any other container runtime. Azure offers several services for running containers, and the simplest is Azure Container Instances (ACI) which is a managed container service. You run your apps in containers and you don't have to manage any of the underlying infrastructure.

## Reference

- [Run container images in Azure Container Instances](https://learn.microsoft.com/en-gb/training/modules/create-run-container-images-azure-container-instances/) | Microsoft Learn

- [Container Instances documentation](https://docs.microsoft.com/en-gb/azure/container-instances/)

- [ACI YAML specification](https://learn.microsoft.com/en-us/azure/container-instances/container-instances-reference-yaml)

- [`az container` commands](https://docs.microsoft.com/en-us/cli/azure/container?view=azure-cli-latest)


## Explore Azure Container Instances

Open the Portal and search to create a new Container Instance resource. Look at the options available to you:

- the image registry to use - it could be your own ACR instance or a public registry like Docker Hub
- the container image to run
- the compute size of your container - number of CPU cores and memory
- in the networking options you can publish ports and choose a DNS name to access your app
- in the advanced options you can set environment variables for the container

You can run Linux and Windows containers with ACI, so you can run new and old applications. The UX is the same - we'll see how the service works using the command line.

## Create an ACI container with the CLI

Start with a new Resource Group for the lab, using your preferred region:

```
az group create -n labs-aci --tags course=az204 -l eastus
```

Ensure your subscription is set up to use ACI:

```
az provider register --namespace Microsoft.ContainerInstance
```

Now you can use the `az container create` command to run ACI instances in the RG.

📋 Create a new container called `simple-web` to run the Linux image `ghcr.io/eltons-academy/simple-web:2025` (stored on GitHub Container Registry). Publish port `8080` and include a DNS name in your command so you'll be able to browse to the app running in the container. You will need to specify more values than you think :)

<details>
  <summary>Not sure how?</summary>

Start with the help:

```
az container create --help
```

You need to use the `image` and `ports` parameters, and pass a unique prefix for the `dns-name-label`. But ACI doesn't use defaults for the OS of the container or the amount of compute you want, so you need to set that too:

```
$DNS_LABEL='az204es001' # set your own here

az container create -g labs-aci --name simple-web --image ghcr.io/eltons-academy/simple-web:2025 --ports 8080 --os-type Linux --cpu 0.2 --memory 0.2 --dns-name-label $DNS_LABEL
```

</details><br/>

When the command returns, the new container is running. The output includes an `fqdn` field, which is the full DNS name you can use to browse to your container app.

ACI publishes ports but it does not do port mapping. The app is listening on port 8080, so the URL to your container is `http://<fqdn>:8080`

> Browse to the app. **It may take a couple of minutes to come online**. It's the same container image we built in the [Containers module](/module/containers/README.md).

You can configure a lot more details in the `container create` command. How much CPU and RAM did you set for your container? That can't be changed when the container is running, but you could replace this container with a new one from the same image and specify a different amount of compute.

Other `az container` commands let you manage your containerized apps. 

📋 Print the application logs from your ACI container.

<details>
  <summary>Not sure how?</summary>

```
az container logs -g labs-aci -n simple-web
```

</details><br/>

You'll see the ASP.NET application logs from the container.

## Setting application configuration

Using non-standard ports is fine for dev and test environments, but in production we need to use the HTTP standards (of course we should use HTTPS, but that's not provided by ACI out of the box).

The web application uses the .NET configuration system, and you can change the behaviour using environment variables:

- `ASPNETCORE_HTTP_PORTS` - to set the HTTP port the app listens on 
- `App__Environment` - sets whether the app is in dev, test, etc.

You can set environment variables when you create an ACI container, which lets us deploy a production version of the web app using the same Docker image.

📋 Update the container `simple-web` to run the  image `ghcr.io/eltons-academy/simple-web:2025`, setting the environment variables so the app listens on port `80` and uses the environment name `PROD`. Make sure you can access the app on a public URL.

<details>
  <summary>Not sure how?</summary>

The `container create` command will update an existing container if you use the same name.

You use the `--environment-variables` parameter to set the app configuration, you can pass multiple settings as key-value pairs. The rest of the command is the same, but you will need to publish port `80` and set a new DNS label.

```
$DNS_LABEL='az204es002' # set your own here

az container create -g labs-aci --name simple-web-prod --image ghcr.io/eltons-academy/simple-web:2025 --ports 80 --os-type Linux --cpu 0.2 --memory 0.2  --environment-variables ASPNETCORE_HTTP_PORTS=80 App__Environment=PROD --dns-name-label $DNS_LABEL
```

</details><br/>

> Browse to the Portal and find your ACI instance. The UI shows the lifecyle of the container, the configuration and the logs. You can also connect to a shell inside the container for troubleshooting.

## Build and run a Windows container

ACI is a multi-platform service - you can run images built for Linux or Windows. 

Windows containers are a whole different topic. You don't need to learn it in detail but it is useful to know that you can quickly spin up a Windows app in ACI. That's a good option if you need to proof-of-concept migrating a legacy app to Azure.

You need to be running Docker on a Windows machine to build a Windows container image. We'll use an ACR Task (which you learned about in the [ACR module](/modules/containers-acr/README.md) to build a simple app based on .NET Framework 4.8:

- [Dockerfile]() - builds the app on top on Microsoft's base image
- [index.aspx]() - this is the app code

Start by creating a registry in the resource group:

```
$ACR_NAME='<your-acr-name>'
az acr create -g labs-acr -l eastus --sku 'Basic' -n $ACR_NAME
```

📋 Build the image using ACR and call it `labs-aci/simple-web-windows`. You will need to give a name for your image and specify the path to the app folder, and also specify the platform to use.

<details>
  <summary>Not sure how?</summary>

Rememer the build command is very similar to `docker build`. You also need to set the ACR name and the platform:

```
az acr build --image labs-aci/simple-web-windows --registry $ACR_NAME --platform windows ./src/simple-web-windows
```

</details><br/>

When the build completes you will have a Windows image stored in ACR - you can browse to it in the Portal to check. Now you can use that image to run a Windows container with ACI.

>>> TO HERE

## Mount Azure Files as container storage

--azure-file-volume-account-name

## Deploy a multi-container app

- YAML with secureValue - secret
- can include (multiple) volume mounts in yaml

## Lab

- task with restart policy?
You can migrate all your .NET apps to containers, but you'll need to use Windows containers for older .NET Framework apps. Docker Desktop on Windows supports Linux and Windows containers (you can switch from the Docker icon in the taskbar), and so does ACI.

The [simple-web image](https://hub.docker.com/r/courselabs/simple-web/tags) has been published with Windows and Linux variants. Run an ACI container from the Windows image version, how does it differ from the Linux version? Then see what happens if you try to run the Linux image which has been compiled for ARM processors instead of Intel/AMD.

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

You can delete the RG for this lab to remove all the resources, including the containers you created with the Docker CLI:

```
az group delete -y --no-wait -n labs-aci
```

Now change your Docker context back to your local Docker Desktop, and remove the lab context:

```
docker context use default

docker context rm labs-aci
```