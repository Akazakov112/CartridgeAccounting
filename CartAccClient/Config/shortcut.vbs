set WshShell = CreateObject("Wscript.shell")
strDesktop = WshShell.SpecialFolders("Desktop")
strLocalAppData = WshShell.ExpandEnvironmentStrings("%LOCALAPPDATA%")
set oShellLink = WshShell.CreateShortcut(strDesktop + "\���� ����������.lnk" )
oShellLink.TargetPath = strLocalAppData + "\������� �����\���� ����������\CartAccClient.exe"
oShellLink.WindowStyle = 1
oShellLink.WorkingDirectory = strLocalAppData + "\������� �����\���� ����������\"
oShellLink.IconLocation = strLocalAppData + "\������� �����\���� ����������\Config\App.ico"
oShellLink.Save