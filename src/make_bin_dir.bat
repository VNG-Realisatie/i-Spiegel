echo We gaan dingen verwijderen en copieren, weet je het zeker, druk dan op een knop! Zoniet, sluit dit venster dan maar snel!
pause

del /P /F /S /Q ..\bin\ISpiegelx86\*.*
del /P /F /S /Q ..\bin\ISpiegelx86
del /P /F /S /Q ..\bin\ISpiegelx64\*.*
del /P /F /S /Q ..\bin\ISpiegelx64

mkdir ..\bin\ISpiegelx86
mkdir ..\bin\ISpiegelx64

copy ..\templates\*.accdb ..\bin\ISpiegelx86
copy ..\templates\*.accdb ..\bin\ISpiegelx64
copy ..\templates\*.csv ..\bin\ISpiegelx86
copy ..\templates\*.csv ..\bin\ISpiegelx64

copy ISpiegel\bin\x86\Release\ISpiegel.exe ..\bin\ISpiegelx86\ISpiegel.exe
copy ISpiegel\bin\x64\Release\ISpiegel.exe ..\bin\ISpiegelx64\ISpiegel64.exe

copy ..\templates\ISpiegel.exe.config ..\bin\ISpiegelx86\ISpiegel.exe.config
copy ..\templates\ISpiegel.exe.config ..\bin\ISpiegelx64\ISpiegel64.exe.config

copy ISpiegel\bin\x86\Release\*.dll ..\bin\ISpiegelx86\
copy ISpiegel\bin\x64\Release\*.dll ..\bin\ISpiegelx64\