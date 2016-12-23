#tool "nuget:?package=GitVersion.CommandLine"
#tool "nuget:?package=OpenCover"
#tool "nuget:?package=ReportGenerator"

var compileConfig = Argument("configuration", "Debug");
var target = Argument("target", "Default");
var artifactsDir = Directory("Artifacts");
var projectJson = "./src/BinaryMash.Responses/project.json";

// version
var _nugetVersion = "0.0.0";

// unit testing
var artifactsForUnitTestsDir = artifactsDir + Directory("UnitTests");
var unitTestAssemblies = @"./src/BinaryMash.Responses.Tests";
var openCoverSettings = new OpenCoverSettings();
var minCodeCoverage = 95d;

Task("Default")
	.IsDependentOn("RunUnitTestsCoverageReport")
	.Does(() =>
	{
	});

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
		_nugetVersion = GetVersion();
		Information("SemVer version number: " + _nugetVersion);

		if (AppVeyor.IsRunningOnAppVeyor)
		{
			Information("Persisting version number...");
			PersistVersion(_nugetVersion);
		}
		else
		{
			Information("We are not running on build server, so we won't persist the version number.");
		}
	});

Task("Compile")
	.IsDependentOn("Clean")
	.IsDependentOn("Version")
	.Does(() =>
	{
		Information("Build configuration is " + compileConfig);
		
		var buildSettings = new DotNetCoreBuildSettings
		{
			Configuration = compileConfig,
			VersionSuffix = _nugetVersion
		};
		
		DotNetCoreRestore();
		DotNetCoreBuild(@"src\BinaryMash.Responses", buildSettings);
	});

Task("RunUnitTests")
	.IsDependentOn("Compile")
	.Does(() =>
	{
		DotNetCoreTest(unitTestAssemblies);
	});

Task("RunUnitTestsCoverageReport")
	.IsDependentOn("RunUnitTests")
	.Does(() =>
	{
		// For now we run the unit tests directly with dotnet test, then run them again via
		// opencover. This is because opencover doesn't make it easy to fail the build when 
		// tests fail

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
				ArgumentCustomization=args=>args.Append(@"-oldstyle")
			}
			.WithFilter("+[BinaryMash.*]*")
			.WithFilter("-[xunit*]*")
			.WithFilter("-[BinaryMash.*.Tests]*")
		);
		
		ReportGenerator(coverageSummaryFile, artifactsForUnitTestsDir);
		
		var sequenceCoverage = XmlPeek(coverageSummaryFile, "//CoverageSession/Summary/@sequenceCoverage");
		var branchCoverage = XmlPeek(coverageSummaryFile, "//CoverageSession/Summary/@branchCoverage");

		Information("Sequence Coverage: " + sequenceCoverage);
		
		if(double.Parse(sequenceCoverage) < minCodeCoverage)
		{
			throw new Exception(string.Format("Code coverage fell below the threshold of {0}%", minCodeCoverage));
		};
	});

RunTarget(target);

private string GetVersion()
{
        GitVersion(new GitVersionSettings{
            UpdateAssemblyInfo = false,
            OutputType = GitVersionOutput.BuildServer
        });

        var versionInfo = GitVersion(new GitVersionSettings{ OutputType = GitVersionOutput.Json });
		return versionInfo.NuGetVersion;
}

private void PersistVersion(string version)
{
	var updatedProjectJson = System.IO.File.ReadAllText(projectJson)
		.Replace("0.0.0-dev", _nugetVersion);

	System.IO.File.WriteAllText(projectJson, updatedProjectJson);
}