$destinationFolder = "C:\Users\Andrey_Vernigora\Documents\WindowsPowerShell\Modules\PSQuickGraph"
$docsFolder = Get-Item "..\docs"


dir $destinationFolder | Remove-Item -Force
copy .\bin\Release\*.dll $destinationFolder
copy .\bin\Release\*.pdb $destinationFolder
copy .\bin\Release\PSQuickGraph.psd1 $destinationFolder

Import-Module platyPS
Import-Module PSQuickGraph
New-ExternalHelp $docsFolder -OutputPath (Join-Path -Path $destinationFolder -Childpath 'en-US')