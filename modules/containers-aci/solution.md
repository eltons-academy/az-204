# Lab Solution

Delete the existing container:

```
az container delete -g labs-aci --name random-logger -y
```

You can manage files in a share using the [az storage file](https://learn.microsoft.com/en-us/cli/azure/storage/file?view=azure-cli-latest) commands:

```
az storage file delete --path log.txt --share-name logs --account-name $SA_NAME --account-key <your-account-key>
```

And now create the container, specifying a `restart-policy` of `OnFailure` - which means the container will only be restarted if it exits with a failure code:

```
az container create -g labs-aci --name random-logger --image ghcr.io/eltons-academy/random-logger:2025 --os-type Linux --cpu 0.1 --memory 0.1 --restart-policy OnFailure --azure-file-volume-account-name $SA_NAME --azure-file-volume-share-name logs --azure-file-volume-mount-path /random --azure-file-volume-account-key <your-account-key>
```

The container will start and run, write one entry to the file and exit. Azure won't restart it, so in the Portal you should see one random number in the file and the ACI status will be _Suceeded_.

> [Back to the lab](README.md#lab)
