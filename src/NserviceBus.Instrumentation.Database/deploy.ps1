"" > sql.log

$sqlcmdpath = "C:\Program Files\Microsoft SQL Server\100\Tools\Binn\sqlcmd.exe";
$servername = "";
$username = "";
$password = "";

Get-ChildItem .\Tables -Recurse | Where-Object { $_.Name.EndsWith(".sql") } | Sort-Object PSParentPath | foreach {
		Write-Host $_.Name; 

		& $sqlcmdpath -S $servername -U $username -P $password -I -i $_.FullName -e -b | Out-File sql.log -Append
	}

Get-ChildItem .\Procedures -Recurse | Where-Object { $_.Name.EndsWith(".sql") } | Sort-Object PSParentPath | foreach {
		Write-Host $_.Name; 

		 & $sqlcmdpath -S $servername -U $username -P $password -I -i $_.FullName -e -b | Out-File sql.log -Append
	}

