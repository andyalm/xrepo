XRepo - Cross repository development finally made easy
======================================================

Quickstart
----------
1) Install xrepo via chocolatey

    chocolatey install xrepo

2) Navigate to the repo where your shared library is built, build it, and register it. For example:

    cd c:\src\frameworks\AspNetMvc
    build.cmd
    xrepo repo register AspNetMvc

3) Pin the repo
    
    xrepo pin AspNetMvc

4) Go build a project that references your shared library via an assembly reference. When you build it, all references to assemblies in your pinned repo will be automatically resolved to locally built copies of the assembly. When you are ready for assembly resolution to go back to normal, simply unpin the repo. For example:

    xrepo unpin AspNetMvc

Configuration options
---------------------
Configuration options can be set via the 'xrepo config' command.

### copy_pins

Determines whether pinned assemblies will be copied to the HintPath location of the assembly reference they are overriding. If you use an real-time code analysis tool like ReSharper, you will want to enable this option.

### pin_warnings

Determines whether a build warning will be generated when a project is built referencing a pinned assembly. This can be a useful reminder when you have something pinned.
