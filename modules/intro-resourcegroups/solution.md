# Lab Solution

You can use JMESPath to filter RGs based on a tag:

```
az group list -o table --query "[?tags.course=='az204']" 
```

Add a field name at the end to include just part of the resource details in the response:

```
az group list -o table --query "[?tags.course=='az204'].name"
```

And switch to TSV format to lose the table header:

```
az group list -o tsv --query "[?tags.course=='az204'].name"
```

> Now you have a list of group names you can feed into the delete command.

How you do that depends on your shell.

_In PowerShell:_

```
az group list -o tsv --query "[?tags.course=='az204'].name" | foreach { az group delete -y -n $_ }
```

_In Bash:_

```
for rg in $(az group list -o tsv --query "[?tags.course=='az204'].name"); do az group delete -y -n ${rg}; done
```