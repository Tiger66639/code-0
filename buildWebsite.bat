echo building website
del C:\Users\Esus\Documents\dev\VS\Projects\AI\Hosts\mvc3\bin\*.* /q /s 
rd C:\Users\Esus\Documents\dev\VS\Projects\AI\Hosts\mvc3\bin\ /q /s
md C:\Users\Esus\Documents\dev\VS\Projects\AI\Hosts\mvc3\bin\
rem devenv /rebuild Release C:\Users\Esus\Documents\dev\VS\Projects\AI\Hosts\mvc3\Aici.sln
devenv C:\Users\Esus\Documents\dev\VS\Projects\AI\Hosts\mvc3\Aici.sln

echo Do the publish now
pause

echo copying site.css to build dirs.
copy C:\Users\Esus\Documents\dev\VS\Projects\AI\Hosts\mvc3\bin\content\site.css  C:\Users\Esus\Documents\dev\VS\Projects\AI\Designer\bin\Debug\DefaultData\site.css
copy C:\Users\Esus\Documents\dev\VS\Projects\AI\Hosts\mvc3\bin\content\site.css  C:\Users\Esus\Documents\dev\VS\Projects\AI\Designer\bin\Release\DefaultData\site.css
copy C:\Users\Esus\Documents\dev\VS\Projects\AI\Hosts\mvc3\bin\content\site.css  "C:\Users\Esus\Documents\dev\VS\Projects\AI\Designer\bin\Debug PRO\DefaultData\site.css"
copy C:\Users\Esus\Documents\dev\VS\Projects\AI\Hosts\mvc3\bin\content\site.css  "C:\Users\Esus\Documents\dev\VS\Projects\AI\Designer\bin\Release PRO\DefaultData\site.css"

"C:\Program Files\7-Zip\7z" a -tzip C:\Users\Esus\Documents\dev\VS\Projects\AI\Hosts\mvc3\bin\online.zip C:\Users\Esus\Documents\dev\VS\Projects\AI\Hosts\mvc3\bin\* -r

echo copying online.zip to build dirs
copy C:\Users\Esus\Documents\dev\VS\Projects\AI\Hosts\mvc3\bin\online.zip  C:\Users\Esus\Documents\dev\VS\Projects\AI\Designer\bin\Debug\online.zip
copy C:\Users\Esus\Documents\dev\VS\Projects\AI\Hosts\mvc3\bin\online.zip  C:\Users\Esus\Documents\dev\VS\Projects\AI\Designer\bin\Release\online.zip
copy C:\Users\Esus\Documents\dev\VS\Projects\AI\Hosts\mvc3\bin\online.zip  "C:\Users\Esus\Documents\dev\VS\Projects\AI\Designer\bin\Debug PRO\online.zip"
copy C:\Users\Esus\Documents\dev\VS\Projects\AI\Hosts\mvc3\bin\online.zip  "C:\Users\Esus\Documents\dev\VS\Projects\AI\Designer\bin\Release PRO\online.zip"
