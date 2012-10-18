﻿Feature: Assembly Registration
	In order to find locally built assembly dependencies
	As a developer
	I want to record where an assembly is built

Scenario: A compiled assembly is registered
	Given a class library project
	When the project is compiled
	Then the resulting assembly is registered by xpack

Scenario: A pinned assembly overrides hint paths
	Given a class library project
		And the project has a reference to assembly nunit.framework
		And the assembly nunit.framework is registered
		And the assembly nunit.framework is pinned
	When the project is compiled
	Then the reference to is resolved to the pinned copy of nunit.framework

Scenario: An unpinned assembly does not override the hint path
	Given a class library project
		And the project has a reference to assembly nunit.framework
		And the assembly nunit.framework is registered
		And the assembly nunit.framework is not pinned
	When the project is compiled
	Then the reference to nunit.framework is resolved via standard msbuild rules