# Azure Container 

## Reference

- [Implement Azure Container Apps](https://learn.microsoft.com/en-gb/training/modules/implement-azure-container-apps/) | Microsoft Learn

- [Container Apps documentation](https://docs.microsoft.com/en-gb/azure/container-apps/)

- [Container Apps YAML specification](https://learn.microsoft.com/en-us/azure/container-apps/azure-resource-manager-api-spec?tabs=yaml)

- [`az containerapp` commands](https://learn.microsoft.com/en-us/cli/azure/containerapp?view=azure-cli-latest)


## Explore Azure Container Apps

Open the Portal, create, search container app, create

- container app environment
- container details - registry, compute
- ingress - internal, external, transport


## Create an ACA container with the CLI

Start with a new Resource Group for the lab, using your preferred region:

```
az group create -n labs-aca --tags course=az204 -l eastus
```

Ensure your subscription and command line updated for ACA:

```
az extension add --name containerapp --upgrade
az provider register -n Microsoft.App --wait
az provider register -n Microsoft.OperationalInsights --wait
```

create env

```
az containerapp env create --name rng -g labs-aca
```

> creates loga with random suffix name

create api container

```
az containerapp create --name rng-api --environment rng -g labs-aca --image ghcr.io/eltons-academy/rng-api:2025 --target-port 8080 --ingress external
```

print dns:

```
az containerapp show -n rng-api -g labs-aca --query properties.configuration.ingress.fqdn
```

test:

```
curl https://<api-fqdn>/rng
```

```
az containerapp logs show -n rng-api -g labs-aca
```

## Connect Container Apps in an Environment

- run web

```
az containerapp create --name rng-web --environment rng -g labs-aca --image ghcr.io/eltons-academy/rng-web:2025 --target-port 8080 --ingress external --query properties.configuration.ingress.fqdn
```

> browse site, shows - error message on clicking Go

```
docker image inspect ghcr.io/eltons-academy/rng-web:2025
```

> conf set in env `RngApi__Url`

```
az containerapp show -g labs-aca -n rng-web --query 'properties.template.containers[0].env'
```

> none

```
az containerapp update -g labs-aca -n rng-web --set-env-vars "RngApi__Url=https://<api-fqdn>/rng"
```

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