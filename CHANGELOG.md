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