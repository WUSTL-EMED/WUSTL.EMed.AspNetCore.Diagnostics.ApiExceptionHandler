// <copyright file="AssemblyInfo.cs" company="Washington University in St. Louis">
// Copyright (c) 2021 Washington University in St. Louis. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// </copyright>

using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("WUSTL.EMed.AspNetCore.Diagnostics.ApiExceptionHandler")]
[assembly: AssemblyDescription("A middleware for setting the response body when an exception occurs on an api controller method based on an api path prefix.")]
[assembly: AssemblyCompany("Emergency Medicine Information Technology Services, Department of Emergency Medicine, Washington University School of Medicine in St. Louis")]
[assembly: AssemblyProduct("WUSTL.EMed.AspNetCore.Diagnostics.ApiExceptionHandler")]
[assembly: AssemblyCopyright("Copyright © 2021 Washington University in St. Louis")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Set assembly configuration based on DEBUG constant.
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]
[assembly: CLSCompliant(false)]
[assembly: NeutralResourcesLanguage("en-US")]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("a4797969-01db-1a98-4b6c-a4ec27422c9b")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.1.0.0")]
[assembly: AssemblyFileVersion("1.1.0.0")]
[assembly: AssemblyInformationalVersion(ThisAssembly.Git.Sha)]
