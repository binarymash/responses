﻿#tool "nuget:?package=GitVersion.CommandLine"
#tool "nuget:?package=GitReleaseNotes"

#tool "nuget:?package=OpenCover"
#tool "nuget:?package=ReportGenerator"

#addin "nuget:?package=Cake.Json"

#tool coveralls.net
#addin Cake.Coveralls


// compile
var compileConfig = Argument("configuration", "Release");
var slnFile = "./BinaryMash.Responses.sln";

// build artifacts
var artifactsDir = Directory("artifacts");

// unit testing
var artifactsForUnitTestsDir = artifactsDir + Directory("UnitTests");
var unitTestAssemblies = @"./src/BinaryMash.Responses.Tests/BinaryMash.Responses.Tests.csproj";
var openCoverSettings = new OpenCoverSettings();
var minCodeCoverage = 95d;
var coverallsRepoToken = "coveralls-repo-token-responses";

// packaging
var packagesDir = artifactsDir + Directory("Packages");
var releaseNotesFile = packagesDir + File("releasenotes.md");
var artifactsFile = packagesDir + File("artifacts.txt");

// unstable releases
var nugetFeedUnstableKey = EnvironmentVariable("nuget-apikey-unstable");
var nugetFeedUnstableUploadUrl = "https://www.myget.org/F/binarymash-unstable/api/v2/package";
var nugetFeedUnstableSymbolsUploadUrl = "https://www.myget.org/F/binarymash-unstable/symbols/api/v2/package";
var nugetFeedUnstableBranchFilter = "^(develop)$|^(PullRequest/)";

// stable releases
var tagsUrl = "https://api.github.com/repos/binarymash/responses/releases/tags/";
var nugetFeedStableKey = EnvironmentVariable("nuget-apikey-stable");
var nugetFeedStableUploadUrl = "https://www.nuget.org/api/v2/package";
var nugetFeedStableSymbolsUploadUrl = "https://www.nuget.org/api/v2/package";

// internal build variables - don't change these.
var releaseTag = "";
GitVersion versioning = null;
var committedVersion = "0.0.0-dev";
var buildVersion = committedVersion;


var target = Argument("target", "Default");

Information("target is " +target);
Information("Build configuration is " + compileConfig);	

Task("Default")
	.IsDependentOn("Build");

Task("Build")
	.IsDependentOn("RunTests")
	.IsDependentOn("CreatePackages");

Task("BuildAndReleaseUnstable")
	.IsDependentOn("Build")
	.IsDependentOn("ReleasePackagesToUnstableFeed");
	
Task("Clean")
	.Does(() =>
	{
        if (DirectoryExists(artifactsDir))
        {
            DeleteDirectory(artifactsDir, recursive:true);
        }
        CreateDirectory(artifactsDir);
	});
	
Task("Version")
	.Does(() =>
	{
		versioning = GetNuGetVersionForCommit();
		var nugetVersion = versioning.NuGetVersion;
		Information("SemVer version number: " + nugetVersion);

		if (AppVeyor.IsRunningOnAppVeyor)
		{
			Information("Persisting version number...");
			PersistVersion(committedVersion, nugetVersion);
			buildVersion = nugetVersion;
		}
		else
		{
			Information("We are not running on build server, so we won't persist the version number.");
		}
	});

Task("Restore")
	.IsDependentOn("Clean")
	.IsDependentOn("Version")
	.Does(() =>
	{	
		DotNetCoreRestore(slnFile);
	});

Task("Compile")
	.IsDependentOn("Restore")
	.Does(() =>
	{	
		var settings = new DotNetCoreBuildSettings
		{
			Configuration = compileConfig,
		};
		
		DotNetCoreBuild(slnFile, settings);
	});

