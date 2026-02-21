@echo off
cd /d "%~dp0"

set CSC=D:\VS\MSBuild\Current\Bin\Roslyn\csc.exe

%CSC% /noconfig /target:winexe /out:bin\Debug\PicSell_new.exe ^
  /r:System.dll ^
  /r:System.Core.dll ^
  /r:System.Drawing.dll ^
  /r:System.Windows.Forms.dll ^
  /r:System.Data.dll ^
  /r:System.Data.DataSetExtensions.dll ^
  /r:Microsoft.CSharp.dll ^
  /r:System.Xml.dll ^
  /r:System.Xml.Linq.dll ^
  /r:System.Net.Http.dll ^
  /r:System.Deployment.dll ^
  /r:System.Management.dll ^
  /r:System.ComponentModel.DataAnnotations.dll ^
  /r:bin\Debug\EntityFramework.dll ^
  /r:bin\Debug\EntityFramework.SqlServer.dll ^
  /r:bin\Debug\INIFileParser.dll ^
  /r:bin\Debug\PluginBase.dll ^
  /r:bin\Debug\System.Data.SQLite.dll ^
  /r:bin\Debug\System.Data.SQLite.EF6.dll ^
  /r:bin\Debug\System.Data.SQLite.Linq.dll ^
  /warn:0 ^
  Program.cs MainForm.cs MainForm.Designer.cs DarkTheme.cs Pixel.cs LicenseManager.cs ^
  AboutBox.cs AboutBox.Designer.cs BackgroundEditorForm.cs ^
  PluginsLoadForm.cs PluginsLoadForm.Designer.cs ^
  PromptThreadForm.cs PromptThreadForm.Designer.cs ^
  StatisticForm.cs StatisticForm.Designer.cs ^
  TextEditorForm.cs ^
  Properties\AssemblyInfo.cs Properties\Resources.Designer.cs Properties\Settings.Designer.cs

echo Exit code: %ERRORLEVEL%
