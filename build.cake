#addin "Cake.ExtendedNuGet"

var MyGetKey = EnvironmentVariable("MYGET_KEY");
var BuildNumber = EnvironmentVariable("TRAVIS_BUILD_NUMBER");

Task("Restore")
    .Does(() =>
{
    DotNetCoreRestore();
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
    DotNetCorePack("./src/Discord.Addons.InteractiveCommands", settings);
    DotNetCoreBuild("./src/Example");
});
Task("Deploy")
    .Does(() =>
{
    var settings = new NuGetPushSettings
    {
        Source = "https://www.myget.org/F/discord-net/api/v2/package",
        ApiKey = "99291d2b-b3e0-42e7-8a49-aaf611d586b1"
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