set WshShell = CreateObject("Wscript.shell")
strDesktop = WshShell.SpecialFolders("Desktop")
strLocalAppData = WshShell.ExpandEnvironmentStrings("%LOCALAPPDATA%")
set oShellLink = WshShell.CreateShortcut(strDesktop + "\Учет картриджей.lnk" )
oShellLink.TargetPath = strLocalAppData + "\Деловые Линии\Учет картриджей\CartAccClient.exe"
oShellLink.WindowStyle = 1
oShellLink.WorkingDirectory = strLocalAppData + "\Деловые Линии\Учет картриджей\"
oShellLink.IconLocation = strLocalAppData + "\Деловые Линии\Учет картриджей\Config\App.ico"
oShellLink.Save