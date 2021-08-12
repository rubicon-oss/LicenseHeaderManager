# License Header Manager for Visual Studio


Manage license headers for your source code files in Visual Studio.

New files will automatically include the license headers defined in the current project.

License Header Manager allows you to:

* Define license headers per Visual Studio project and per file extension
* Share license headers between projects via "Add as Link"
* Add, remove and replace headers at any time for one or all files
* Put your license headers in #regions
* Use [Expendable Properties](https://github.com/rubicon-oss/LicenseHeaderManager/wiki/Expendable-Properties) like %FileName%, %Project% or %UserName% and many more, which are automatically filled everytime the Header is reinserted 


[Start right here!](https://github.com/rubicon-oss/LicenseHeaderManager/wiki)

Install License Header Manager from Visual Studio via Tools | Extensions and Updates | Online or directly from [Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=StefanWenig.LicenseHeaderManager)

ReSharper user? Take a look at [this](https://github.com/rubicon-oss/LicenseHeaderManager/wiki/License-Header-Manager-and-Resharper)!

Previously located at: https://licensemanager.codeplex.com/

There is also the `LicenseHeaderManager.Console` project that is a console application which can be used to insert, replace or remove license headers
via the command line. It can be supplied with a license header definition, a list of files or a directory as input and a JSON configuration file that controls
the behaviour of the `LicenseHeaderManager.Core` component. The code below illustrates its usage.

<details>
  <summary>Click to show help text</summary>

```text
> LicenseHeaderManager.Console.exe --help

LicenseHeaderManager.Console 1.0.0.0
Copyright © 2021

  -m, --mode                         (Default: Add) Specifies whether license headers should be added or removed. Must be one of {Add, Remove}, case-insensitive.

  -l, --license-header-definition    Required. The path to the license header definition file to be used for the update operations.

  -c, --configuration                The path to the JSON file that configures the behaviour of the Core component. If not present, default values will be used.

  -f, --files                        Paths to the files whose headers should be updated, separated by comma (','). Must not be present if "directory" is present.

  -d, --directory                    Path of the directory containing the files whose headers should be updated. Must not be present if "files" is present.

  -r, --recursive                    (Default: false) Specifies whether the directory represented by "directory" should be searched recursively. Ignored if "files" is present instead of "directory".

  --help                             Display this help screen.

  --version                          Display version information.

USAGE:
 Add license headers to one file with a custom configuration:
   LicenseHeaderManager.Console.exe --configuration CoreOptions.json --files file.cs --license-header-definition DefinitionFile.licenseheader
 Remove license headers from multiple files with standard configuration:
   LicenseHeaderManager.Console.exe --files file1.cs,file2.html,file3.xaml --license-header-definition DefinitionFile.licenseheader --mode Remove
 Add license headers to all files in a directory, but not its subdirectories, with custom configuration:
   LicenseHeaderManager.Console.exe --configuration CoreOptions.json --directory C:\SomeDirectory --license-header-definition DefinitionFile.licenseheader
 Remove license headers from all files in a directory and its subdirectories with standard configuration:
   LicenseHeaderManager.Console.exe --directory C:\SomeDirectory --license-header-definition DefinitionFile.licenseheader --mode Remove --recursive
```
</details>

The JSON options that can be supplied with the `--configuration` flag represent the configuration of the `Core` component.
The expander below shows a JSON representation of the default options that are used when the `--configuration` is not supplied.  

<details>
  <summary>Click to show default Core settings</summary>

  ```json
{
  "useRequiredKeywords": true,
  "requiredKeywords": "license, copyright, (c), ©",
  "licenseHeaderFileText": "extensions: .cs\r\n/* Copyright (c) rubicon IT GmbH\r\n *\r\n * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the \"Software\"),\r\n * to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,\r\n * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:\r\n * \r\n * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.\r\n * \r\n * THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS\r\n * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,\r\n * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. \r\n */\r\n\r\nextensions: .xaml\r\n<!--\r\nCopyright (c) rubicon IT GmbH\r\n\r\nPermission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the \"Software\"),\r\nto deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,\r\nand/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:\r\n\r\nThe above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.\r\n\r\nTHE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS\r\nFOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,\r\nWHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.\r\n-->",
  "languages": [
    {
      "extensions": [
        ".cs"
      ],
      "lineComment": "//",
      "beginComment": "/*",
      "endComment": "*/",
      "beginRegion": "#region",
      "endRegion": "#endregion",
      "skipExpression": ""
    },
    {
      "extensions": [
        ".c",
        ".cpp",
        ".cxx",
        ".h",
        ".hpp"
      ],
      "lineComment": "//",
      "beginComment": "/*",
      "endComment": "*/",
      "beginRegion": "",
      "endRegion": "",
      "skipExpression": ""
    },
    {
      "extensions": [
        ".vb"
      ],
      "lineComment": "'",
      "beginComment": "",
      "endComment": "",
      "beginRegion": "#Region",
      "endRegion": "#End Region",
      "skipExpression": ""
    },
    {
      "extensions": [
        ".aspx",
        ".ascx"
      ],
      "lineComment": "",
      "beginComment": "<%--",
      "endComment": "--%>",
      "beginRegion": "",
      "endRegion": "",
      "skipExpression": ""
    },
    {
      "extensions": [
        ".htm",
        ".html",
        ".xhtml",
        ".xml",
        ".xaml",
        ".resx",
        ".config",
        ".xsd"
      ],
      "lineComment": "",
      "beginComment": "<!--",
      "endComment": "-->",
      "beginRegion": "",
      "endRegion": "",
      "skipExpression": "(<\\?xml(.|\\s)*?\\?>)?(\\s*<!DOCTYPE(.|\\s)*?>)?( |\\t)*(\\n|\\r\\n|\\r)?"
    },
    {
      "extensions": [
        ".css"
      ],
      "lineComment": "",
      "beginComment": "/*",
      "endComment": "*/",
      "beginRegion": "",
      "endRegion": "",
      "skipExpression": ""
    },
    {
      "extensions": [
        ".js",
        ".ts"
      ],
      "lineComment": "//",
      "beginComment": "/*",
      "endComment": "*/",
      "beginRegion": "",
      "endRegion": "",
      "skipExpression": "(/// *<reference.*/>( |\\t)*(\\n|\\r\\n|\\r)?)*"
    },
    {
      "extensions": [
        ".sql"
      ],
      "lineComment": "--",
      "beginComment": "/*",
      "endComment": "*/",
      "beginRegion": "",
      "endRegion": "",
      "skipExpression": ""
    },
    {
      "extensions": [
        ".php"
      ],
      "lineComment": "//",
      "beginComment": "/*",
      "endComment": "*/",
      "beginRegion": "",
      "endRegion": "",
      "skipExpression": ""
    },
    {
      "extensions": [
        ".wxs",
        ".wxl",
        ".wxi"
      ],
      "lineComment": "",
      "beginComment": "<!--",
      "endComment": "-->",
      "beginRegion": "",
      "endRegion": "",
      "skipExpression": ""
    },
    {
      "extensions": [
        ".fs"
      ],
      "lineComment": "//",
      "beginComment": "(*",
      "endComment": "*)",
      "beginRegion": "",
      "endRegion": "",
      "skipExpression": ""
    },
    {
      "extensions": [
        ".cshtml",
        ".vbhtml"
      ],
      "lineComment": "",
      "beginComment": "@*",
      "endComment": "*@",
      "beginRegion": "",
      "endRegion": "",
      "skipExpression": ""
    },
    {
      "extensions": [
        ".py"
      ],
      "lineComment": "",
      "beginComment": "\"\"",
      "endComment": "\"\"",
      "beginRegion": null,
      "endRegion": null,
      "skipExpression": null
    }
  ]
}
  ```
</details>

# License
[MIT-License](license/MIT.txt)
