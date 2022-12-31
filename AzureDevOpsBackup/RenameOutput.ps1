#Folder for old builds
$FolderName = ".\Old\"
if(Get-Item -Path $FolderName -ErrorAction Ignore) {   
    Write-Host "Folder Exists"
    Get-ChildItem -Path ".\*AzureDevOpsBackup*Build at*.exe" -Recurse | Move-Item -Destination $FolderName
}
else {
    Write-Host "Folder Doesn't Exists - Creating it..."    
    #PowerShell Create directory if not exists
    New-Item $FolderName -ItemType Directory
}

#Get file v. for last build ServiceAccounts.exe file
$FileVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo(".\AzureDevOpsBackup.exe").FileVersion

#Rename file to v. and buildtime
Get-ChildItem ".\AzureDevOpsBackup.exe" | Where-Object {!$_.PSIsContainer -and $_.extension -eq '.exe'} | Rename-Item -NewName {"$($_.BaseName) v. $FileVersion - Build at $(Get-Date -format "ddMMyyyy-HHmmss")$($_.extension)"} -Force

#Delete old .exe file there not need to be used anymore
#Get-ChildItem ".\bin\x64" -Recurse -File | Where CreationTime -lt (Get-Date).AddSeconds(-5) | Remove-Item -Force
#Get-ChildItem ".\*AzureDevOpsBackup*" -File | Where-Object CreationTime -lt (Get-Date).AddSeconds(-5) | Remove-Item -Force