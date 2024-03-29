param (
    [string]$OutputFolderPath
)

# Remove any double quotes from the provided $OutputFolderPath parameter
$OutputFolderPath = $OutputFolderPath -replace '"', ''

# Folder for old builds
$FolderName = Join-Path -Path $OutputFolderPath -ChildPath "Old"
if (Test-Path -Path $FolderName -PathType Container) {   
    Write-Host "Old folder for Debug builds exists"
    Get-ChildItem -Path "$OutputFolderPath\*AzureDevOpsBackup*Debug Build at*.exe" -Recurse | Move-Item -Destination $FolderName
    Write-Host "Moved files $OutputFolderPath\*AzureDevOpsBackup*Debug Build at*.exe to $FolderName"
} else {
    Write-Host "Old folder for Debug builds doesn't exist - Creating it..."
    # PowerShell Create directory if not exists
    New-Item -Path $FolderName -ItemType Directory
    Write-Host "Old folder for Debug builds doesn't exist - Created..."
}

# Delete old .exe files that are not needed anymore (2 seconds old or more)
Get-ChildItem -Path "$OutputFolderPath\*AzureDevOpsBackup*Debug Build at*" -File | Where-Object CreationTime -lt (Get-Date).AddSeconds(-2) | Remove-Item -Force

# Get the file version for the last build of AzureDevOpsBackup.exe
$FileVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo("$OutputFolderPath\AzureDevOpsBackup.exe").FileVersion

# Rename the file to include version and build time (keep original)
Get-ChildItem -Path "$OutputFolderPath\AzureDevOpsBackup.exe" | Where-Object {!$_.PSIsContainer -and $_.Extension -eq '.exe'} | ForEach-Object {
    $NewFileName = "{0} v. {1} - Debug Build at {2}{3}" -f $_.BaseName, $FileVersion, (Get-Date -Format "ddMMyyyy-HHmmss"), $_.Extension
    $NewFilePath = Join-Path -Path $OutputFolderPath -ChildPath $NewFileName
    Copy-Item -Path $_.FullName -Destination $NewFilePath -Force
    Write-Host "Copied" $_.FullName "to $NewFilePath"
}

<#
param (
    [string]$OutputFolderPath
)

# Remove any double quotes from the provided $OutputFolderPath parameter
$OutputFolderPath = $OutputFolderPath -replace '"', ''

# Determine the source folder containing AzureDevOpsBackup.exe
$SourceFolderPath = (Get-Location).Path
$AzureDevOpsBackupFile = Get-ChildItem -Path $SourceFolderPath -Filter "AzureDevOpsBackup.exe" -Recurse | Sort-Object CreationTime -Descending | Select-Object -First 1

if ($AzureDevOpsBackupFile) {
    # Check if the output folder exists, if not, create it
    if (-not (Test-Path -Path $OutputFolderPath -PathType Container)) {
        Write-Host "Output folder doesn't exist - Creating it..."
        New-Item -Path $OutputFolderPath -ItemType Directory
    }

    # Move files to the output folder, excluding the .\old folder
    $SearchPattern = "*AzureDevOpsBackup*Debug Build at*.exe"
    $FilesToMove = Get-ChildItem -Path $SourceFolderPath -Filter $SearchPattern -Recurse | Where-Object { $_.DirectoryName -ne "$OutputFolderPath\old" }
    foreach ($File in $FilesToMove) {
        $DestinationPath = Join-Path -Path $OutputFolderPath -ChildPath $File.Name
        
        # Check if the source and destination paths are the same
        if ($File.FullName -ne $DestinationPath) {
            Copy-Item -Path $File.FullName -Destination $DestinationPath -Force
        }
    }

    # Delete old .exe files that are 2 seconds or more old
    $OutputFolderPath = $OutputFolderPath.TrimEnd("\")
    Get-ChildItem -Path $OutputFolderPath -Filter $SearchPattern -File | Where-Object CreationTime -lt (Get-Date).AddSeconds(-2) | Remove-Item -Force

    # Get the version of the AzureDevOpsBackup.exe file
    $FileVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($AzureDevOpsBackupFile.FullName).FileVersion

    # Rename the file to include version and build time
    $NewFileName = "{0} v. {1} - Debug Build at {2}{3}" -f $AzureDevOpsBackupFile.BaseName, $FileVersion, (Get-Date -Format "ddMMyyyy-HHmmss"), $AzureDevOpsBackupFile.Extension
    $NewFilePath = Join-Path -Path $OutputFolderPath -ChildPath $NewFileName
    Rename-Item -Path $AzureDevOpsBackupFile.FullName -NewName $NewFilePath -Force

    # Copy old files to the .\old folder while keeping the original filenames
    $OldFolderPath = Join-Path -Path $OutputFolderPath -ChildPath "old"
    if (-not (Test-Path -Path $OldFolderPath -PathType Container)) {
        Write-Host "Creating .\old folder..."
        New-Item -Path $OldFolderPath -ItemType Directory
    }

    # Copy old files to the .\old folder while keeping the original filenames
    $OldFiles = Get-ChildItem -Path $OutputFolderPath -Filter $SearchPattern -Recurse | Where-Object { $_.DirectoryName -ne $OldFolderPath }
    foreach ($OldFile in $OldFiles) {
        $OldFileDestinationPath = Join-Path -Path $OldFolderPath -ChildPath $OldFile.Name
        Copy-Item -Path $OldFile.FullName -Destination $OldFileDestinationPath -Force
    }
} else {
    Write-Host "AzureDevOpsBackup.exe not found in the source folder: $SourceFolderPath"
}
#>

<#
#Folder for old builds
$FolderName = ".\Old\"
if(Get-Item -Path $FolderName -ErrorAction Ignore) {   
    Write-Host "Old folder for Debug builds Exists"
    Get-ChildItem -Path ".\*AzureDevOpsBackup*Debug Build at*.exe" -Recurse | Move-Item -Destination $FolderName
}
else {
    Write-Host "Old folder for Debug builds doesn't Exists - Creating it..."     
    #PowerShell Create directory if not exists
    New-Item $FolderName -ItemType Directory
}
#Delete old .exe file there not need to be used anymore 2 sec. old or more
Get-ChildItem ".\*AzureDevOpsBackup*Debug Build at*" -File | Where-Object CreationTime -lt (Get-Date).AddSeconds(-2) | Remove-Item -Force

#Get file v. for last build ServiceAccounts.exe file
$FileVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo(".\AzureDevOpsBackup.exe").FileVersion

#Rename file to v. and buildtime - keep original
Get-ChildItem ".\AzureDevOpsBackup.exe" | Where-Object {!$_.PSIsContainer -and $_.extension -eq '.exe'} | Copy-Item -Path ".\AzureDevOpsBackup.exe" -Destination {"$($_.BaseName) v. $FileVersion - Debug Build at $(Get-Date -format "ddMMyyyy-HHmmss")$($_.extension)"} -Force
#>