# Accelergreat Examples

This directory contains practical examples showing how to use Accelergreat for different testing scenarios. Each example demonstrates a unique setup and use case.

## Examples Overview

### 01 - Basic SQLite Entity Framework
Simple example showing how to set up Accelergreat with SQLite and Entity Framework for basic CRUD operations.

### 02 - SQL Server Entity Framework  
Demonstrates SQL Server integration with different reset strategies (transactions vs snapshot rollback) and migrations.

### 03 - ASP.NET Core API Testing
Shows how to test ASP.NET Core APIs with database integration using Accelergreat's web app components.

### 04 - Minimal API Testing
Example of testing minimal APIs with Accelergreat, showing a lightweight approach.

### 05 - Microservices Testing
Complex example demonstrating how to test multiple microservices communicating with each other.

### 06 - Combined Example
Advanced example combining multiple components: databases, APIs, and microservices in a single test suite.

## Getting Started

1. Each example is self-contained with its own README
2. Make sure you have .NET 6+ installed
3. For SQL Server examples, you'll need a SQL Server instance running
4. For SQLite examples, no additional setup is required

## Common Patterns

All examples follow these patterns:
- **Startup.cs**: Configure Accelergreat components
- **accelergreat.json**: Configuration file with database settings
- **Tests**: Test classes inheriting from `AccelergreatXunitTest`
- **Components**: Custom components extending Accelergreat base classes

## Prerequisites

- .NET 6, 7, 8, or 9
- SQL Server (for SQL Server examples)
- Visual Studio or VS Code recommended

## Running Examples

Each example can be run independently:

```bash
cd 01-basic-sqlite-entity-framework
dotnet test
```

## Support

For questions about these examples or Accelergreat:
- Documentation: https://accelergreat.net
- Issues: Create an issue in the main Accelergreat repository 