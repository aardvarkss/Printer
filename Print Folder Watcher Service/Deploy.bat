net stop "Meticulus Print Folder Watcher Service"

copy bin\Debug\* "%ProgramFiles%\Meticulus\Print Folder Watcher"

net start "Meticulus Print Folder Watcher Service"

pause