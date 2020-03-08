#r "paket:
nuget Fake.IO.FileSystem
nuget Fake.DotNet.AssemblyInfoFile
nuget Fake.DotNet.MSBuild
nuget Fake.DotNet.Testing.Nunit
nuget Fake.Testing.Common
nuget Fake.DotNet.NuGet
nuget Fake.IO.Zip
nuget NUnit.Console
nuget Fake.Core.Target //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.IO
open Fake.IO.Globbing.Operators //enables !! and globbing
open Fake.DotNet
open Fake.Core
open Fake.Testing
open Fake.DotNet.Testing


//Properties
let appName = "Compliance.Notifications"
let buildFolder = System.IO.Path.GetFullPath("./build/")
let buildAppFolder = buildFolder + "app"
let buildTestFolder = buildFolder + "test"
let buildSetupFolder = buildFolder + "setup"
let buildSetupFolderEnUs = buildSetupFolder + "/en-us"
let artifactFolder = System.IO.Path.GetFullPath("./artifact/")
let artifactAppFolder = artifactFolder + "app"

let assemblyVersion =
    let majorVersion = "1"
    let minorVersion = "0"
    let now = System.DateTime.Now    
    let buildVersion = sprintf "%02d%03d" (now.Year - 2000) (now.DayOfYear) //Example: 19063
    let revisionVersion = "46"
    sprintf "%s.%s.%s.%s" majorVersion minorVersion buildVersion revisionVersion //Example: 1.0.19063.1

let getVersion file = 
    System.Reflection.AssemblyName.GetAssemblyName(file).Version.ToString()

//Targets
Target.create "Clean" (fun _ ->
    Trace.trace "Clean build folder..."
    Shell.cleanDirs [ buildFolder; artifactFolder ]
)

Target.create "RestorePackages" (fun _ ->
     ("./" + appName + ".sln")
     |> Fake.DotNet.NuGet.Restore.RestoreMSSolutionPackages (fun p ->
         { p with             
             Retries = 4 })
   )

Target.create "BuildApp" (fun _ -> 
    Trace.trace "Building app..."
    AssemblyInfoFile.createCSharp ("./src/app/" + appName + "/Properties/AssemblyInfo.cs")
        [
            AssemblyInfo.Title appName
            AssemblyInfo.Description "Shows compliance notifications to the user." 
            AssemblyInfo.Product appName
            AssemblyInfo.Company "github/trondr"
            AssemblyInfo.Copyright "Copyright \u00A9 github/trondr 2020"
            AssemblyInfo.Version assemblyVersion
            AssemblyInfo.FileVersion assemblyVersion                        
            AssemblyInfo.ComVisible false
            AssemblyInfo.Guid "8B2765D7-A756-45AA-B7BD-4FB98F9F51F4"
            AssemblyInfo.InternalsVisibleTo (appName + ".Tests")
        ]

    !! "src/app/**/*.csproj"
        |> MSBuild.runRelease id buildAppFolder "Build"
        |> Trace.logItems "BuildApp-Output: "
)

Target.create "BuildTest" (fun _ -> 
    Trace.trace "Building test..."
    AssemblyInfoFile.createCSharp ("./src/test/" + appName + ".Tests/Properties/AssemblyInfo.cs")
        [
            AssemblyInfo.Title (appName + ".Tests")
            AssemblyInfo.Description ("Tests of " + appName) 
            AssemblyInfo.Product appName
            AssemblyInfo.Company "github/trondr"
            AssemblyInfo.Copyright "Copyright \u00A9 github/trondr 2020"
            AssemblyInfo.Version assemblyVersion
            AssemblyInfo.FileVersion assemblyVersion                        
            AssemblyInfo.ComVisible false
            AssemblyInfo.Guid "1348CCF2-BD3E-4D55-A337-22C8E01AC47B"            
        ]

    !! "src/test/**/*.csproj"
        |> MSBuild.runRelease id buildTestFolder "Build"
        |> Trace.logItems "BuildTest-Output: "
)

Target.create "BuildSetup" (fun _ -> 
    Trace.trace "Build setup..."
    !! "src/setup/**/*.wixproj"
    |> MSBuild.runRelease id buildSetupFolder "Build"
    |> Trace.logItems "BuildSetup-Output: "
)

let nugetGlobalPackagesFolder =
    System.Environment.ExpandEnvironmentVariables("%userprofile%\.nuget\packages")

let nunitConsoleRunner =
    Trace.trace "Locating nunit-console.exe..."
    let consoleRunner = 
        !! (nugetGlobalPackagesFolder + "/**/nunit3-console.exe")
        |> Seq.head
    printfn "Console runner:  %s" consoleRunner
    consoleRunner

Target.create "Test" (fun _ -> 
    Trace.trace "Testing app..."    
    !! ("build/test/**/*.Tests.dll")    
    |> NUnit3.run (fun p ->
        {p with ToolPath = nunitConsoleRunner;Where = "cat==UnitTests";TraceLevel=NUnit3.NUnit3TraceLevel.Verbose})
)

Target.create "Publish" (fun _ ->
    Trace.trace "Publishing app..."
    let assemblyVersion = getVersion (System.IO.Path.Combine(buildAppFolder,appName + ".exe"))
    let files = 
        [|
            System.IO.Path.Combine(buildAppFolder, appName + ".exe")
            System.IO.Path.Combine(buildAppFolder, appName + ".pdb")
            System.IO.Path.Combine(buildAppFolder, appName + ".exe.config")            
        |]
    let zipFile = System.IO.Path.Combine(artifactFolder,sprintf "%s.%s.zip" appName assemblyVersion)
    files
    |> Fake.IO.Zip.createZip buildAppFolder zipFile (sprintf "%s %s" appName assemblyVersion) 9 false


    let files = 
        [|
            System.IO.Path.Combine(buildSetupFolder,"en-us", appName + ".msi")            
        |]
    let zipFile = System.IO.Path.Combine(artifactFolder,sprintf "%s.%s.msi.zip" appName assemblyVersion)
    files
    |> Fake.IO.Zip.createZip buildSetupFolderEnUs zipFile (sprintf "%s %s MSI" appName assemblyVersion) 9 false
)

Target.create "Default" (fun _ ->
    Trace.trace "Hello world from FAKE"
)

//Dependencies
open Fake.Core.TargetOperators

"Clean" 
    ==> "RestorePackages"
    ==> "BuildApp"
    ==> "BuildTest"
    ==> "BuildSetup"
    ==> "Test"
    ==> "Publish"
    ==> "Default"

//Start build
Target.runOrDefault "Default"