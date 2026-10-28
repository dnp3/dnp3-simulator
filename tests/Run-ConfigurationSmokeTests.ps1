param([string]$MSBuild = 'C:\Program Files\Microsoft Visual Studio\18\Professional\MSBuild\Current\Bin\MSBuild.exe')
$ErrorActionPreference = 'Stop'
$repository = Split-Path $PSScriptRoot -Parent
& $MSBuild (Join-Path $repository 'simulator.sln') /t:Build /p:Configuration=Release '/p:Platform=Any CPU' /v:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
$output = Join-Path $repository 'simulator\Simulator\bin\x86\Release'
$references = @('SimulatorApplication.exe', 'SimulatorAPI.dll', 'SimulatorDNP3.dll', 'SimulatorCommons.dll',
    'DNP3CLRInterface.dll', 'DNP3PluginAPI.dll', 'DefaultOutstationPlugin.dll') | ForEach-Object { '/r:' + (Join-Path $output $_) }
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe'
& $compiler /nologo /target:exe /platform:x86 "/out:$output\ConfigurationSmokeTests.exe" /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:System.Xml.Linq.dll /r:System.Runtime.Serialization.dll $references (Join-Path $PSScriptRoot 'ConfigurationSmokeTests.cs')
if ($LASTEXITCODE -ne 0) { throw 'Test compilation failed.' }
& (Join-Path $output 'ConfigurationSmokeTests.exe')
if ($LASTEXITCODE -ne 0) { throw 'Configuration tests failed.' }
