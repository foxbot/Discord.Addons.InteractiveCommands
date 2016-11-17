#addin nuget:?package=NuGet.Core
#addin "Cake.ExtendedNuGet"

var MyGetKey = EnvironmentVariable("MYGET_KEY");
var BuildNumber = EnvironmentVariable("TRAVIS_BUILD_NUMBER");

Task("Restore")
    .Does(() =>
{
    var settings = new DotNetCoreRestoreSettings
    {
        Sources = new[] { "https://www.myget.org/F/discord-net/api/v2", "https://www.nuget.org/api/v2" }
    };
    DotNetCoreRestore(settings);
});
Task("Build")
    .Does(() =>
{
    var settings = new DotNetCorePackSettings
    {
        Configuration = "Release",
        OutputDirectory = "./artifacts/",
        VersionSuffix = BuildNumber
    };
    DotNetCorePack("./src/Discord.Addons.InteractiveCommands/", settings);
    DotNetCoreBuild("./src/Example/");
});
Task("Deploy")
    .Does(() =>
{
    var settings = new NuGetPushSettings
    {
        Source = "https://www.myget.org/F/discord-net/api/v2/package",
        ApiKey = MyGetKey
    };
    var packages = GetFiles("./artifacts/*.nupkg");
    NuGetPush(packages, settings);
});

Task("Default")
    .IsDependentOn("Restore")
    .IsDependentOn("Build")
    .IsDependentOn("Deploy")
    .Does(() => 
{
    Information("Build Succeeded");
});

RunTarget("Default");