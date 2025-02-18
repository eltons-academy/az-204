# AZ-204: Acing the Azure Developer Associate Exam 

Welcome to _Acing AZ-204_. 

This is where you find the exercises for the training course at [www.eltons.academy/az-204](https://www.eltons.academy/az-204).

These are hands-on resources to help you **really learn Azure** and **ace** the AZ-204 exam.

## Reference

- [AZ-204 Skills Measured (15 January 2025)](https://learn.microsoft.com/en-us/credentials/certifications/resources/study-guides/az-204#skills-measured-as-of-january-15-2025)

- [AZ-204 Learning Path](https://learn.microsoft.com/en-us/training/courses/az-204t00) | Microsoft Learn

- [Azure Developer Associate Certification](https://learn.microsoft.com/en-us/credentials/certifications/azure-developer/?source=recommendations&practice-assessment-type=certification)

## Pre-reqs

 - Create an Azure account (there is a [free option](https://azure.microsoft.com/en-in/pricing/free-services/))
 - [Set up the AZ command line, Git and Docker](./setup/README.md) 
 - Download your repo
    - Open a terminal (PowerShell on Windows; any shell on Linux/macOS) 
    - Run: `git clone https://github.com/eltons-academy/az-204.git`
     - Open the folder: `cd az-204`
- _Optional_
    - Install [Visual Studio Code](https://code.visualstudio.com) (free - Windows, macOS and Linux) - it's the best way to browse the repo and documentation
    - Install [PowerShell](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell) (free - Windows, macOS and Linux) - for the best experience following scripts in the exercises

## 0. Do this first :)

| # | Skill | Modules |
|-|-|-|
| 0.1 | Sign in and explore the Azure tools | [intro-tools](/modules/intro-tools/README.md) |
| 0.2 | Understand Resource Groups | [intro-resourcegroups](/modules/intro-resourcegroups/README.md) |

## Block 1. Develop Azure compute solutions (25–30%)

### Topic 1.1 Implement containerized solutions

| # | Skill | Modules |
|-|-|-|
| 1.1.1 | Create and manage container images for solutions| [containers](/modules/containers/README.md) |
| 1.1.2 | Publish an image to Azure Container Registry | [containers-acr](/modules/containers-acr/README.md) |
| 1.1.3 | Run containers by using Azure Container Instance | [containers-aci](/modules/containers-aci/README.md) |
| 1.1.4 | Create solutions by using Azure Container Apps | [containers-aca](/modules/containers-aca/README.md) |
|  |  | [containers-aca-security](/modules/containers-aca-security/README.md) |

### Topic 1.2 Implement Azure App Service Web Apps

| | |
|-|-|
| 1.1.1 | [Create an Azure App Service Web App](/modules/appservice/README.md) |
- Configure and implement diagnostics and logging
- Deploy code and containers
- Configure settings including Transport Layer Security (TLS), API settings, and service connections
- Implement autoscaling
- Configure deployment slots

### Implement Azure Functions

- Create and configure an Azure Functions app
- Implement input and output bindings
- Implement function triggers by using data operations, timers, and webhooks

## Develop for Azure storage (15–20%)

## Develop solutions that use Azure Cosmos DB

- Perform operations on containers and items by using the SDK
- Set the appropriate consistency level for operations
- Implement change feed notifications

## Develop solutions that use Azure Blob Storage

- Set and retrieve properties and metadata
- Perform operations on data by using the appropriate SDK
- Implement storage policies and data lifecycle management

## Implement Azure security (15–20%)

### Implement user authentication and authorization
- Authenticate and authorize users by using the Microsoft Identity platform
- Authenticate and authorize users and apps by using Microsoft Entra ID
- Create and implement shared access signatures
- Implement solutions that interact with Microsoft Graph

### Implement secure Azure solutions

- Secure app configuration data by using App Configuration or Azure Key Vault
- Develop code that uses keys, secrets, and certificates stored in Azure Key Vault
- Implement Managed Identities for Azure resources


## Monitor, troubleshoot, and optimize Azure solutions (10–15%)

### Implement caching for solutions

- Configure cache and expiration policies for Azure Cache for Redis
- Implement secure and optimized application cache patterns including data sizing, connections, encryption, and expiration
- Implement Azure Content Delivery Network endpoints and profiles

### Troubleshoot solutions by using Application Insights

- Monitor and analyze metrics, logs, and traces
- Implement Application Insights web tests and alerts
- Instrument an app or service to use Application Insights

## Connect to and consume Azure services and third-party services (20–25%)

### Implement API Management

- Create an Azure API Management instance
- Create and document APIs
- Configure access to APIs
- Implement policies for APIs

### Develop event-based solutions

- Implement solutions that use Azure Event Grid
- Implement solutions that use Azure Event Hub

### Develop message-based solutions

- Implement solutions that use Azure Service Bus
- Implement solutions that use Azure Queue Storage queues

_Resource Groups and Virtual Machines_

- [Signing In](/labs/signin/README.md)
- [Regions and Resource Groups](/labs/resourcegroups/README.md)
- [Virtual Machines](/labs/vm/README.md)
- [VMs as Linux Web servers](/labs/vm-web/README.md)
- [VMs as Windows dev machines](/labs/vm-win/README.md)
- [Automating VM configuration](/labs/vm-config/README.md)

_SQL Databases and ARM_

- [SQL Server](/labs/sql/README.md)
- [SQL Server VMs](/labs/sql-vm/README.md)
- [Deploying database schemas](/labs/sql-schema/README.md)
- [Automation with ARM](/labs/arm/README.md)
- [Automation with Bicep](/labs/arm-bicep/README.md)

_App Deployment with IaaS_

- [IaaS app deployment](/labs/iaas-apps/README.md)
- [Automating IaaS app deployment](/labs/iaas-bicep/README.md)
- [Creating and using VM images](/labs/vm-image/README.md)
- [Scaling with VM Scale Sets](/labs/vmss-win/README.md)
- [Provisiong Scale Sets with cloud-init](/labs/vmss-linux/README.md)

_App Service_

- [App Service for web applications](/labs/appservice/README.md)
- [App Service for static web apps](/labs/appservice-static/README.md)
- [App Service for distributed apps](/labs/appservice-api/README.md)
- [App Service configuration and administration](/labs/appservice-config/README.md)
- [App Service CI/CD](/labs/appservice-cicd/README.md)

_Project_

- [Project 1: Lift and Shift](/projects/lift-and-shift/README.md)

## Storage and Communication

_Storage Accounts_

- [Storage Accounts](/labs/storage/README.md)
- [Blob storage](/labs/storage-blob/README.md)
- [File shares](/labs/storage-files/README.md)
- [Using storage for static web content](/labs/storage-static/README.md)
- [Working with table storage](/labs/storage-table/README.md)

_Cosmos DB_

- [Cosmos DB](/labs/cosmos/README.md)
- [Cosmos DB with the Mongo API](/labs/cosmos-mongo/README.md)
- [Cosmos DB with the Table API](/labs/cosmos-table/README.md)
- [Cosmos DB performance and billing](/labs/cosmos-perf/README.md)

_KeyVault and Virtual Networks_

- [KeyVault](/labs/keyvault/README.md)
- [Virtual Networks](/labs/vnet/README.md)
- [Securing KeyVault Access](/labs/keyvault-access/README.md)
- [Securing VNet Access](/labs/vnet-access/README.md)
- [Securing apps with KeyVault and VNet](/labs/vnet-apps/README.md)

_Events and Messages_

- [Service Bus Queues](/labs/servicebus/README.md)
- [Service Bus Topics](/labs/servicebus-pubsub/README.md)
- [Event Hubs](/labs/eventhubs/README.md)
- [Eveng Hubs partitioned consumer](/labs/eventhubs-consumers/README.md)
- [Azure Cache for Redis](/labs/redis/README.md)

_Project_

- [Project 2: Distributed App](/projects/distributed/README.md)

## Compute and Containers

_Docker and Azure Container Instances_

- [Docker 101](/labs/docker/README.md)
- [Docker images and Azure Container Registry](/labs/acr/README.md)
- [Azure Container Instances](/labs/aci/README.md)
- [Distributed apps with Docker Compose](/labs/docker-compose/README.md)
- [Distributed apps with ACI](/labs/aci-compose/README.md)

_Kubernetes_

- [Nodes](/labs/kubernetes/nodes/README.md)
- [Pods](/labs/kubernetes/pods/README.md)
- [Services](/labs/kubernetes/services/README.md)
- [Deployments](/labs/kubernetes/deployments/README.md)
- [ConfigMaps](/labs/kubernetes/configmaps/README.md)
- [Azure Kubernetes Service](/labs/aks/README.md)

_Intermediate Kubernetes_

- [PersistentVolumes](/labs/kubernetes/persistentvolumes/README.md)
- [AKS PersistentVolumes](/labs/aks-persistentvolumes/README.md)
- [Ingress](/labs/kubernetes/ingress/README.md)
- [AKS with Application Gateway Ingress Controller](/labs/aks-ingress/README.md)
- [Container Probes](/labs/kubernetes/containerprobes/README.md)
- [Troubleshooting](/labs/kubernetes/troubleshooting/README.md)

_AKS Integration_

- [Namespaces](/labs/kubernetes/namespaces/README.md)
- [Secrets](/labs/kubernetes/secrets/README.md)
- [AKS with KeyVault secrets](/labs/aks-keyvault/README.md)
- [Helm](/labs/kubernetes/helm/README.md)
- [Securing AKS apps with KeyVault and VNet](/labs/aks-apps/README.md)

_Project_

- [Project 3: Containerized App](/projects/conatinerized/README.md)

## Serverless and App Management

_Azure Functions_

- [HTTP trigger](/labs/functions/http/README.md)
- [Timer trigger & blob output](/labs/functions/timer/README.md)
- [Blob trigger & SQL output](/labs/functions/blob/README.md)
- [Service Bus trigger & multiple outputs](/labs/functions/servicebus/README.md)
- [RabbitMQ trigger & blob output](/labs/functions/rabbitmq/README.md)
- [CosmosDB trigger & output](/labs/functions/cosmos/README.md)

_Durable Functions_

- [CI/CD for Azure Functions](/labs/functions/cicd/README.md)
- [Durable functions](/labs/functions-durable/chained/README.md)
- [Fan-out fan-in pattern](/labs/functions-durable/fan-out/README.md)
- [Human interaction pattern](/labs/functions-durable/human/README.md)
- [Azure SignalR Service](/labs/signalr/README.md)
- [SignalR functions output](/labs/functions/signalr/README.md)

_API Management_ 

- [API Management](/labs/apim/README.md)
- [Mocking APIs](/labs/apim-mock/README.md)
- [Securing APIs with policies](/labs/apim-policies/README.md)
- [Versioning APIs for breaking changes](/labs/apim-versioning/README.md)

_Web Application Firewall & CDN_

- [Application Gateway & WAF](/labs/appgw/README.md)
- [Front Door with CDN & WAF](/labs/frontdoor/README.md)

_Monitoring_

- [Monitoring with Application Insights](/labs/applicationinsights/README.md)
- [Querying logs and metrics with Log Analytics](/labs/loganalytics/README.md)

_Project_

- [Project 4: Serverless Apps](/projects/serverless/README.md)


#### Credits

Created by [@EltonStoneman](https://twitter.com/EltonStoneman) ([sixeyed](https://github.com/sixeyed)): Freelance Consultant and Trainer. Author of [Learn Docker in a Month of Lunches](https://www.manning.com/books/learn-docker-in-a-month-of-lunches), [Learn Kubernetes in a Month of Lunches](https://www.manning.com/books/learn-kubernetes-in-a-month-of-lunches) and [many Pluralsight courses](https://pluralsight.pxf.io/c/1197078/424552/7490?u=https%3A%2F%2Fwww.pluralsight.com%2Fauthors%2Felton-stoneman).

