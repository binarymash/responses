#tool "nuget:?package=OpenCover"
#tool "nuget:?package=ReportGenerator"
#tool "nuget:?package=GitVersion.CommandLine"

var compileConfig = Argument("configuration", "Debug");
var target = Argument("target", "Default");
var artifactsDir = Directory("Artifacts");

// version
var semVer = "0.0.0";

// unit testing
var artifactsForUnitTestsDir = artifactsDir + Directory("UnitTests");
var unitTestAssemblies = @".\src\BinaryMash.Responses.Tests";
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
		EnsureDirectoryExists(artifactsDir);
		CleanDirectories(artifactsDir);
	});
	
Task("GetVersion")
	.Does(() =>
	{
		if (AppVeyor.IsRunningOnAppVeyor)
		{
			Information("Skipping GitVersion");
		}
		else
		{
			var gitVersion = GitVersion();
			semVer = gitVersion.SemVer;
			Information("SemVer according to GitVersion: " + semVer);
		}
	});

Task("Compile")
	.IsDependentOn("Clean")
//	.IsDependentOn("GetVersion")
	.Does(() =>
	{
		Information("Build configuration is " + compileConfig);
		
		var buildSettings = new DotNetCoreBuildSettings
		{
			Configuration = compileConfig,
			VersionSuffix = semVer
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