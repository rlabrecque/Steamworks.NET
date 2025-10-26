First build binaries of all supported platform. Enter
```bat
dotnet build -t:BatchBuild Steamworks.NET.Standard.sln
```
to your terminal.

Then use nuget client with the `.nuspec` to pack an all-in-one package. Enter
```bat
nuget pack Steamworks.NET.nuspec -OutputDirectory bin\
```
to your terminal.