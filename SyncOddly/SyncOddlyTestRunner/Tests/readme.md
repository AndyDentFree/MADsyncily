# TestSyncOddly shared tests
In prior versions of the Xamarin/Visual Studio ecosystem, this would have been a [Shared Project][SP] however as of Visual Studio Mac 17.x (VSMac 2022) these are [no longer available][SPD].

So, we just have this shared folder which is included directly into a testrunner project. The code to do this was manually inserted
```
<ItemGroup>
  <Compile Include="..\TestSyncOddlyShared\**\*.cs" Link="TestSyncOddlyShared\%(RecursiveDir)%(Filename)%(Extension)" />
</ItemGroup>
```

[SP]: https://learn.microsoft.com/en-us/xamarin/cross-platform/app-fundamentals/shared-projects?tabs=macos
[SPD]: https://developercommunity.visualstudio.com/t/Shared-Project-template-missing-in-VS-20/10104303