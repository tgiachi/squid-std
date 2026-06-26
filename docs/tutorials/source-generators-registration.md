# Generated registrations

Use `SquidStd.Generators` to replace repetitive manual registration calls with compile-time generated DryIoc extensions.

## Add the generator

```bash
dotnet add package SquidStd.Generators
```

## Standard services

```csharp
using SquidStd.Abstractions.Attributes;
using SquidStd.Abstractions.Interfaces.Services;
using SquidStd.Generators.Services;

public interface IGreetingService : ISquidStdService { }

[RegisterStdService(typeof(IGreetingService), Priority = 10)]
public sealed class GreetingService : IGreetingService
{
    public ValueTask StartAsync(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;

    public ValueTask StopAsync(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;
}

container.RegisterGeneratedStdServices();
```

## Config sections

```csharp
using SquidStd.Abstractions.Attributes;
using SquidStd.Generators.Config;

[RegisterConfigSection("greeting", Priority = -10)]
public sealed class GreetingConfig
{
    public string Message { get; set; } = "hello";
}

container.RegisterGeneratedConfigSections();
```

## Job handlers

```csharp
using SquidStd.Generators.Workers;
using SquidStd.Workers.Abstractions.Data;
using SquidStd.Workers.Attributes;
using SquidStd.Workers.Interfaces;

[RegisterJobHandler]
public sealed class GreetingJobHandler : IJobHandler
{
    public string JobName => "greet";

    public Task HandleAsync(JobRequest job, CancellationToken cancellationToken)
    {
        Console.WriteLine(job.JobName);

        return Task.CompletedTask;
    }
}

container.RegisterGeneratedJobHandlers();
```

## Lua script modules

```csharp
using SquidStd.Generators.Scripting.Lua;
using SquidStd.Scripting.Lua.Attributes;
using SquidStd.Scripting.Lua.Attributes.Scripts;

[RegisterScriptModule]
[ScriptModule("greeting")]
public sealed class GreetingScriptModule { }

container.RegisterGeneratedScriptModules();
```

The Lua generator only emits `RegisterScriptModule<T>()`; `[ScriptModule("name")]` is still required because the Lua runtime uses it as the module name.
