# Azure Setup

There are many ways to work with Azure but the [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/what-is-azure-cli) is the most user-friendly and well documented.

As well as Azure services we'll be running containers locally with [Docker](https://www.docker.com/).

We'll also use [Git](https://git-scm.com) to download the lab content, so you'll need a client on your machine to talk to GitHub.

## Git Client - Mac, Windows or Linux

Git is a free, open source tool for source control:

- [Install Git](https://git-scm.com/downloads)

## Azure Subscription

You'll need your own Azure Subscription, or one which you have _Owner_ permissions for:

- [Create a free subscription with $200 credit](https://azure.microsoft.com/en-gb/free/)

## Azure Command Line - Mac, Windows or Linux

The `az` command is a cross-platform tool for managing Azure resources:

- [Install the Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)

## .NET - Mac, Windows or Linux

We'll us C# for simple demo applications:

- [Download .NET SDK](https://dotnet.microsoft.com/en-us/download)

## Docker Desktop - Mac, Windows or Linux

Docker Desktop is for running containers locally and gives you a Kubernetes environment:

- [Install Docker Desktop - Mac or Windows](https://www.docker.com/products/docker-desktop/)

- [Install Docker Desktop - Linux ](https://docs.docker.com/desktop/setup/install/linux/)

The download and install takes a few minutes. When it's done, run the _Docker_ app and you'll see the Docker whale logo in your taskbar (Windows) or menu bar (macOS).

> On Windows the install may need a restart before you get here.

## Check your setup

When you're done you should be able to run these commands and get a response with no errors:

```
git version

az --version

dotnet --list-sdks

docker version
```

> Don't worry about the actual version numbers, but if you get errors then you'll need to look at the installs again.