Task("RunUnitTestsCoverageReport")
	.IsDependentOn("Compile")
	.Does(context =>
	{
        var coverageSummaryFile = artifactsForUnitTestsDir + File("coverage.xml");
        
        EnsureDirectoryExists(artifactsForUnitTestsDir);
        
        OpenCover(tool => 
            {
                tool.DotNetCoreTest(unitTestAssemblies);
            },
            new FilePath(coverageSummaryFile),
            new OpenCoverSettings()
            {
                Register="user",
                ArgumentCustomization=args=>args.Append(@"-oldstyle -returntargetcode")
            }
            .WithFilter("+[BinaryMash.*]*")
            .WithFilter("-[xunit*]*")
            .WithFilter("-[BinaryMash.*.Tests]*")
        );
        
        ReportGenerator(coverageSummaryFile, artifactsForUnitTestsDir);
		
		if (AppVeyor.IsRunningOnAppVeyor)
		{
			var repoToken = EnvironmentVariable(coverallsRepoToken);
			if (string.IsNullOrEmpty(repoToken))
			{
				throw new Exception(string.Format("Coveralls repo token not found. Set environment variable '{0}'", coverallsRepoToken));
			}

			Information("Uploading test coverage to coveralls.io");
			CoverallsNet(coverageSummaryFile, CoverallsNetReportType.OpenCover, new CoverallsNetSettings()
			{
				RepoToken = repoToken
			});
		}
		else
		{
			Information("We are not running on the build server so we won't publish the coverage report to coveralls.io");
		}

		var sequenceCoverage = XmlPeek(coverageSummaryFile, "//CoverageSession/Summary/@sequenceCoverage");
		var branchCoverage = XmlPeek(coverageSummaryFile, "//CoverageSession/Summary/@branchCoverage");

		Information("Sequence Coverage: " + sequenceCoverage);
		
		if(double.Parse(sequenceCoverage) < minCodeCoverage)
		{
			throw new Exception(string.Format("Code coverage fell below the threshold of {0}%", minCodeCoverage));
		};
	});

Task("RunTests")
	.IsDependentOn("RunUnitTestsCoverageReport");

Task("CreatePackages")
	.IsDependentOn("Compile")
	.Does(() => 
	{
		EnsureDirectoryExists(packagesDir);
		CopyFiles("./src/**/BinaryMash.Responses.*.nupkg", packagesDir);

		GenerateReleaseNotes(releaseNotesFile);

        System.IO.File.WriteAllLines(artifactsFile, new[]{
            "nuget:BinaryMash.Responses." + buildVersion + ".nupkg",
//            "nugetSymbols:BinaryMash.Responses." + buildVersion + ".symbols.nupkg",
            "releaseNotes:releasenotes.md"
        });

		if (AppVeyor.IsRunningOnAppVeyor)
		{
			var path = packagesDir.ToString() + @"/**/*";

			foreach (var file in GetFiles(path))
			{
				AppVeyor.UploadArtifact(file.FullPath);
			}
		}
	});

Task("ReleasePackagesToUnstableFeed")
	.IsDependentOn("CreatePackages")
	.Does(() =>
	{
		if (ShouldPublishToUnstableFeed(nugetFeedUnstableBranchFilter, versioning.BranchName))
		{
			PublishPackages(packagesDir, artifactsFile, nugetFeedUnstableKey, nugetFeedUnstableUploadUrl, nugetFeedUnstableSymbolsUploadUrl);
		}
	});

Task("EnsureStableReleaseRequirements")
    .Does(() =>
    {
        if (!AppVeyor.IsRunningOnAppVeyor)
		{
           throw new Exception("Stable release should happen via appveyor");
		}
        
		var isTag =
           AppVeyor.Environment.Repository.Tag.IsTag &&
           !string.IsNullOrWhiteSpace(AppVeyor.Environment.Repository.Tag.Name);

        if (!isTag)
		{
           throw new Exception("Stable release should happen from a published GitHub release");
		}
    });

Task("UpdateVersionInfo")
    .IsDependentOn("EnsureStableReleaseRequirements")
    .Does(() =>
    {
        releaseTag = AppVeyor.Environment.Repository.Tag.Name;
        AppVeyor.UpdateBuildVersion(releaseTag);
    });

Task("DownloadGitHubReleaseArtifacts")
    .IsDependentOn("UpdateVersionInfo")
    .Does(() =>
    {
        EnsureDirectoryExists(packagesDir);

		var releaseUrl = tagsUrl + releaseTag;
        var assets_url = ParseJson(GetResource(releaseUrl))
            .GetValue("assets_url")
			.Value<string>();

        foreach(var asset in DeserializeJson<JArray>(GetResource(assets_url)))
        {
			var file = packagesDir + File(asset.Value<string>("name"));
			Information("Downloading " + file);
            DownloadFile(asset.Value<string>("browser_download_url"), file);
        }
    });

