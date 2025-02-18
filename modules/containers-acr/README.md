# Azure Container Registry

Open source applications are often published as container images on Docker Hub. Services which host images are called container registries, and you'll want to use a private registry for your own apps rather than a public one. Azure Container Registry is the service you use to create and manage your own registry. It integrates with Azure security and lets you store images in the same region as the service where you'll run containers.

## Reference

- [Manage container images in Azure Container Registry](https://learn.microsoft.com/en-gb/training/modules/publish-container-image-to-azure-container-registry/) | Microsoft Learn

- [Docker Hub overview](https://docs.docker.com/docker-hub/)

- [Container Registry documentation](https://docs.microsoft.com/en-gb/azure/container-registry/)

- [ACR Tasks documentation](https://learn.microsoft.com/en-gb/azure/container-registry/container-registry-tasks-overview)

- [`az acr` commands](https://docs.microsoft.com/en-us/cli/azure/acr?view=azure-cli-latest)


## Explore ACR in the Portal

Open the Portal and search to create a new Container Registry resource. Switch through the different SKUs and look at the options you have:

- private networking and customer-managed encryption keys are available with the Premium SKU
- the registry name becomes the DNS name, with the `.azurecr.io` suffix, so it needs to be globally unique.

Back to the terminal now to create a registry with the command line.

## Create an ACR instance with the CLI

Start with a new Resource Group for the lab, using your preferred region:

```
az group create -n labs-acr --tags course=az204 -l eastus
```

Ensure your subscription is set up to use ACR:

```
az provider register --namespace Microsoft.ContainerRegistry
```

You'll need a unique name for your ACR instance. Store it in a variable for later use:

```
# with PowerShell:
$ACR_NAME='<your-acr-name>'

# OR with Linux shells:
ACR_NAME='<your-acr-name>'
```

> I'm using PowerShell and my ACR name is _az204es001_, so I'll run `$ACR_NAME='az204es001'`

📋 Create a new registry with the `acr create` command, using the `ACR_NAME` variable for the name.

<details>
  <summary>Not sure how?</summary>

Start with the help:

```
az acr create --help
```

There are a lot more options than you see in the Portal. If you do use the Portal to create a registry you can set these in the management page.

This creates a Basic-SKU registry:

```
az acr create -g labs-acr -l eastus --sku 'Basic' -n $ACR_NAME
```

ACR names are stricter than most. You will get a `ResourceNameInvalid` error if you try to use an illegal character, or an `AlreadyInUse` error if that name is taken.

</details><br/>

When the command completes you have your own registry, available at the domain name `<your-acr-name>.azurecr.io` - you'll see the full name in the `loginServer` field in the output.

---
🧭 Explore ACR in the Azure Portal from the [Container Registry list](https://portal.azure.com/#browse/Microsoft.ContainerRegistry%2Fregistries). Check through the blades - here are some of the key things to note:

- _Settintgs...Properties_ for pricing plan and admin user
- _Services...Geo-replications_ to add regional replicated ACR instances 
- _Repository Permissions...Tokens_ for fine-grained access to non-Azure users
- _Monitoring...Metrics_ to graph storage used for your images
---

## Pull and push images to ACR

Docker image names can include a registry domain. The default registry is Docker Hub (`docker.io`) so you don't need a domain for that - the full name for the image `nginx:alpine` is actually `docker.io/nginx:alpine`.

Pulling an image downloads the latest version:

```
docker image pull docker.io/nginx:alpine
```

You can upload a copy of that image to ACR, but you need to change the name to use your ACR domain instead of Docker Hub. The `tag` command does that:

```
docker image tag docker.io/nginx:alpine "$ACR_NAME.azurecr.io/labs-acr/nginx:alpine-az204"
```

> You can change all parts of the image name with a new tag.

Now you have two tags for the Nginx image:

```
docker image ls --filter reference=nginx --filter reference='*/labs-acr/nginx'
```

Your ACR tag and the Docker Hub tag both have the same image ID; tags are like aliases and one image can have many tags.

You upload images to a registry with the `push` command, but first you need to authenticate.

_Try pushing your image to ACR:_

```
# this will fail:
docker image push "$ACR_NAME.azurecr.io/labs-acr/nginx:alpine-az204"
```

📋 You can authenticate to the registry with your Azure account. Log in with an `az acr` command and then push the image.

<details>
  <summary>Not sure how?</summary>

List the ACR commands:

```
az acr --help
```

You'll see there's a `login` command which just needs your ACR name:

```
az acr login -n $ACR_NAME
```

Now when you push your image it will upload:

```
docker image push "$ACR_NAME.azurecr.io/labs-acr/nginx:alpine-az204"
```

</details><br/>

You can run a container from that image with this command:

```
docker run -d -p 8080:80 "$ACR_NAME.azurecr.io/labs-acr/nginx:alpine-az204"
```

You can browse the app at http://localhost:8080. It's the standard Nginx app, but it's available from your own image registry. Anyone who has access to your ACR can run the same app from the image.

## Import an image 

You will use ACR to store your own application images and also any third-party images which you want to have control over. 

Pushing and pulling images from another registry can be scripted with the `docker` commands, but ACR has a shortcut. The `import` command loads an image into your ACR instance, and the pushing and pulling all happens in Azure.

📋 Import this image from GitHub into your ACR: `ghcr.io/eltons-academy/nginx:alpine-2025`. You can choose your own target image name.

<details>
  <summary>Not sure how?</summary>

Print the help:

```
az acr import --help
```

You need to provide the name of your registry, the full reference of the image you want to import, and the target image name:

```
az acr import -n $ACR_NAME --source ghcr.io/eltons-academy/nginx:alpine-2025 --image library/nginx:alpine-az204
```

</details><br/>

You should have two images in ACR now. You can list the repositories:

```
az acr repository list --name $ACR_NAME --output table
```

And the tags for a repository:

```
az acr repository show-tags -n $ACR_NAME --repository labs-acr/nginx
```

The default output is a JSON array of the image tags.

> Image tags are just labels but they are usually used to identify the version of the application in the image.

## Build and push your own image

You can build images on your machine and push them to ACR with the Docker command line, but ACR can build your image for you.

The `acr build` command works like `docker build`, except that it sends the folder with your Dockerfile and source code to Azure and provisions some compute to do the build for you.

You'll see all the usual build ouput, but the commands are running in Azure with an ACR Task. When the build completes the image gets pushed to ACR.

There's a Hello World application in the folder `src/hello-azure`:

- [Dockerfile](src/hello-azure/Dockerfile) - runs a script to print some text

📋 Build the image using ACR. You will need to give a name for your image and specify the path to the app folder.

<details>
  <summary>Not sure how?</summary>

Print the help:

```
az acr build --help
```

The build command is very similar to `docker build` - you also need to set the ACR name:

```
az acr build --image labs-acr/hello-azure --registry $ACR_NAME ./src/hello-azure
```

</details><br/>

> By default ACR will build an image targeted for Linux on Intel machines. Docker is multi-platform and you can specify ACR to target  Windows or Arm.

You should find the build completes very quickly. ACR queues the build task and it runs on an agent from a pool. There is usually lots of capacity in the pool (and you have the option to create your own agent pools).

---
🧭 Open your ACR service in the Portal, now you have some images stored. Look through:

- _Services...Repositories_ for your image lists
- _Services...Tasks_ for scheduled tasks and runs
- _Services...Webhooks_ to create webhook notifications from repository events
---

## Run a one-off container

You can check your ACR build worked by running a container locally:

```
docker run "${ACR_NAME}.azurecr.io/labs-acr/hello-azure"
```

There is also the `acr run` command which lets you run a one-off container as an ACR task:

```
az acr run -r $ACR_NAME --cmd "${ACR_NAME}.azurecr.io/labs-acr/hello-azure" /dev/null
```

> The syntax looks odd because you can actually upload a source code folder and send the input to run command.

These container runs are not the same as ACR Tasks. Tasks are for building and pushing images, they can be scheduled or triggered from a GitHub pull request, or from a change to the base image (see the [ACR Tasks samples](https://github.com/Azure-Samples/acr-tasks) on GitHub).

## Lab

If you use containers with Azure you might have a CI job which builds and pushes images to ACR every time there's a code change. You're charged for storage with ACR so you might want a script that can clean up old images on a schedule.

Look at how you can delete images with the `az` command, and if scripting is your thing see if you can write a script which will delete all but the 5 most recent image versions.

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

You can delete the RG for this lab to remove all the Azure resources, including your ACR instance and its images:

```
az group delete -y --no-wait -n labs-acr
```

And run this command to remove all your local Docker containers:

```
docker rm -f $(docker ps -aq)
```