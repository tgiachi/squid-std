using System.Collections.Concurrent;
using Serilog;
using SquidStd.Core.Data.Commands;
using SquidStd.Core.Interfaces.Commands;
using SquidStd.Services.Core.Services.Internal;
using ILogger = Serilog.ILogger;

namespace SquidStd.Services.Core.Services;

/// <summary>
///     In-process command dispatcher with parallel per-handler fan-out, fault isolation, and a dispatch
///     result. Commands route by their CLR type within the ambient <typeparamref name="TContext" />.
/// </summary>
/// <typeparam name="TContext">The ambient context type.</typeparam>
public sealed class CommandDispatcher<TContext> : ICommandDispatcher<TContext>, IDisposable
{
    private readonly ConcurrentDictionary<Type, List<object>> _handlers = new();
    private readonly ICommandContextFactory<TContext>? _contextFactory;
    private readonly ILogger _logger = Log.ForContext<CommandDispatcher<TContext>>();
    private bool _disposed;

    /// <summary>Initializes the dispatcher without a context factory.</summary>
    public CommandDispatcher()
        : this(null)
    {
    }

    /// <summary>Initializes the dispatcher with an optional context factory.</summary>
    /// <param name="contextFactory">Factory used by the context-less dispatch overload, if any.</param>
    public CommandDispatcher(ICommandContextFactory<TContext>? contextFactory)
    {
        _contextFactory = contextFactory;
    }

    /// <inheritdoc />
    public IDisposable RegisterHandler<TCommand>(ICommandHandler<TCommand, TContext> handler)
        where TCommand : ICommand
    {
        ArgumentNullException.ThrowIfNull(handler);

        return Add(typeof(TCommand), handler);
    }

    /// <inheritdoc />
    public IDisposable Subscribe<TCommand>(Func<TCommand, TContext, CancellationToken, Task> handler)
        where TCommand : ICommand
    {
        ArgumentNullException.ThrowIfNull(handler);

        return Add(typeof(TCommand), new DelegateCommandHandler<TCommand, TContext>(handler));
    }

    /// <inheritdoc />
    public async Task<CommandDispatchResult> DispatchAsync<TCommand>(
        TCommand command, TContext context, CancellationToken cancellationToken = default
    )
        where TCommand : ICommand
    {
        cancellationToken.ThrowIfCancellationRequested();

        var handlers = Snapshot(typeof(TCommand));

        if (handlers is null)
        {
            return new CommandDispatchResult(false, 0, []);
        }

        var tasks = new Task<CommandHandlerError?>[handlers.Length];

        for (var i = 0; i < handlers.Length; i++)
        {
            tasks[i] = DispatchSafeAsync(handlers[i], command, context, cancellationToken);
        }

        var outcomes = await Task.WhenAll(tasks);
        var errors = outcomes.Where(static error => error is not null).Select(static error => error!).ToArray();

        return new CommandDispatchResult(true, handlers.Length, errors);
    }

    /// <inheritdoc />
    public Task<CommandDispatchResult> DispatchAsync<TCommand>(
        TCommand command, CancellationToken cancellationToken = default
    )
        where TCommand : ICommand
    {
        if (_contextFactory is null)
        {
            throw new InvalidOperationException(
                "No ICommandContextFactory<TContext> is registered; pass an explicit context instead."
            );
        }

        return DispatchAsync(command, _contextFactory.Create(), cancellationToken);
    }

    private CommandSubscription Add(Type commandType, object handler)
    {
        ThrowIfDisposed();

        var bucket = _handlers.GetOrAdd(commandType, static _ => []);

        lock (bucket)
        {
            bucket.Add(handler);
        }

        return new CommandSubscription(bucket, handler);
    }

    private object[]? Snapshot(Type commandType)
    {
        if (!_handlers.TryGetValue(commandType, out var bucket))
        {
            return null;
        }

        lock (bucket)
        {
            return bucket.Count == 0 ? null : bucket.ToArray();
        }
    }

    private async Task<CommandHandlerError?> DispatchSafeAsync<TCommand>(
        object handler, TCommand command, TContext context, CancellationToken cancellationToken
    )
        where TCommand : ICommand
    {
        try
        {
            if (handler is ICommandHandler<TCommand, TContext> typedHandler)
            {
                await typedHandler.HandleAsync(command, context, cancellationToken);
            }

            return null;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error(
                ex,
                "Command handler {HandlerType} failed for command {CommandType}",
                handler.GetType().FullName,
                typeof(TCommand).Name
            );

            return new CommandHandlerError(handler.GetType(), ex);
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(CommandDispatcher<TContext>));
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _handlers.Clear();
    }
}
