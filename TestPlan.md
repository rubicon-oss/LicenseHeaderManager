# Testcases

## Instructions

Open the TestSolution with the to be tested LicenseHeaderManager Version and follow the documented steps. Have your preferred Git GUI open to check if there are changes made and check visually if the expected files are the same as documented


## Quick Test

| ID | Right-Click project file          | Remove LicenseHeaders - changed files | Add LicenseHeaders                                      | Add LicenseHeaders again         |
|----|-----------------------------------|---------------------------------------|---------------------------------------------------------|----------------------------------|
| 1  | App2/App2.Windows                 | 6 changed files                       | There should be no changed files                        | There should be no changed files |
| 2  | SharedApp/BlankSharedApp1.Windows | 1 changed files                       | Press "no" on dialog - There should be no changed files | There should be no changed files |
| 3  | App1                              | 5 changed files                       | There should be no changed files                        | There should be no changed files |
| 4  | ConsoleApplication1               | 2 changed files                       | There should be no changed files                        | There should be no changed files |
| 5  | LHM-CsProjTest                    | 5 changed files                       | There should be no changed files                        | There should be no changed files |
| 6  | OfficeApp1                        | 2 changed files                       | There should be no changed files                        | There should be no changed files |
| 7  | OfficeApp1Web                     | 268 changed files                     | There should be no changed files                        | There should be no changed files |
| 8  | PHPWebProject1                    | 2 changed files                       | There should be no changed files                        | There should be no changed files |
| 9  | PythonApplication1                | 3 changed files                       | There should be no changed files                        | There should be no changed files |
| 10 | SetupProject1                     | 3 changed files                       | There should be no changed files                        | There should be no changed files |
| 11 | TypeScriptHTMLApp1                | 5 changed files                       | There should be no changed files                        | There should be no changed files |

## Solution Test

| ID | Right-Click solution | Remove LicenseHeaders - changed files | Add LicenseHeaders - click "no" on dialog | Add LicenseHeaders again - click "no" on dialog |
|----|----------------------|---------------------------------------|-------------------------------------------|-------------------------------------------------|
| 12 | LHM-CsProjTest       | 302 changed files                     | There should be no changed files          | There should be no changed files                |

## Settings Persistence Test

| ID | Go to Tools->Options->License Header Manager | Change Setting                                                  | Close options page abd reopen                             | Close Visual Studio and reopen   |
|----|----------------------------------------------|-----------------------------------------------------------------|-----------------------------------------------------------|----------------------------------|
| 13 | General                                      | Change the first checkbox, edit the textbox and add one command | Changed settings should be there                          | Changed settings should be there |
| 14 | Default Header                               | Change default header                                           | Changed settings should be there                          | Changed settings should be there |
| 15 | Languages                                    | Add a new language, change existing language                    | Changed settings should be there                          | Changed settings should be there |

## Settings Functionality Test

| ID | Setting                                                                       | Action                                                                                                         | Result                                        |
|----|-------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------|-----------------------------------------------|
| 16 | Checkbox "Automatically insert license headers in new files"                  | Check, create a new file "test1.cs"                                                                            | License Header should be there                |
| 17 | Checkbox "Automatically insert license headers in new files"                  | Uncheck, create new file "test2.cs"                                                                            | License Header should not be there            |
| 18 | Checkbox "Only remove comments containing at least one of the keywords below" | Uncheck, go to "test1.cs", remove copyright region, add license header                                         | There should be two license headers now       |
| 19 | Checkbox "Only remove comments containing at least one of the keywords below" | Check, change textbox below to "foo", go to "test1.cs", add license header                                     | There should be three license headers now     |
| 20 | Linked Commands                                                               | Add a command you don't already have e.g. "File.SaveAll", go to "test2.cs", do the command"                    | License Header should be there                |
| 21 | Linked Commands                                                               | Remove the added command and remove the license header from "test2.cs", do the command                         | License Header should not be there            |
| 22 | Default Header                                                                | Change default license header, remove license header file, generate new one, add license header to "test2.cs"  | New License Header should be there            |
| 23 | Languages                                                                     | Remove .cs from languages, right click "test2.cs"                                                              | There should be no license header options     |
| 24 | Languages                                                                     | Add .cs to languages, right click "test2.cs"                                                                   | There should be license header options        |

