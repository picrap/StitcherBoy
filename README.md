# StitcherBoy

This is a simple library allowing to write MSBuild tasks to modify assemblies after build. It allows to debug tasks because they can be build as applications.
It uses the excellent [dnlib](https://github.com/0xd4d/dnlib) to modify assemblys.

Available as a [NuGet package](https://www.nuget.org/packages/StitcherBoy).  
Current build status: [![Build status](https://ci.appveyor.com/api/projects/status/ta68llgihfomlct9?svg=true)](https://ci.appveyor.com/project/picrap/stitcherboy).

## In details

### Implementing

Create a project which generates a console application.
Then add two classes:
- The processor:
```csharp
public class MyStitcher : SingleStitcher
{
  protected override bool Process(ModuleDefMD moduleDef, string assemblyPath, ProjectDefinition project, string projectPath, string solutionPath)
  {
    // ... Play with the assembly
  }
}
```
- And the entry point:
```csharp
public class MyTask : StitcherTask<MyStitcher>
{
    public static int Main(string[] args) => Run(new MyTask(), args);
}
```

### Embedding it as MSBuild task

// TODO (lazy boy)

### Debugging it

Simply set the following command-line parameters `ProjectPath=<pathToCSProj>` `SolutionPath=<pathToSln>`.

## They use StitcherBoy

- [VersionSticher](https://github.com/picrap/VersionStitcher): a task to inject version information after build, based on values injected in strings.
