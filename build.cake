#addin "Cake.Xamarin"

var TARGET = Argument ("t", Argument ("target", Argument ("Target", "Default")));

Task ("libs").Does (() => 
{
	if (IsRunningOnWindows ()) {
		
		CreateDirectory ("./output/");
		CreateDirectory ("./output/wp8/");
		CreateDirectory ("./output/winrt/");
		
		NuGetRestore ("./Xamarin.Mobile.Windows.sln");

		MSBuild("./WindowsPhone8/Xamarin.Mobile/Xamarin.Mobile.csproj", c => c.SetConfiguration("Release").SetPlatformTarget(PlatformTarget.x86));
		CopyFiles ("./WindowsPhone8/Xamarin.Mobile/Bin/**/Release/*.dll", "./output/wp8");

		MSBuild ("./WindowsRT/Xamarin.Mobile/Xamarin.Mobile.csproj", c => c.SetConfiguration("Release").SetPlatformTarget(PlatformTarget.x86));
		CopyFiles ("./WindowsRT/Xamarin.Mobile/bin/**/Release/*.dll", "./output/winrt");
        
	} else {

		CreateDirectory ("./output/");
		CreateDirectory ("./output/android/");
		CreateDirectory ("./output/ios-unified/");
		CreateDirectory ("./output/ios/");

		NuGetRestore ("./Xamarin.Mobile.Mac.sln");

		DotNetBuild ("./MonoDroid/Xamarin.Mobile/Xamarin.Mobile.Android.csproj",  c => c.Configuration = "Release");
		CopyFiles ("./MonoDroid/Xamarin.Mobile/bin/Release/*.dll", "./output/android");

		iOSBuild ("./MonoTouch/Xamarin.Mobile/Xamarin.Mobile.iOS.csproj", new MDToolSettings { Configuration = "Release|iPhone" });
		CopyFiles ("./MonoTouch/Xamarin.Mobile/bin/unified/iPhone/Release/*.dll", "./output/ios-unified");

		iOSBuild ("./MonoTouch/Xamarin.Mobile/Xamarin.Mobile.iOS-Classic.csproj", new MDToolSettings { Configuration = "Release|iPhone" });
		CopyFiles ("./MonoTouch/Xamarin.Mobile/bin/classic/iPhone/Release/*.dll", "./output/ios");
	}	
});
Task ("Default").IsDependentOn ("libs").Does (() => { });

Task ("samples").IsDependentOn ("libs").Does (() => 
{
	if (IsRunningOnWindows ()) {
		MSBuild ("./WindowsPhone8/Samples/ContactsSample/ContactsSample.csproj", c => c.SetConfiguration("Release").SetPlatformTarget(PlatformTarget.x86));
		MSBuild ("./WindowsPhone8/Samples/GeolocationSample/GeolocationSample.csproj", c => c.SetConfiguration("Release").SetPlatformTarget(PlatformTarget.x86));
		MSBuild ("./WindowsPhone8/Samples/MediaPickerSample/MediaPickerSample.csproj", c => c.SetConfiguration("Release").SetPlatformTarget(PlatformTarget.x86));

		MSBuild ("./WindowsRT/Samples/GeolocationSample/GeolocationSample.csproj", c => c.SetConfiguration("Release").SetPlatformTarget(PlatformTarget.x86));
		MSBuild ("./WindowsRT/Samples/MediaPickerSample/MediaPickerSample.csproj", c => c.SetConfiguration("Release").SetPlatformTarget(PlatformTarget.x86));
	} else {		
		DotNetBuild ("./MonoDroid/Samples/ContactsSample/Contacts Sample.csproj", c => c.Configuration = "Release");
		DotNetBuild ("./MonoDroid/Samples/GeolocationSample/Geolocation Sample.csproj", c => c.Configuration = "Release");
		DotNetBuild ("./MonoDroid/Samples/MediaPickerSample/MediaPicker Sample.csproj", c => c.Configuration = "Release");

		DotNetBuild ("./MonoTouch/Samples/ContactsSample/Contacts Sample.csproj", c => c.Configuration = "Release"); //new MDToolSettings { Configuration = "Release|iPhone" });
		DotNetBuild ("./MonoTouch/Samples/Geolocation/Geolocation Sample.csproj", c => c.Configuration = "Release"); // new MDToolSettings { Configuration = "Release|iPhone" });
		DotNetBuild ("./MonoTouch/Samples/MediaPickerSample/MediaPicker Sample.csproj", c => c.Configuration = "Release"); //new MDToolSettings { Configuration = "Release|iPhone" });

		DotNetBuild ("./MonoTouch/Samples/ContactsSample-Classic/Contacts Sample-Classic.csproj", c => c.Configuration = "Release"); // new MDToolSettings { Configuration = "Release|iPhone" });
		DotNetBuild ("./MonoTouch/Samples/Geolocation-Classic/Geolocation Sample-Classic.csproj", c => c.Configuration = "Release"); //new MDToolSettings { Configuration = "Release|iPhone" });
		DotNetBuild ("./MonoTouch/Samples/MediaPickerSample-Classic/MediaPicker Sample-Classic.csproj", c => c.Configuration = "Release"); //new MDToolSettings { Configuration = "Release|iPhone" });
	}
});

Task ("nuget").IsDependentOn ("samples").Does (() =>
{
	if (!FileExists ("./tools/nuget.exe")) {
		DownloadFile ("http://nuget.org/nuget.exe", "./tools/nuget.exe");
	}

	StartProcess (MakeAbsolute (new FilePath ("./tools/nuget.exe")), new ProcessSettings {
		Arguments = "pack Xamarin.Mobile.nuspec"
	});

	DeleteFiles ("./output/*.nupkg");
	MoveFiles ("./*.nupkg", "./output/");
});

Task ("component").IsDependentOn ("nuget").Does (() =>
{
	if (!FileExists ("./tools/xamarin-component.exe")) {
		DownloadFile ("https://components.xamarin.com/submit/xpkg", "./tools/xpkg.zip");
		Unzip ("./tools/xpkg.zip", "./tools/");		
	}

	StartProcess (MakeAbsolute (new FilePath ("./tools/xamarin-component.exe")), new ProcessSettings {
		Arguments = "package ./"
	});

	DeleteFiles ("./output/*.xam");
	MoveFiles ("./*.xam", "./output/");
});

Task ("clean").Does (() => 
{
	if (DirectoryExists ("./output/"))
		DeleteDirectory ("./output/", true);

	if (DirectoryExists ("./tools/"))
		DeleteDirectory ("./tools/", true);

	CleanDirectories ("./**/bin");
	CleanDirectories ("./**/obj");
});

Task ("zip").Does (() =>
{
	if (IsRunningOnWindows ()) {
		if (FileExists ("./win-output.zip"))
			DeleteFile ("./win-output.zip");
	
		Zip ("./output/", "./win-output.zip");
	}
	else {
		if (FileExists ("./osx-output.zip"))
			DeleteFile ("./osx-output.zip");
	
		Zip ("./output/", "./osx-output.zip");
	}
});

RunTarget (TARGET);
