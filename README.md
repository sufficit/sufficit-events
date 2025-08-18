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
