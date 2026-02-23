XRepo - Cross repository development made easier
======================================================

XRepo makes it easier to work on shared libraries across repositories by letting you temporarily replace NuGet package references with local project references that your IDE (Rider, Visual Studio) natively understands.

Quickstart
----------
1) Install xrepo as a .NET global tool
```shell
dotnet tool install -g xrepo
```
2) Bootstrap xrepo by installing MSBuild hooks that will watch for packages that you build locally register them with xrepo:
```shell
xrepo bootstrap
```
3) Navigate to the repo where your shared library is built, build it, and register it:
```shell
cd ~/src/MySharedLib
dotnet build
xrepo repo register MySharedLib
```
4) In the repo that consumes the shared library, ref it:
```shell
cd ~/src/MyApp
xrepo ref MySharedLib
```
This will find all NuGet packages from MySharedLib that your solution references and add local `ProjectReference` entries so your IDE can navigate into the source, set breakpoints, and get full IntelliSense.
5) When you're done working on the shared library, unref it:
```shell
xrepo unref
```

Commands
--------

| Command                             | Description                                                          |
|-------------------------------------|----------------------------------------------------------------------|
| `bootstrap`                         | Install global MSBuild hooks for package registration                |
| `repo register <name> [-p\|--path]` | Register a repo at the current or specified path                     |
| `repo unregister <name>`            | Unregister a repo                                                    |
| `repos`                             | List all registered repos                                            |
| `packages`                          | List all registered packages                                         |
| `which <name>`                      | Show the most recently registered location for a package or assembly |
| `where <name>`                      | Show all registered locations for a package or assembly              |
| `ref <name> [-s\|--solution]`       | Add project references for a repo's packages into a solution         |
| `unref [name] [-s\|--solution]`     | Remove project references that were added by xrepo ref               |

Tab Completion
--------------

XRepo supports shell tab completion via [dotnet-suggest](https://github.com/dotnet/command-line-api/blob/main/docs/dotnet-suggest.md). This gives you completions for commands, options, and dynamic values like registered repo names and package IDs.

1) Install `dotnet-suggest` as a global tool:
```shell
dotnet tool install -g dotnet-suggest
```
2) Add the shell shim to your profile:

**bash** (`~/.bashrc`):
```shell
dotnet-suggest script bash >> ~/.bashrc
```

**zsh** (`~/.zshrc`):

The default script from `dotnet-suggest script zsh` has bugs that break file path
completion and can produce errors on empty results. Use the fixed version included
in this repo instead:
```shell
curl -fsSL https://raw.githubusercontent.com/andyalm/xrepo/main/.tab_completions/zsh.sh >> ~/.zshrc
```
Or, if you have a local clone of this repo:
```shell
cat /path/to/xrepo/.tab_completions/zsh.sh >> ~/.zshrc
```

**PowerShell** (`$PROFILE`):
```powershell
dotnet-suggest script powershell >> $PROFILE
```

3) Restart your shell, and tab completion will work for xrepo commands:
```shell
xrepo <TAB>              # lists all commands
xrepo ref <TAB>          # lists registered repos, packages, or paths to csproj files
xrepo which <TAB>        # lists registered projects for a package
xrepo repo unregister <TAB>  # lists registered repos
```

How it works
------------

XRepo has two phases:

**Discovery**: XRepo installs MSBuild build hooks that automatically register where your packages are built. When you run `dotnet build` or `dotnet pack` in a project, XRepo records the package ID, version, and project path.

**Referencing**: When you run `xrepo ref <RepoName>`, XRepo looks up the repo's path, finds all registered packages built from that repo, and checks which projects in your solution reference those packages. For each match, it adds a `ProjectReference` to the consuming project and adds the source project to your solution file under an "xrepo" folder. Running `xrepo unref` reverses the process.
