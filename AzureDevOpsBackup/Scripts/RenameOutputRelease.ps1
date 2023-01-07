#Folder for old builds
$FolderName = ".\Old\"
if(Get-Item -Path $FolderName -ErrorAction Ignore) {   
    Write-Host "Old folder for Release builds Exists"
    Get-ChildItem -Path ".\*AzureDevOpsBackup*Build at*.exe" -Recurse | Move-Item -Destination $FolderName
}
else {
    Write-Host "Old folder for Release builds doesn't Exists - Creating it..."    
    #PowerShell Create directory if not exists
    New-Item $FolderName -ItemType Directory
}
#Delete old .exe file there not need to be used anymore 2 sec. old or more
Get-ChildItem ".\*AzureDevOpsBackup*Build at*" -File | Where-Object CreationTime -lt (Get-Date).AddSeconds(-2) | Remove-Item -Force

#Get file v. for last build ServiceAccounts.exe file
$FileVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo(".\AzureDevOpsBackup.exe").FileVersion

#Rename file to v. and buildtime - keep original
Get-ChildItem ".\AzureDevOpsBackup.exe" | Where-Object {!$_.PSIsContainer -and $_.extension -eq '.exe'} | Copy-Item -Path ".\AzureDevOpsBackup.exe" -Destination {"$($_.BaseName) v. $FileVersion - Build at $(Get-Date -format "ddMMyyyy-HHmmss")$($_.extension)"} -Force