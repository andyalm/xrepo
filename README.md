XRepo - Cross repository development finally made easy
======================================================

XRepo makes it easier to work on shared libraries across repositories by letting you temporarily replace NuGet package references with local project references that your IDE (Rider, Visual Studio) natively understands.

Quickstart
----------
1) Install xrepo as a .NET global tool

    dotnet tool install -g xrepo

2) Bootstrap xrepo by installing MSBuild hooks that will watch for packages that you build locally register them with xrepo:

    xrepo bootstrap

3) Navigate to the repo where your shared library is built, build it, and register it:

    cd ~/src/MySharedLib
    dotnet build
    xrepo repo register MySharedLib

4) In the repo that consumes the shared library, ref it:

    cd ~/src/MyApp
    xrepo ref MySharedLib

This will find all NuGet packages from MySharedLib that your solution references and add local `ProjectReference` entries so your IDE can navigate into the source, set breakpoints, and get full IntelliSense.

5) When you're done working on the shared library, unref it:

    xrepo unref

Commands
--------

| Command | Description |
|---------|-------------|
| `bootstrap` | Install global MSBuild hooks for package registration |
| `repo register <name> [-p\|--path]` | Register a repo at the current or specified path |
| `repo unregister <name>` | Unregister a repo |
| `repos` | List all registered repos |
| `packages` | List all registered packages |
| `which <name>` | Show the most recently registered location for a package or assembly |
| `where <name>` | Show all registered locations for a package or assembly |
| `ref <name> [-s\|--solution]` | Add project references for a repo's packages into a solution |
| `unref [name] [-s\|--solution]` | Remove project references that were added by xrepo ref |

How it works
------------

XRepo has two phases:

**Discovery**: XRepo installs MSBuild build hooks that automatically register where your packages are built. When you run `dotnet build` or `dotnet pack` in a project, XRepo records the package ID, version, and project path.

**Referencing**: When you run `xrepo ref <RepoName>`, XRepo looks up the repo's path, finds all registered packages built from that repo, and checks which projects in your solution reference those packages. For each match, it adds a `ProjectReference` to the consuming project and adds the source project to your solution file under an "xrepo" folder. Running `xrepo unref` reverses the process.
