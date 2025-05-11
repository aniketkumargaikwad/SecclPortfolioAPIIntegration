# P1 Technical Test - SECCL API Integration (P1SecclApp)

This project is a solution for the P1 technical test, demonstrating an integration with the SECCL API to fetch, manipulate, and display client portfolio data. It follows a decoupled architecture with a .NET Core backend, a core logic library, and a Blazor WASM frontend.

## Tech Stack

* **Backend API :** ASP.NET Core 8 Minimal API (C#)
* **Core Logic/Middleware:** .NET 8 Class Library (C#) - Handles SECCL API communication and data aggregation.
* **Frontend UI:** Blazor WebAssembly (.NET 8, C#)
* **Development Environment:** Visual Studio 2022 Community Edition
* **Version Control:** Git & GitHub

## Prerequisites

* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [Visual Studio 2022 Community Edition](https://visualstudio.microsoft.com/vs/community/) (with "ASP.NET and web development" and ".NET desktop development" workloads installed)
* [Git](https://git-scm.com/downloads)

## SECCL API Credentials

The application requires credentials to access the SECCL API. These are:
* **FirmId:** `P1IMX`
* **Id (Username):** `nelahi6642@4tmail.net`
* **Password:** `DemoBDM1`

These credentials should be configured in the `P1SecclApp.BackendApi/appsettings.Development.json` file (or `appsettings.json` if `Development` doesn't exist) under the `SecclApi` section:

```json
{
  "SecclApi": {
    "FirmId": "P1IMX",
    "Username": "nelahi6642@4tmail.net",
    "Password": "DemoBDM1"
  }
}