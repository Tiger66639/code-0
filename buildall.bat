echo build the designer
devenv Designer.sln /rebuild Release 
devenv designer.sln /rebuild "Release PRO" 

echo build Aici android
rem devenv /rebuild Release Hosts\AiciAndroid\AiciAndroid.sln
echo build the package
rem msbuild.exe Hosts\AiciAndroid\AiciAndroid.csproj /p:Configuration=Release /t:PackageForAndroid 
echo signing the package
rem "C:\Program Files\Java\jdk1.6.0_31\bin\jarsigner" -verbose -storepass Funkmaster1  -keystore Hosts\AiciAndroid\JaStDev_Aici.keystore Hosts\AiciAndroid\bin\Release\com.bragisoft.aici.apk JaStDev_Aici_Key 
echo allign package
rem rename Hosts\AiciAndroid\bin\Release\com.bragisoft.aici.apk renameMe.apk
rem "C:\Users\Bragi\AppData\Local\Android\android-sdk\tools\zipalign" -v 4 Hosts\AiciAndroid\bin\Release\renameMe.apk Hosts\AiciAndroid\bin\Release\com.bragisoft.aici.apk
rem del Hosts\AiciAndroid\bin\Release\renameMe.apk
echo copying android package to bins
copy Hosts\AiciAndroid\bin\Release\com.bragisoft.aici.apk  Designer\bin\Debug\com.bragisoft.aici.apk
copy Hosts\AiciAndroid\bin\Release\com.bragisoft.aici.apk  Designer\bin\Release\com.bragisoft.aici.apk
copy Hosts\AiciAndroid\bin\Release\com.bragisoft.aici.apk  "Designer\bin\Debug PRO\com.bragisoft.aici.apk"
copy Hosts\AiciAndroid\bin\Release\com.bragisoft.aici.apk  "Designer\bin\Release PRO\com.bragisoft.aici.apk"
copy Hosts\AiciAndroid\bin\Release\com.bragisoft.aici.apk  Setup\bin\com.bragisoft.aici.apk

echo building website
del Hosts\mvc3\bin\*.* /q /s 
rd Hosts\mvc3\bin\ /q /s
md Hosts\mvc3\bin\
rem devenv /rebuild Release Hosts\mvc3\Aici.sln
devenv Hosts\mvc3\Aici.sln

echo Do the publish now
pause

echo copying site.css to build dirs.
copy Hosts\mvc3\bin\content\site.css  Designer\bin\Debug\DefaultData\site.css
copy Hosts\mvc3\bin\content\site.css  Designer\bin\Release\DefaultData\site.css
copy Hosts\mvc3\bin\content\site.css  "Designer\bin\Debug PRO\DefaultData\site.css"
copy Hosts\mvc3\bin\content\site.css  "Designer\bin\Release PRO\DefaultData\site.css"

cd Hosts\mvc3\bin\  
"C:\Program Files\7-Zip\7z" a -tzip -r online.zip *
cd ..\..\..


echo copying online.zip to build dirs
copy Hosts\mvc3\bin\online.zip  Designer\bin\Debug\online.zip
copy Hosts\mvc3\bin\online.zip  Designer\bin\Release\online.zip
copy Hosts\mvc3\bin\online.zip  "Designer\bin\Debug PRO\online.zip"
copy Hosts\mvc3\bin\online.zip  "Designer\bin\Release PRO\online.zip"
copy Hosts\mvc3\bin\online.zip  Setup\bin\online.zip

echo adjust installer version number
"C:\Program Files\Inno Setup 5\compil32"  "Setup\CBDdesigner.iss" 
pause
echo creating setup application
"C:\Program Files\Inno Setup 5\iscc" /dVERDESIGNER "Setup\CBDdesigner.iss" 
"C:\Program Files\Inno Setup 5\iscc" /dVERPRO "Setup\CBDdesigner.iss" 

echo all compiled, press a key to start uploading.

rem ftp Bragisoft.com /s:UploadAll.txt

