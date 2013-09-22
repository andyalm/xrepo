Feature: Assembly Registration
	In order to find locally built assembly dependencies
	As a developer
	I want to record where an assembly is built

Scenario: A compiled assembly is registered
	Given a class library project
	When the project is compiled
	Then the resulting assembly is registered by xrepo

Scenario: A pinned assembly overrides hint paths by default
	Given a class library project
		And the project has a reference to assembly nunit.framework
		And the assembly nunit.framework is registered
		And the assembly nunit.framework is pinned
		And the copypins config setting is false
	When the project is compiled
	Then the reference to is resolved to the pinned copy of nunit.framework

Scenario: A pinned assembly is copied to hint path location when copypins is true
	Given a class library project
		And the project has a reference to assembly nunit.framework
		And the assembly nunit.framework is registered
		And the assembly nunit.framework is pinned
		And the copypins config setting is true
	When the project is compiled
	Then the registered copy of nunit.framework is copied to the hint path's location
		And a backup copy of the original nunit.framework is kept

Scenario: An unpinned assembly does not override the hint path
	Given a class library project
		And the project has a reference to assembly nunit.framework
		And the assembly nunit.framework is registered
		And the assembly nunit.framework is not pinned
	When the project is compiled
	Then the reference to nunit.framework is resolved via standard msbuild rules

Scenario: A pinned repo overrides hint paths for all registered assemblies within the repo
	Given a repo MyRepo
		And a class library project
		And the project has a reference to assembly nunit.framework
		And the assembly nunit.framework is registered at a location within MyRepo
		And the repo MyRepo is pinned
		And the copy_pins config setting is false
	When the project is compiled
	Then the reference to is resolved to the pinned copy of nunit.framework
