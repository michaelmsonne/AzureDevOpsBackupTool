param (
    [string]$OutputFolderPath
)

# Remove any double quotes from the provided $OutputFolderPath parameter
$OutputFolderPath = $OutputFolderPath -replace '"', ''

# Folder for old builds
$FolderName = Join-Path -Path $OutputFolderPath -ChildPath "Old"
if (Test-Path -Path $FolderName -PathType Container) {   
    Write-Host "Old folder for builds exists"
    Get-ChildItem -Path "$OutputFolderPath\*AzureDevOpsBackup*Build at*.exe" -Recurse | Move-Item -Destination $FolderName
    Write-Host "Moved files $OutputFolderPath\*AzureDevOpsBackup*Build at*.exe to $FolderName"
} else {
    Write-Host "Old folder for Release builds doesn't exist - Creating it..."
    # PowerShell Create directory if not exists
    New-Item -Path $FolderName -ItemType Directory
    Write-Host "Old folder for Release builds doesn't exist - Created..."
}

# Delete old .exe files that are not needed anymore (2 seconds old or more)
Get-ChildItem -Path "$OutputFolderPath\*AzureDevOpsBackup*Build at*" -File | Where-Object CreationTime -lt (Get-Date).AddSeconds(-2) | Remove-Item -Force

# Get the file version for the last build of AzureDevOpsBackup.exe
$FileVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo("$OutputFolderPath\AzureDevOpsBackup.exe").FileVersion

# Rename the file to include version and build time (keep original)
Get-ChildItem -Path "$OutputFolderPath\AzureDevOpsBackup.exe" | Where-Object {!$_.PSIsContainer -and $_.Extension -eq '.exe'} | ForEach-Object {
    $NewFileName = "{0} v. {1} - Build at {2}{3}" -f $_.BaseName, $FileVersion, (Get-Date -Format "ddMMyyyy-HHmmss"), $_.Extension
    $NewFilePath = Join-Path -Path $OutputFolderPath -ChildPath $NewFileName
    Copy-Item -Path $_.FullName -Destination $NewFilePath -Force
    Write-Host "Copied" $_.FullName "to $NewFilePath"
}

<#
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
#>