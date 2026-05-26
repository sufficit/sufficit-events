using Microsoft.Extensions.DependencyInjection;
using Sufficit.Events;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sufficit.Events.Tests;

// ---------------------------------------------------------------------------
// Minimal event hierarchy used only for testing polymorphic dispatch
// ---------------------------------------------------------------------------

internal class BaseTestEvent { }

internal class DerivedTestEvent : BaseTestEvent { }

internal class AnotherDerivedTestEvent : BaseTestEvent { }

// ---------------------------------------------------------------------------
// Capturing handler stubs
// ---------------------------------------------------------------------------

/// <summary>
/// Counts how many times it receives a <see cref="BaseTestEvent"/> (or any subtype via polymorphic dispatch).
/// </summary>
internal sealed class BaseTestEventHandler : IEventHandler<BaseTestEvent>
{
    public int InvocationCount { get; private set; }
    public BaseTestEvent? LastEvent { get; private set; }

    public Task HandleAsync(BaseTestEvent eventData, CancellationToken cancellationToken = default)
    {
        InvocationCount++;
        LastEvent = eventData;
        return Task.CompletedTask;
    }
}

/// <summary>
/// Counts how many times it receives a <see cref="DerivedTestEvent"/> specifically.
/// </summary>
internal sealed class DerivedTestEventHandler : IEventHandler<DerivedTestEvent>
{
    public int InvocationCount { get; private set; }
    public DerivedTestEvent? LastEvent { get; private set; }

    public Task HandleAsync(DerivedTestEvent eventData, CancellationToken cancellationToken = default)
    {
        InvocationCount++;
        LastEvent = eventData;
        return Task.CompletedTask;
    }
}

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

public sealed class EventBusPolymorphicDispatchTests : IAsyncLifetime
{
    private ServiceProvider? _provider;
    private IEventBus? _bus;
    private BaseTestEventHandler? _baseHandler;
    private DerivedTestEventHandler? _derivedHandler;

    public Task InitializeAsync()
    {
        _baseHandler   = new BaseTestEventHandler();
        _derivedHandler = new DerivedTestEventHandler();

        var services = new ServiceCollection();
        services.AddLogging();

        // Register EventBus
        services.AddSingleton<IEventBus, EventBus>();

        // Register handlers as singletons so we can inspect the captured fields
        services.AddSingleton<IEventHandler<BaseTestEvent>>(_baseHandler);
        services.AddSingleton<IEventHandler<DerivedTestEvent>>(_derivedHandler);

        _provider = services.BuildServiceProvider();
        _bus = _provider.GetRequiredService<IEventBus>();

        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (_provider is not null)
            await _provider.DisposeAsync();
    }

    // -----------------------------------------------------------------------
    // Scenario 1: publishing a derived event without explicit type parameter
    // The base handler must receive it via hierarchy walk.
    // -----------------------------------------------------------------------
    [Fact]
    public async Task PublishDerivedEvent_BaseHandlerReceivesIt()
    {
        var evt = new DerivedTestEvent();

        await _bus!.PublishAsync(evt);

        // Allow channel worker to drain
        await Task.Delay(200);

        Assert.Equal(1, _baseHandler!.InvocationCount);
        Assert.Same(evt, _baseHandler.LastEvent);
    }

    // -----------------------------------------------------------------------
    // Scenario 2: the derived-specific handler ALSO receives it when published.
    // -----------------------------------------------------------------------
    [Fact]
    public async Task PublishDerivedEvent_DerivedHandlerAlsoReceivesIt()
    {
        var evt = new DerivedTestEvent();

        await _bus!.PublishAsync(evt);

        await Task.Delay(200);

        Assert.Equal(1, _derivedHandler!.InvocationCount);
        Assert.Same(evt, _derivedHandler.LastEvent);
    }

    // -----------------------------------------------------------------------
    // Scenario 3: no double-dispatch — base handler receives exactly once even
    // when both a base and a derived registration exist.
    // -----------------------------------------------------------------------
    [Fact]
    public async Task PublishDerivedEvent_BaseHandlerNotCalledTwice()
    {
        var evt = new DerivedTestEvent();

        await _bus!.PublishAsync(evt);

        await Task.Delay(200);

        // Exactly one invocation — hierarchy walk deduplicates via seen set
        Assert.Equal(1, _baseHandler!.InvocationCount);
    }

    // -----------------------------------------------------------------------
    // Scenario 4: publishing the base type does NOT trigger derived handlers.
    // -----------------------------------------------------------------------
    [Fact]
    public async Task PublishBaseEvent_DerivedHandlerNotInvoked()
    {
        var evt = new BaseTestEvent();

        await _bus!.PublishAsync(evt);

        await Task.Delay(200);

        Assert.Equal(0, _derivedHandler!.InvocationCount);
    }

    // -----------------------------------------------------------------------
    // Scenario 5: publishing with explicit <BaseTestEvent> type parameter still
    // dispatches to the base handler even when the runtime type is derived.
    // -----------------------------------------------------------------------
    [Fact]
    public async Task PublishWithExplicitBaseType_BaseHandlerReceivesIt()
    {
        var evt = new DerivedTestEvent();

        await _bus!.PublishAsync<BaseTestEvent>(evt);

        await Task.Delay(200);

        Assert.Equal(1, _baseHandler!.InvocationCount);
    }

    // -----------------------------------------------------------------------
    // Scenario 6: multiple different derived types — each triggers base handler,
    // but counts stay independent.
    // -----------------------------------------------------------------------
    [Fact]
    public async Task PublishMultipleDerivedTypes_BaseHandlerReceivesBoth()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IEventBus, EventBus>();

        var trackingHandler = new BaseTestEventHandler();
        services.AddSingleton<IEventHandler<BaseTestEvent>>(trackingHandler);

        await using var provider = services.BuildServiceProvider();
        var bus = provider.GetRequiredService<IEventBus>();

        await bus.PublishAsync(new DerivedTestEvent());
        await bus.PublishAsync(new AnotherDerivedTestEvent());

        await Task.Delay(300);

        Assert.Equal(2, trackingHandler.InvocationCount);
    }
}
