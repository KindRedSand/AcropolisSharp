@echo off
echo Start build for linux-arm64
dotnet publish --framework net8.0 --runtime linux-arm64 --sc
echo Creating artifacts for linux-arm64
"C:\Program Files\7-Zip\7z.exe" a -tzip -ssw -mx9 -r0 -xr!*.pdb .\bin\Release\acropolis-linux-arm64.zip .\bin\Release\net8.0\linux-arm64\publish\*
echo Start build for linux-arm
dotnet publish --framework net8.0 --runtime linux-arm --sc
echo Creating artifacts for linux-arm
"C:\Program Files\7-Zip\7z.exe" a -tzip -ssw -mx9 -r0 -xr!*.pdb .\bin\Release\acropolis-linux-arm.zip .\bin\Release\net8.0\linux-arm\publish\*
echo Start build for linux-x64
dotnet publish --framework net8.0 --runtime linux-x64 --sc
echo Creating artifacts for linux-x64
"C:\Program Files\7-Zip\7z.exe" a -tzip -ssw -mx9 -r0 -xr!*.pdb .\bin\Release\acropolis-linux-x64.zip .\bin\Release\net8.0\linux-x64\publish\*
echo Start build for win-x64
dotnet publish --framework net8.0 --runtime win-x64 --sc
echo Creating artifacts for win-x64
"C:\Program Files\7-Zip\7z.exe" a -tzip -ssw -mx9 -r0 -xr!*.pdb .\bin\Release\acropolis-win-x64.zip .\bin\Release\net8.0\win-x64\publish\*