Task("ReleasePackagesToStableFeed")
    .IsDependentOn("DownloadGitHubReleaseArtifacts")
    .Does(() =>
    {
		PublishPackages(packagesDir, artifactsFile, nugetFeedStableKey, nugetFeedStableUploadUrl, nugetFeedStableSymbolsUploadUrl);
    });

Task("Release")
    .IsDependentOn("ReleasePackagesToStableFeed");

RunTarget(target);

/// Gets nuique nuget version for this commit
private GitVersion GetNuGetVersionForCommit()
{
    GitVersion(new GitVersionSettings{
        UpdateAssemblyInfo = false,
        OutputType = GitVersionOutput.BuildServer
    });

    return GitVersion(new GitVersionSettings{ OutputType = GitVersionOutput.Json });
}

/// Updates project version in all of our projects
private void PersistVersion(string committedVersion, string newVersion)
{
	Information(string.Format("We'll search all csproj files for {0} and replace with {1}...", committedVersion, newVersion));

	var projectFiles = GetFiles("./**/*.csproj");

	foreach(var projectFile in projectFiles)
	{
		var file = projectFile.ToString();
 
		Information(string.Format("Updating {0}...", file));

		var updatedProjectFile = System.IO.File.ReadAllText(file)
			.Replace(committedVersion, newVersion);

		System.IO.File.WriteAllText(file, updatedProjectFile);
	}
}

/// generates release notes based on issues closed in GitHub since the last release
private void GenerateReleaseNotes(ConvertableFilePath releaseNotesFile)
{
	if(!IsRunningOnWindows())
	{
		Warning("We are not running on Windows so we cannot generate release notes.");
		return;
	}

	Information("Generating release notes at " + releaseNotesFile);

    var releaseNotesExitCode = StartProcess(
        @"tools/GitReleaseNotes/tools/gitreleasenotes.exe", 
        new ProcessSettings { Arguments = ". /o " + releaseNotesFile });

    if (string.IsNullOrEmpty(System.IO.File.ReadAllText(releaseNotesFile)))
	{
        System.IO.File.WriteAllText(releaseNotesFile, "No issues closed since last release");
	}

    if (releaseNotesExitCode != 0) 
	{
		throw new Exception("Failed to generate release notes");
	}
}

/// Publishes code and symbols packages to nuget feed, based on contents of artifacts file
private void PublishPackages(ConvertableDirectoryPath packagesDir, ConvertableFilePath artifactsFile, string feedApiKey, string codeFeedUrl, string symbolFeedUrl)
{
        var artifacts = System.IO.File
            .ReadAllLines(artifactsFile)
            .Select(l => l.Split(':'))
            .ToDictionary(v => v[0], v => v[1]);

		var codePackage = packagesDir + File(artifacts["nuget"]);
//		var symbolsPackage = packagesDir + File(artifacts["nugetSymbols"]);

        NuGetPush(
            codePackage,
            new NuGetPushSettings {
                ApiKey = feedApiKey,
                Source = codeFeedUrl
            });

//        NuGetPush(
//            symbolsPackage,
//            new NuGetPushSettings {
//                ApiKey = feedApiKey,
//                Source = symbolFeedUrl
//            });

}

/// gets the resource from the specified url
private string GetResource(string url)
{
	Information("Getting resource from " + url);

    var assetsRequest = System.Net.WebRequest.CreateHttp(url);
    assetsRequest.Method = "GET";
    assetsRequest.Accept = "application/vnd.github.v3+json";
    assetsRequest.UserAgent = "BuildScript";

    using (var assetsResponse = assetsRequest.GetResponse())
    {
        var assetsStream = assetsResponse.GetResponseStream();
        var assetsReader = new StreamReader(assetsStream);
        return assetsReader.ReadToEnd();
    }
}

private bool ShouldPublishToUnstableFeed(string filter, string branchName)
{
	var regex = new System.Text.RegularExpressions.Regex(filter);
	var publish = regex.IsMatch(branchName);
	if (publish)
	{
		Information("Branch " + branchName + " will be published to the unstable feed");
	}
	else
	{
		Information("Branch " + branchName + " will not be published to the unstable feed");
	}
	return publish;	
}