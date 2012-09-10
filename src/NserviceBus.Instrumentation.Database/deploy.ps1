"" > sql.log

if ($Env:SifSqlServer) { $servername = $Env:SifSqlServer; } else { $servername = "ramqtdev03.rugby.com.au,4656"; }
if ($Env:SifSqlUser) { $username = $Env:SifSqlUser; } else { $username = "sa"; }
if ($Env:SifSqlPassword) { $password = $Env:SifSqlPassword; } else { $password = "P@ssw0rd"; }

Get-ChildItem .\Tables -Recurse | Where-Object { $_.Name.EndsWith(".sql") } | Sort-Object PSParentPath | foreach {
		Write-Host $_.Name; 

		& "C:\Program Files\Microsoft SQL Server\100\Tools\Binn\sqlcmd.exe" -S $servername -U $username -P $password -I -i $_.FullName -e -b | Out-File sql.log -Append
	}

Get-ChildItem .\Procedures -Recurse | Where-Object { $_.Name.EndsWith(".sql") } | Sort-Object PSParentPath | foreach {
		Write-Host $_.Name; 

		 & "C:\Program Files\Microsoft SQL Server\100\Tools\Binn\sqlcmd.exe" -S $servername -U $username -P $password -I -i $_.FullName -e -b | Out-File sql.log -Append
	}

