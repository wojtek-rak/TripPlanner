# TripPlanner

Blazor (.NET 9, C# 13) web application for planning trips, secured with ASP.NET Core Identity and backed by Entity Framework Core with SQL Server.

![CI](https://github.com/wojtek-rak/TripPlanner/actions/workflows/ci.yml/badge.svg?branch=master)

## Features
- Blazor components for trips:
  - List, detail and summary views (`Components/Trips/*`)
- Authentication and account management (Identity UI under `/Account/*`)
- EF Core with SQL Server (migrations in `Data/Migrations`)
- E2E tests with Microsoft Playwright (`TripPlanner.E2E`)
- GitHub Actions CI on Ubuntu (`.github/workflows/ci.yml`)

## Tech stack
- .NET 9, C# 13
- Blazor
- ASP.NET Core Identity
- EF Core (SQL Server)
- xUnit + Microsoft.Playwright for E2E

## Getting started

### Prerequisites
- .NET SDK 9.0.x
- SQL Server (LocalDB/Express or a reachable SQL instance)
- PowerShell 7+ (for Playwright’s install script on non-Windows use `pwsh`)

