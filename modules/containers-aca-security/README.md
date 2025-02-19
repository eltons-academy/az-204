# Azure Container Apps - Security

One of the big appeals of Container Apps is the turnkey feature set. You can enable lots of high-value infrastructure functions with very little effort and with no changes to your application code. We'll focus on the main features for securing Container Apps in this module: adding authentication to web apps; restricting access to internal components; using secrets for senstive config settings and enabling encryption between components.

## Reference

- [Authentication and authorization in Azure Container Apps](https://learn.microsoft.com/en-us/azure/container-apps/authentication)

- [Container Apps YAML specification](https://learn.microsoft.com/en-us/azure/container-apps/azure-resource-manager-api-spec?tabs=yaml)

- [Distributed Application Runtime (Dapr) documentation](https://docs.dapr.io)

- [`az containerapp` commands](https://learn.microsoft.com/en-us/cli/azure/containerapp/auth?view=azure-cli-latest)

## Deploy Container Apps from YAML

We'll recreate the random number app again, this time deploying it from Container App YAML models.

Start with a new Resource Group for the lab, using your preferred region:

```
az group create -n labs-aca-security3 --tags course=az204 -l eastus
```

And a new Container App environment:

```
az containerapp env create --name rng -g labs-aca-security3
```

> You can't model the full Environment with multiple Container Apps in a single YAML model in the Azure format - you could use the COmpose model for that. Instead you model each Container App:

- [rng-api-aca.yaml](\modules\containers-aca-security\rng-api-aca.yaml) - YAML model for the API
- [rng-web-aca.yaml](\modules\containers-aca-security\rng-web-aca.yaml) - YAML model for the web app

Yes, they are pretty much identical.

```
az containerapp create -n rng-api -g labs-aca-security3 --environment rng --yaml "modules/containers-aca-security/rng-api-aca.yaml"

az containerapp create -n rng-web -g labs-aca-security3 --environment rng --yaml "modules/containers-aca-security/rng-web-aca.yaml"


```



## Enable authentication for the web app

ACA supports turnkey authentication and authorization. You can integrate with multiple identity providers - including Microsoft, GitHub, Apple, Google and OpenID Connect. 

You don't need any code changes to add authentication to your app; the auth flow sits before your application and only sends requests when users have a validated token from a supported identity provider.

Auth is not enabled by default:

```
az containerapp auth show -g labs-aca-security3 -n rng-web
```

We'll integrate with Microsoft's identity provider. To do that we need to create a few identity resources - these will get their own module later, so don't worry too much about what's happening here.l

```
# store your web app's callback URL:
$RNG_WEB=$(az containerapp show -n rng-web -g labs-aca-security3 --query 'properties.configuration.ingress.fqdn' -o tsv)
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
az containerapp auth microsoft update -g labs-aca-security3 -n rng-web --client-id $CLIENT_ID --issuer "https://sts.windows.net/$TENANT_ID/"

# requre auth for the app:
az containerapp auth update -g labs-aca-security3 -n rng-web --redirect-provider azureactivedirectory --action RedirectToLoginPage
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
az containerapp ingress show -g labs-aca-security3 -n rng-api -o table
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
az containerapp ingress update -g labs-aca-security3 -n rng-api --type internal --transport http --target-port 8080 --allow-insecure true -o table
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
az containerapp secret set -g labs-aca-security3 -n rng-web --secrets "rng-api-url=http://rng-api/rng"
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
az containerapp update -g labs-aca-security3 -n rng-web --set-env-vars "RngApi__Url=secretref:rng-api-url"
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
az containerapp env dapr-component set -g labs-aca-security3 -n rng --dapr-component-name rng-web --yaml modules/containers-aca/dapr/rng-web-component.yaml
az containerapp env dapr-component set -g labs-aca-security3 -n rng --dapr-component-name rng-api --yaml modules/containers-aca/dapr/rng-api-component.yaml
```

enable dapr:

```
az containerapp dapr enable -g labs-aca-security3 -n rng-api --dapr-app-id rng-api --dapr-app-port 8080

az containerapp dapr enable -g labs-aca-security3 -n rng-web --dapr-app-id rng-web --dapr-app-port 8080 --dapr-enable-api-logging
```

set web to use dapr sidecar:

```
az containerapp update -g labs-aca-security3 -n rng-web --set-env-vars "RngApi__Url=http://localhost:3500/v1.0/invoke/rng-api/method/rng"

az containerapp revision list -g labs-aca-security3 -n rng-web -o table
```

- test

remove ingress from api - all coms via dapr:

```
az containerapp ingress disable -g labs-aca-security3 -n rng-api 
```

> can add resiliency policy for retries, timeout etc https://learn.microsoft.com/en-us/azure/container-apps/dapr-component-resiliency?tabs=cli



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