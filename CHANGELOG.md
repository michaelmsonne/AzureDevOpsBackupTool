
## [1.0.6.0] - 08-08-2024

### Changed
- Upgraded RestSharp.111.3.0 to RestSharp.111.4.1

### Fixed
- Fix an exception has been thrown (Installer bug)
- Fixed some assembly references

## [1.0.5.9] - 10-07-2024

### Changed
- Upgraded System.Text.Json.8.0.3 to System.Text.Json.8.0.4 (fix CVE-2024-30105)
- Upgraded RestSharp.110.2.0 to RestSharp.111.3.0

## [1.0.5.8] - 13-04-2024

### Changed
- Small typos in logging
- Small typos in tool in general
- Sanitize directory name(s) to avoid path error(s) in backups and cleanup
- Some code cleanup and optimization
- Upgraded Microsoft.Bcl.AsyncInterfaces.7.0.0 to Microsoft.Bcl.AsyncInterfaces.8.0.0
- Upgraded System.Text.Encodings.Web.7.0.0 to System.Text.Encodings.Web.8.0.0
- Upgraded System.Text.Json.7.0.3 to System.Text.Json.8.0.3
- Upgraded RestSharp from 106.15.0.0 to 110.2.0

### Fixed
- Fix special characters in branch name to avoid path error when saving backups
- The program now saves files using the DownloadStream method, allowing large files to download without crash and useing async.

## [1.0.5.7] - 11-03-2024

### Add
- Add link to blog in about

### Changed
- Typos in report layout
- Changes for better logfile layout/formatting

## [1.0.5.6] - 03-09-2023

### Changed
- Upgraded System.Text.Json to 7.0.3

## [1.0.5.5] - 06-05-2023

### Add
- Added function to count current numers of backups in backup folder and report to email report
- Added function test connection to REST API - if not connected, it will show the error details
- Added function to test if needed system requrements is set

### Changed
- Some changes to help text

### Fixed
- Fixed report status check for original .zip files letover in backup (if clenup parameter is set)
- Fixed wrong number of current backups in backup folder in email report

## [1.0.5.4] - 03-4-2023

### Add
- Added Class to secure the token for authentication to the Azure DevOps API form an encrypted .bin file based in hardware id for the hardware the .bin file is generated on

### Changed
- Some changes to help text

## [1.0.5.3] - 29-3-2023

### Add
- Added Class to secure the token for authentication to the Azure DevOps API in console runtime

### Changed
- Small changes and optimization

## [1.0.5.2] - 26-3-2023

### Fixed
- Now the logfile is named correct again under some conditions

## [1.0.5.1] - 25-3-2023

### Add
- Added new parameter to set email priority

### Fixed
- Now the log is attathed to to e-mail report again

### Changed
- Reorder the backup cleanup of the code to Classes

## [1.0.5.0] - 24-3-2023

### Add
- Added new parameter for information about the tool in console

### Changed
- Reorder the most of the code to Classes
- Updated Newtonsoft.Json and System.Text.Json to the last versions
- Renamed argument for backup folder from --outdir to --backup
- Other Small changes and optimization
- Upgraded to .Net 4.8

## [1.0.4.2] - 15-2-2023

### Changed
- Small changes and optimization

## [1.0.4.1] - 25-2-2023

### Add
- Added branch name to list of repos there is backed up to email report
- Added orgname to email report
- Added backup start and end time timestamp to email report

### Changed
- Small changes to email report text

## [1.0.4.0] - 23-2-2023

### Changed
- Changed backup feature to backup all branches prom Git project(s)
- Small cleanup in email function

### Fixed
- Small changes to logning

## [1.0.3.3] - 22-2-2023

Use new code sign certificate

### Changed
- Checked api uses - no changes in API calls, so set to api-version=7.0 (api-version=5.1-preview.1 before)
- Small cleanup in email function

### Fixed
- CWE-691: Insufficient Control Flow Management in code
- Minor code use issues
- Remove unused check for data in a function

## [1.0.3.2] - 06-2-2023

### Changed
- Small changes to email function

### Fixed
- Fixed vulnerable Incorrect Regular Expression in RestSharp (CVE-2021-27293)
- Fixed vulnerable Improper Handling of Exceptional Conditions in Newtonsoft.Json (GHSA ID GHSA-5crp-9r3c-p9vr)

## [1.0.3.1] - 05-2-2023

### Changed
- Small changes to email text

### Fixed
- Missing email text for missing email status text for backups to keep (status)

## [1.0.3.0] - 30-1-2023

Now signed with Code Sign Certificate from Sectigo

### Fixed
- Fix no arguments is set - show the help in console
- Error handling if files cant be deletes
- Better email status/added some of the missing statuses (still more to do)
- args.Length > 0' is always true

### Added
- Help menu
- Error handling if files cant be deleted
- Count errors and add to email report

### Changed
- Changes to log text
- Changes to arguments names
- Small changes to email function
- Reordered some code/functions for better application

## 7-1-2023
- Created an installer for the project

## [1.0.0.2] - 5-1-2023
### Fixed
- Fix no logging when email function reads logfile when sending report

### Changed
- Added more logging and error handling
- Added option to use simpel or full email report layout
- Changed Assembly Title

## [1.0.0.1] - 1-1-2023
### Fixed
- Small typos
- Code cleanup and added better mail status text

### Changed
- Added more logging and error handling
- New email rapport layout and small typos

## [1.0.0.0] - 11-12-2022
- Initial release