$today=Get-Date -Format "MMddyyyy"
$groupname="CCProjektPrugger"
$location="West Europe"
$storagename="ccprojektstorageprugger"
$tablename="Statistik"
$containername="images"
$queue1name="bookinputqueue"
$queue2name="userinputqueue"
$af1name="createstatistics"
$af2name="getstatistics"
$afappname="CCProjektFunctions"
$afruntime="dotnet"
$cosmosaccountname="ccpruggercosmosdb"
$cosmosdbname="ccstandarddb"
$container1name="books"
$container1key="/id"
$container2name="users"
$container2key="/id"
$partitionkeykind="Hash"
$keyvaultname="cckeyvaultprugger-$today"
$keyvaultuser="patrick.prugger@edu.campus02.at"

New-AzResourceGroup -Name $groupname -Location $location

New-AzStorageAccount -ResourceGroupName $groupname `
  -Name $storagename `
  -Location $location `
  -SkuName "Standard_LRS" `
  -Kind StorageV2

$storageAccountKey = (Get-AzStorageAccountKey -ResourceGroupName $groupname -Name $storagename).Value[0]
$storageContext = New-AzureStorageContext -StorageAccountName $storagename -StorageAccountKey $storageAccountKey

New-AzureStorageTable -Name $tablename -Context $storageContext
New-AzureStorageContainer -Name $containername -Context $storageContext
New-AzureStorageQueue -Name $queue1name -Context $storageContext
New-AzureStorageQueue -Name $queue2name -Context $storageContext

New-AzFunctionApp -Name $afappname -ResourceGroupName $groupname -StorageAccount $storagename -Runtime $afruntime -FunctionsVersion 3 -Location $location

New-AzCosmosDBAccount -ResourceGroupName $groupname -Name $cosmosaccountname  -Location $location
New-AzCosmosDBSqlDatabase -AccountName $cosmosaccountname -Name $cosmosdbname -ResourceGroupName $groupname
New-AzCosmosDBSqlContainer -AccountName $cosmosaccountname -DatabaseName $cosmosdbname -ResourceGroupName $groupname -Name $container1name -PartitionKeyPath $container1key -PartitionKeyKind $partitionkeykind
New-AzCosmosDBSqlContainer -AccountName $cosmosaccountname -DatabaseName $cosmosdbname -ResourceGroupName $groupname -Name $container2name -PartitionKeyPath $container2key -PartitionKeyKind $partitionkeykind

New-AzKeyVault -VaultName $keyvaultname -ResourceGroupName $groupname -Location $location
Set-AzKeyVaultAccessPolicy -VaultName $keyvaultname -UserPrincipalName $keyvaultuser -PermissionsToSecrets Get,List,Set,Delete


Write-Host "storage account key 1 = " $storageAccountKey
Get-AzureStorageContainer -Context $storageContext
Get-AzureStorageQueue -Context $storageContext