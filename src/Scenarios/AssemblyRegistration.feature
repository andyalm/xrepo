Feature: Assembly Registration
	In order to find locally built assembly dependencies
	As a developer
	I want to record where an assembly is built

Scenario: A compiled assembly is registered
	Given a class library project
	When the project is compiled
	Then the resulting assembly is registered by xpack

@wip
Scenario: A pinned assembly overrides hint paths
	Given a class library project
		And the project has a reference to assembly MyAssembly1
		And the assembly MyAssembly1 is registered
		And the assembly MyAssembly1 is pinned
	When the project is compiled
	Then the reference to is resolved to the pinned copy of MyAssembly1