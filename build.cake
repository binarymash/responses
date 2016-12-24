#tool "nuget:?package=GitVersion.CommandLine"
#tool "nuget:?package=OpenCover"
#tool "nuget:?package=ReportGenerator"
#tool "nuget:?package=GitReleaseNotes"

var target = Argument("target", "Default");
var artifactsDir = Directory("artifacts");

// versioning
var committedVersion = "0.0.0-dev";
var buildVersion = committedVersion;

// unit testing
var artifactsForUnitTestsDir = artifactsDir + Directory("UnitTests");
var unitTestAssemblies = @"./src/BinaryMash.Responses.Tests";
var compileConfig = Argument("configuration", "Debug");
var openCoverSettings = new OpenCoverSettings();
var minCodeCoverage = 95d;

// packaging
var packagesDir = artifactsDir + Directory("Packages");
var projectJson = "./src/BinaryMash.Responses/project.json";

// release notes
var releaseNotesDir = artifactsDir + Directory("Release");
var releaseNotesFile = releaseNotesDir + File("releasenotes.md");

Task("Default")
	.IsDependentOn("RunUnitTestsCoverageReport")
	.IsDependentOn("Package")
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
		var nugetVersion = GetVersion();
		Information("SemVer version number: " + nugetVersion);

		if (AppVeyor.IsRunningOnAppVeyor)
		{
			Information("Persisting version number...");
			PersistVersion(nugetVersion);
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
		Information("Build configuration is " + compileConfig);
		
		var buildSettings = new DotNetCoreBuildSettings
		{
			Configuration = compileConfig,
		};
		
		DotNetCoreRestore("./src");
	});

Task("RunUnitTests")
	.IsDependentOn("Restore")
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

Task("Package")
	.Does(() => 
	{
        GenerateReleaseNotes();

		var settings = new DotNetCorePackSettings
			{
				OutputDirectory = packagesDir,
				NoBuild = true
			};

		DotNetCorePack(projectJson, settings);

		if (AppVeyor.IsRunningOnAppVeyor)
		{
			var uploadSettings = new AppVeyorUploadArtifactsSettings()
				.SetArtifactType(AppVeyorUploadArtifactType.NuGetPackage);

			var path = packagesDir.ToString() + @"/**/*";

			foreach (var file in GetFiles(path))
			{
				AppVeyor.UploadArtifact(file.FullPath, uploadSettings);
			}
		}
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
	Information(string.Format("We'll search all project.json files for {0} and replace with {1}...", committedVersion, version));
	var projectJsonFiles = GetFiles("./**/project.json");

	foreach(var projectJsonFile in projectJsonFiles)
	{
		var file = projectJsonFile.ToString();
 
		Information(string.Format("Updating {0}...", file));

		var updatedProjectJson = System.IO.File.ReadAllText(file)
			.Replace(committedVersion, version);

		System.IO.File.WriteAllText(file, updatedProjectJson);
	}
}

private void GenerateReleaseNotes()
{
	Information("Generating release notes at " + releaseNotesFile);

	EnsureDirectoryExists(releaseNotesDir);

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