# Defects

## 3.0.0

| Defect ID | ID    | Project  | Step                           | Description                                   | Issue | Remarks |
|-----------|-------|----------|--------------------------------|-----------------------------------------------|-------|---------|
| 1         | 13/15 | Settings | Close Visual Studio and reopen | Settings saved as a collection are not saved. | #108  |

## 2.0.1

| Defect ID | ID | Project    | Step                | Description                                                                                                                                                         | Issue | Remarks |
|-----------|----|------------|---------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------|---------|
| 1         | 6  | OfficeApp1 | Add LicenseHeaders  | One file remains changed, but it is a file from OfficeApp1Web Project, which seems unrelated to current Project. Error is also happening with 2.0.0, so no new bug. |

## 1.7.3

| Defect ID | ID | Project            | Step                                            | Description                                                                                            | Issue            | Remarks |
|-----------|----|--------------------|-------------------------------------------------|--------------------------------------------------------------------------------------------------------|------------------|---------|
| 1         | 12 | LHM-CsProjTest     | Add LicenseHeaders again (click "no" on dialog) | On files where a skip expression is defined, LHM tries to remove an extra NewLine when readding the LH | #62              |         |
| 2         | 6  | OfficeApp1         | Add LicenseHeaders again                        | Same as Defect 1                                                                                       | Same as Defect 1 |         |
| 3         | 11 | TypeScriptHTMLApp1 | Add LicenseHeaders again                        | Same as Defect 1                                                                                       | Same as Defect 1 |

## 1.7.2

| Defect ID | ID | Project                           | Step                 | Description                                                                                                                                                                                                                                                               | Issue | Remarks                                                                                                                 |
|-----------|----|-----------------------------------|----------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------|-------------------------------------------------------------------------------------------------------------------------|
| 1         | 2  | SharedApp/BlankSharedApp1.Windows | Add LicenseHeader    | Error message "No License Header Definition File found in Project" appears                                                                                                                                                                                                |       | There exists a linked .licenseheader file in the project. When does this Window appears, what is the desired behaviour? |
| 2         | 3  | App1                              | Add LicenseHeader    | Whitespace is added into app.config after skipped expression                                                                                                                                                                                                              | #59   |                                                                                                                         |
| 3         | 5  | LHM-CsProjTest                    | Add LicenseHeader    | Whitespace is added into .xml file after skipped expression                                                                                                                                                                                                               | #59   |                                                                                                                         |
| 4         | 7  | OfficeApp1Web                     | Add LicenseHeader    | Visual Studio freezes                                                                                                                                                                                                                                                     |       | Did happen just once                                                                                                    |
| 5         | 9  | PythonApplication1                | Add LicenseHeader    | Error message "No License Header Definition File found in Project" appears                                                                                                                                                                                                |       | There exists a linked .licenseheader file in the project. When does this Window appears, what is the desired behaviour? |
| 6         | 11 | TypeScriptHTMLApp1                | Remove LicenseHeader | One .licenseheader definition contains an extra newline before the next extensions are defined. When removing the licenseheader, an extra newline is added right after the removed licenseheader. This extra newline gets deleted again when re-adding the .licenseheader |       |                                                                                                                         |
| 7         | 12 | LHM-CsProjTest                    | Add LicenseHeader    | Whitespace errors see 1.7.2 Defect number 2/3                                                                                                                                                                                                                             |       |                                                                                                                         |
| 8         | 8  | PHPWebProject1                    | Add LicenseHeader    | Whitespace is added into .js file after skipped expression                                                                                                                                                                                                                | #59   |