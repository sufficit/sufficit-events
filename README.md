<h1>
  Sufficit.Events
  <a href="https://github.com/sufficit/sufficit"><img src="https://avatars.githubusercontent.com/u/66928451?s=200&v=4" alt="Sufficit Logo" width="80" align="right"></a>
</h1>

[![NuGet](https://img.shields.io/nuget/v/Sufficit.Events.svg)](https://www.nuget.org/packages/Sufficit.Events/)

## 📖 About the Project

`Sufficit.Events` is a lightweight, multi-targeted event bus library designed to provide a simple publish/subscribe in-process event system for Sufficit projects. It supports background processing using System.Threading.Channels, DI-based handler resolution and basic observability via metrics snapshots and System.Diagnostics.Metrics counters.

This library aims to be consumed both by modern .NET applications and legacy codebases that require `netstandard2.0` compatibility.

### ✨ Key Features

* In-process publish/subscribe event bus with background processing (Channels).
* Handler discovery and registration via dependency injection (IServiceCollection extension).
* Thread-safe internal metrics with snapshot DTO (`EventBusStatistics`) and Meter/Counters for integration with observability systems.
* Multi-target support: `netstandard2.0`, `net6.0`, `net7.0`, `net9.0`.

## 🚀 Getting Started

This is a library intended to be consumed by other projects. Install it as a NuGet package when available or reference the project locally.

### 📦 NuGet Package

Install via .NET CLI:

    dotnet add package Sufficit.Events

### Local Project Reference

If you have the source in your solution, prefer using a ProjectReference during development:

    <ProjectReference Include="..\sufficit-events\src\Sufficit.Events.csproj" />

## 🛠️ Usage

Register the event system and your handlers in `IServiceCollection`:

    services.AddEventSystem(typeof(Startup).Assembly);

Create handlers by implementing `IEventHandler<TEvent>`:

    public class MyEventHandler : IEventHandler<MyEvent>
    {
        public Task HandleAsync(MyEvent evt, CancellationToken cancellationToken)
        {
            // handle
            return Task.CompletedTask;
        }
    }

Publish events using `IEventBus`:

    await eventBus.PublishAsync(new MyEvent { ... });

Get a snapshot of metrics:

    var stats = eventBus.GetStatistics();

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.

## 🤝 Contributing

Contributions welcome — please follow the fork & pull request workflow and read the `Documentation` folder for internal guidelines.

## 📧 Support

Sufficit - [development@sufficit.com.br](mailto:development@sufficit.com.br)

## Related

See `sufficit-base` for shared DTOs and `sufficit-telephony-eventspanel-core` for example consumers.
<h1>
  Sufficit.Base
  <a href="https://github.com/sufficit"><img src="https://avatars.githubusercontent.com/u/66928451?s=200&v=4" alt="Sufficit Logo" width="80" align="right"></a>
</h1>

[![NuGet](https://img.shields.io/nuget/v/Sufficit.Base.svg)](https://www.nuget.org/packages/Sufficit.Base/)

## 📖 About the Project

`Sufficit.Base` is a foundational library for the entire Sufficit software ecosystem. It provides the most essential and widely-used components, including base classes, core interfaces, utility methods, and data transfer objects (DTOs) that serve as the building blocks for other Sufficit projects.

The goal of this library is to promote code reuse, consistency, and a common development standard across all our applications and services.

### ✨ Key Features

* Core interfaces and abstract base classes for common patterns (e.g., Services, Repositories).
* Commonly used data models and DTOs.
* A rich set of helper classes and extension methods.
* Shared constants, enumerations, and attributes used throughout the ecosystem.

## 🚀 Getting Started

This project is a class library. The recommended way to use it is by installing the NuGet package into your project.

### 📦 NuGet Package

You can install the package via the .NET CLI or the NuGet Package Manager Console.

**.NET CLI:**

    dotnet add package Sufficit.Base

**Package Manager Console:**

    Install-Package Sufficit.Base

## 🛠️ Usage

As a foundational library, `Sufficit.Base` is designed to be a dependency for nearly all other projects within the Sufficit ecosystem.

**Example of a base model usage:**

    using Sufficit.Base;
    
    public class User : IID
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
    }

**Example of an extension method:**

    using Sufficit.Base.Extensions;

    var text = " some text with extra spaces ";
    var cleanedText = text.TrimAndLower();
    // Result: "some text with extra spaces"
    
## 🤝 Contributing

Contributions are what make the open-source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

1.  Fork the Project.
2.  Create your Feature Branch (`git checkout -b feature/AmazingFeature`).
3.  Commit your Changes (`git commit -m 'Add some AmazingFeature'`).
4.  Push to the Branch (`git push origin feature/AmazingFeature`).
5.  Open a Pull Request.

## 📄 License

Distributed under the MIT License. See `LICENSE` for more information.

## 📧 Contact

Sufficit - [contato@sufficit.com.br](mailto:contato@sufficit.com.br)

Project Link: [https://github.com/sufficit/sufficit-base](https://github.com/sufficit/sufficit-base)