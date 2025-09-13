using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// [MANDATORY] The following GUID is used as a unique identifier of the plugin. Generate a fresh one for your plugin!
[assembly: Guid("a78409de-879f-4005-aa49-4fcd061c40a8")]

// [MANDATORY] The assembly versioning
//Should be incremented for each new release build of a plugin
[assembly: AssemblyVersion("1.0.0.1")]
[assembly: AssemblyFileVersion("1.0.0.1")]

// [MANDATORY] The name of your plugin
[assembly: AssemblyTitle("SkyFlats")]
// [MANDATORY] A short description of your plugin
[assembly: AssemblyDescription("SQM based sky flat automation")]

// The following attributes are not required for the plugin per se, but are required by the official manifest meta data

// Your name
[assembly: AssemblyCompany("photon")]
// The product name that this plugin is part of
[assembly: AssemblyProduct("SkyFlats")]
[assembly: AssemblyCopyright("Copyright © 2025 photon")]

// The minimum Version of N.I.N.A. that this plugin is compatible with
[assembly: AssemblyMetadata("MinimumApplicationVersion", "3.0.0.2017")]

// The license your plugin code is using
[assembly: AssemblyMetadata("License", "MPL-2.0")]
// The url to the license
[assembly: AssemblyMetadata("LicenseURL", "https://www.mozilla.org/en-US/MPL/2.0/")]
// The repository where your pluggin is hosted
[assembly: AssemblyMetadata("Repository", "https://github.com/photon1503/SkyFlats")]

// The following attributes are optional for the official manifest meta data

//[Optional] Your plugin homepage URL - omit if not applicaple
[assembly: AssemblyMetadata("Homepage", "tbd")]

//[Optional] Common tags that quickly describe your plugin
[assembly: AssemblyMetadata("Tags", "skyflats")]

//[Optional] A link that will show a log of all changes in between your plugin's versions
[assembly: AssemblyMetadata("ChangelogURL", "https://github.com/photon1503/SkyFlats/blob/main/CHANGELOG.md")]

//[Optional] The url to a featured logo that will be displayed in the plugin list next to the name

[assembly: AssemblyMetadata("FeaturedImageURL", "https://github.com/photon1503/SkyFlats/blob/efb79a7cb68cf32549b86a81cd31e2b07cfcff83/skyflats.png?raw=true")]
//[Optional] A url to an example screenshot of your plugin in action
[assembly: AssemblyMetadata("ScreenshotURL", "")]
//[Optional] An additional url to an example example screenshot of your plugin in action
[assembly: AssemblyMetadata("AltScreenshotURL", "")]
//[Optional] An in-depth description of your plugin
[assembly: AssemblyMetadata("LongDescription", @"This plugin extends the core NINA sky flat sequence functionality with enhanced automation capabilities for precision flat-field calibration. The module introduces SQM-based filter sequencing that automatically triggers flat-field acquisition for each filter at predefined sky brightness levels as measured by a Sky Quality Meter, ensuring optimal exposure conditions for each optical filter throughout the changing twilight sky.

Additionally, the implementation incorporates dynamic null point tracking, which periodically re-slews the telescope to the astronomical null point during capture sequences. This maintains optimal positioning for sky gradient minimization throughout the flat acquisition process, following the methodology established in astronomical literature for obtaining uniform sky background illumination. The combination of these features provides a robust automated solution for obtaining consistent, high-quality flat fields through precision calibration without manual intervention.")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]
// [Unused]
[assembly: AssemblyConfiguration("")]
// [Unused]
[assembly: AssemblyTrademark("")]
// [Unused]
[assembly: AssemblyCulture("")]