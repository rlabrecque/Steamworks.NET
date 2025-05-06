First build binaries of all supported platform. Run command below in your terminal.
```bat
dotnet build -t:BatchBuild -p:SNetDoingNugetBuild=true -p:Version={CurrentVersion} Steamworks.NET.Standard.csproj
```

Then use nuget client(not the one included in dotnet SDK) with the `.nuspec` to pack an all-in-one package. 
```bat
nuget pack Steamworks.NET.nuspec -OutputDirectory bin/packages
```
