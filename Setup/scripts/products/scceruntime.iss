[CustomMessages]
ssceruntime_title=SQL Server Compact 3.5 Service Pack 2 for Windows Destkop
ssceruntime_unpack=SQL Server Compact 3.5 Service Pack 2 for Windows Destkop (uncompress package)

ssceruntime_size=5.3 MB


[Code]
const
	ssceruntime_url = 'http://download.microsoft.com/download/E/C/1/EC1B2340-67A0-4B87-85F0-74D987A27160/SSCERuntime-ENU.exe';

procedure ssceruntime();
var
	version: cardinal;
begin
	if not RegKeyExists(HKLM, 'SOFTWARE\Microsoft\Microsoft SQL Server Compact Edition\v3.5') then
   	AddProduct('ssceruntime.msi',
	  	'/qb',
	    CustomMessage('ssceruntime_title'),
      CustomMessage('ssceruntime_size'),
	    ssceruntime_url,false,false);
end;
