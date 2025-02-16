# Azure Container 

## Reference

- [Implement Azure Container Apps](https://learn.microsoft.com/en-gb/training/modules/implement-azure-container-apps/) | Microsoft Learn

- [Container Apps documentation](https://docs.microsoft.com/en-gb/azure/container-apps/)

- [Container Apps YAML specification](https://learn.microsoft.com/en-us/azure/container-apps/azure-resource-manager-api-spec?tabs=yaml)

- [`az containerapp` commands](https://learn.microsoft.com/en-us/cli/azure/containerapp?view=azure-cli-latest)


## Explore Azure Container Instances

Open the Portal ...


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




