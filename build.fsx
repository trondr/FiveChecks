#r "paket:
nuget Fake.IO.FileSystem
nuget Fake.DotNet.AssemblyInfoFile
nuget Fake.DotNet.MSBuild
nuget Fake.DotNet.Testing.Nunit
nuget Fake.Testing.Common
nuget Fake.DotNet.NuGet
nuget Fake.IO.Zip
nuget NUnit.Console
nuget trondr.Fake.CustomTasks
nuget Fake.Tools.Git
nuget Fake.Core.Target //"
#load "./.fake/build.fsx/intellisense.fsx"

open Fake.IO
open Fake.IO.Globbing.Operators //enables !! and globbing
open Fake.DotNet
open Fake.Core
open Fake.Testing
open Fake.DotNet.Testing
open trondr.Fake.CustomTasks.SignTool

//Properties
let appName = "Compliance.Notifications"
let buildFolder = System.IO.Path.GetFullPath("./build/")
let buildAppFolder = buildFolder + "app"
let buildTestFolder = buildFolder + "test"
let buildSetupFolder = buildFolder + "setup"
let buildSetupFolderEnUs = buildSetupFolder + "/en-us"
let artifactFolder = System.IO.Path.GetFullPath("./artifact/")
let artifactAppFolder = artifactFolder + "app"
let codeSigningSha1Thumbprint = Fake.Core.Environment.environVarOrNone "CODE_SIGNING_SHA1_THUMBPRINT"

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
            AssemblyInfo.Copyright "Copyright \u00A9 github.trondr 2020"
            AssemblyInfo.Company "FiveChecks"
            AssemblyInfo.Version assemblyVersion
            AssemblyInfo.FileVersion assemblyVersion                        
            AssemblyInfo.ComVisible false
            AssemblyInfo.Guid "8B2765D7-A756-45AA-B7BD-4FB98F9F51F4"
            AssemblyInfo.InternalsVisibleTo (appName + ".Tests")
            AssemblyInfo.StringAttribute("NeutralResourcesLanguage", "en-US", "System.Resources")
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
            AssemblyInfo.Copyright "Copyright \u00A9 github.trondr 2020"
            AssemblyInfo.Company "FiveChecks"
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

//
// How to create a self signed certificated for development purposes.
//
// Example Powershell script (change the dns domain name):
//
(*
    $myDnsName = "Development.and.Test.$($env:UserName)"
    $PasswordString = ([char[]]([char]33..[char]95) + ([char[]]([char]97..[char]126)) + 0..9 | sort {Get-Random})[0..20] -join ''
    $password = ConvertTo-SecureString -String $PasswordString -Force -AsPlainText
    $cert = New-SelfSignedCertificate -certstorelocation cert:\currentuser\my -dnsname $myDnsName -Type CodeSigningCert
    $certExport = Export-PfxCertificate -Cert $cert -FilePath "c:\temp\$myDnsName.pfx" -Password $password
    $cert | Format-List
    Write-Host "Password: $PasswordString"
    Write-Host "Thumbprint: $($cert.Thumbprint)"
    Write-Host "Pfxfile: $($certExport.FullName)"
    Write-Host "Save the password, thumbprint and pfx file for later use." -ForegroundColor Green
*)

//
//  How to setup signing. Requires that a certificate has been phurcased. For development purposes use a self-signed certificate (see procedure above).
//
// https://stackoverflow.com/questions/17710357/how-do-i-securely-store-a-pfx-password-to-use-in-msbuild
// 1. Log in as the build user
// 2. Run certmgr.msc
// 3. Right-click Certificates - Current User / Personal / Certificates, and select All Tasks / Import...
// 4. Select your .pfx file, enter the password, and click Next and Finish
// 5. Double-click on the imported certificate
// 6. In the Details page, the thumbprint algorithm should be sha1
// 7. Copy the thumbprint, it looks something like 12 34 56 78 90 ab cd ef 12 34 56 78 90 ab cd ef 12 34 56 78
// 8. signtool /sha1 1234567890abcdef1234567890abcdef12345678 /t http://timestamp.verisign.com/scripts/timestamp.dll /d "My Signature description" "<path to file to be signed>"


Target.create "Sign" (fun _ ->
    Trace.trace "Signing assemblies and msi..."
    //2020-03-08: Fake.Tools.SignTool is still in alpha version and causes some unresolved dependencies when trying to reference it
    //2020-03-08: So here we need to call SignTool.exe manually
    match codeSigningSha1Thumbprint with
    |None ->
        Trace.traceError "Certificate Sha1 thumbprint environent variable (CODE_SIGNING_SHA1_THUMBPRINT) has not been specified." 
        Trace.traceError "Artifacts will not be signed." 
    |Some sha1Thumbprint ->
        let timeStampServers = ["http://timestamp.verisign.com/scripts/timestamp.dll"]
        let signingResult = 
            let filesToBeSigned = 
                !! ("build/**/*.exe")
                ++ ("build/**/*.dll")
                ++ ("build/**/*.msi")
                ++ ("build/**/*.ps1")
                |>Seq.toArray
            let description = Some (appName + " " + Fake.Tools.Git.Information.getCurrentHash())
            filesToBeSigned
            |> trondr.Fake.CustomTasks.SignTool.runSignTool sha1Thumbprint description timeStampServers
        match signingResult with
        |SignResult.Success -> Trace.trace "Successfull finished signing."
        |SignResult.Failed msg -> Trace.traceError (sprintf "Signing failed. %s" msg)
        |SignResult.TimeServerError msg -> Trace.traceError (sprintf "Signing failed due to issue with time stamp server. %s" msg)
        |SignResult.Uknown -> Trace.traceError "Signing failed due unknown reason."
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
    Trace.trace (appName + "." + assemblyVersion)
)

//Dependencies
open Fake.Core.TargetOperators

"Clean" 
    ==> "RestorePackages"
    ==> "BuildApp"
    ==> "BuildTest"
    ==> "BuildSetup"
    ==> "Test"
    ==> "Sign"
    ==> "Publish"
    ==> "Default"

//Start build
Target.runOrDefault "Default"