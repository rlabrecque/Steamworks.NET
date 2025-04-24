First build binaries of all supported platform. Enter
```bat
dotnet build -t:BatchBuild -p:SNetDoingNugetBuild=true -p:Version={CurrentVersion} Steamworks.NET.Standard.sln
```
to your terminal.

Then use nuget client(not the one included in dotnet SDK) with the `.nuspec` to pack an all-in-one package. Enter
```bat
nuget pack Steamworks.NET.nuspec -OutputDirectory bin\
```
to your terminal.