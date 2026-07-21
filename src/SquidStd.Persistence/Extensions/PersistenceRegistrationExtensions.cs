using DryIoc;
using SquidStd.Abstractions.Extensions.Config;
using SquidStd.Abstractions.Extensions.Container;
using SquidStd.Abstractions.Extensions.Services;
using SquidStd.Core.Directories;
using SquidStd.Core.Interfaces.Serialization;
using SquidStd.Persistence.Abstractions;
using SquidStd.Persistence.Abstractions.Data;
using SquidStd.Persistence.Abstractions.Interfaces.Persistence;
using SquidStd.Persistence.Data;
using SquidStd.Persistence.Data.Internal;
using SquidStd.Persistence.Internal;
using SquidStd.Persistence.Services;

namespace SquidStd.Persistence.Extensions;

/// <summary>
/// Registers persisted entity types for descriptor construction at bootstrap, and wires the
/// persistence stack itself - entity registry, journal, snapshot service, and the
/// <see cref="IPersistenceService" /> lifecycle service.
/// </summary>
public static class PersistenceRegistrationExtensions
{
    extension(IContainer container)
    {
        /// <summary>Records a persisted entity type; its descriptor is built and registered at bootstrap.</summary>
        public IContainer RegisterPersistedEntity<TEntity, TKey>(
            ushort typeId,
            string typeName,
            int schemaVersion,
            Func<TEntity, TKey> keySelector,
            Action<TEntity, TKey>? keySetter = null,
            IIdGenerator<TKey>? idGenerator = null,
            ushort? legacyTypeId = null
        )
            where TKey : notnull
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(typeName);
            ArgumentNullException.ThrowIfNull(keySelector);

            if (typeId == ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(typeId),
                    "Type id 65535 is reserved for the internal id-sequence bucket."
                );
            }

            container.AddToRegisterTypedList(
                new PersistedEntityRegistration(
                    typeId,
                    typeName,
                    (registry, resolver) => registry.Register(
                        new PersistenceEntityDescriptor<TEntity, TKey>(
                            resolver.Resolve<IDataSerializer>(),
                            resolver.Resolve<IDataDeserializer>(),
                            typeId,
                            typeName,
                            schemaVersion,
                            keySelector,
                            keySetter,
                            idGenerator,
                            legacyTypeId
                        )
                    )
                )
            );

            return container;
        }

        /// <summary>
        /// Records a persisted entity whose type id is derived from <paramref name="typeName" />. Prefer
        /// this over the explicit-id overload: nothing has to know which ids are already taken, which is
        /// the only way a plugin author can register an entity safely.
        /// </summary>
        /// <param name="legacyTypeId">
        /// The id this store used before ids became derived. Set it on an entity that already has saved
        /// data, so its snapshot is renamed and its journal entries are translated on the next start.
        /// </param>
        public IContainer RegisterPersistedEntity<TEntity, TKey>(
            string typeName,
            int schemaVersion,
            Func<TEntity, TKey> keySelector,
            Action<TEntity, TKey>? keySetter = null,
            IIdGenerator<TKey>? idGenerator = null,
            ushort? legacyTypeId = null
        )
            where TKey : notnull
            => container.RegisterPersistedEntity(
                PersistedTypeId.Derive(typeName),
                typeName,
                schemaVersion,
                keySelector,
                keySetter,
                idGenerator,
                legacyTypeId
            );

        /// <summary>Builds and registers all recorded persisted-entity descriptors into the registry.</summary>
        public IContainer ApplyPersistedEntityRegistrations()
        {
            if (!container.IsRegistered<List<PersistedEntityRegistration>>())
            {
                return container;
            }

            var registry = container.Resolve<IPersistenceEntityRegistry>();
            var registrations = container.Resolve<List<PersistedEntityRegistration>>();

            for (var i = 0; i < registrations.Count; i++)
            {
                registrations[i].Register(registry, container);
            }

            return container;
        }

        /// <summary>
        /// Registers a persistence seeder by type; it is resolved from the container (with its
        /// dependencies) when the persistence service is constructed, and runs only on a fresh
        /// save, in registration order.
        /// The seeder must not constructor-inject IPersistenceService; the service arrives as the
        /// SeedAsync parameter.
        /// </summary>
        /// <typeparam name="TSeeder">The seeder type.</typeparam>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterPersistenceSeeder<TSeeder>()
            where TSeeder : class, IPersistenceSeeder
        {
            container.Register<TSeeder>(Reuse.Singleton);
            container.AddToRegisterTypedList(
                new PersistenceSeederRegistration(static resolver => resolver.Resolve<TSeeder>())
            );

            return container;
        }

        /// <summary>
        /// Registers a delegate-backed persistence seeder; it runs only on a fresh save, in
        /// registration order.
        /// </summary>
        /// <param name="seed">The seeding callback.</param>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterPersistenceSeeder(Func<IPersistenceService, CancellationToken, ValueTask> seed)
        {
            ArgumentNullException.ThrowIfNull(seed);
            container.AddToRegisterTypedList(
                new PersistenceSeederRegistration(_ => new DelegatePersistenceSeeder(seed))
            );

            return container;
        }

        /// <summary>
        /// Registers the whole persistence stack: entity registry (with the recorded
        /// <see cref="RegisterPersistedEntity{TEntity,TKey}" /> registrations applied on first
        /// resolution), journal, snapshot service, and <see cref="IPersistenceService" /> as a
        /// lifecycle service (snapshot load and journal replay at start, autosave loop, final
        /// snapshot at stop). Requires a registered <see cref="IDataSerializer" />. When
        /// <paramref name="config" /> is null the "persistence" YAML section is bound; the
        /// save directory defaults to the managed "save" directory under the root.
        /// Do not call <see cref="ApplyPersistedEntityRegistrations" /> manually when using
        /// this registration.
        /// </summary>
        /// <param name="config">Explicit configuration; when set, the YAML section is not bound and the file is ignored for this section.</param>
        /// <returns>The same container for chaining.</returns>
        public IContainer RegisterPersistence(PersistenceConfig? config = null)
        {
            if (!container.IsRegistered<IDataSerializer>())
            {
                throw new InvalidOperationException(
                    "Register a data serializer before RegisterPersistence "
                    + "(RegisterDataSerializer(), RegisterMessagePackSerializer() or RegisterYamlDataSerializer())."
                );
            }

            if (config is not null)
            {
                container.RegisterInstance(config, IfAlreadyRegistered.Replace);
            }
            else
            {
                container.RegisterConfigSection("persistence", static () => new PersistenceConfig(), -40);
            }

            container.Register<IPersistenceEntityRegistry, PersistenceEntityRegistry>(Reuse.Singleton);
            container.RegisterInitializer<IPersistenceEntityRegistry>(
                static (registry, resolver) =>
                {
                    var registrations = resolver.Resolve<List<PersistedEntityRegistration>>(IfUnresolved.ReturnDefault);

                    if (registrations is null)
                    {
                        return;
                    }

                    for (var i = 0; i < registrations.Count; i++)
                    {
                        registrations[i].Register(registry, resolver);
                    }
                }
            );

            container.RegisterDelegate<IJournalService>(
                static resolver =>
                {
                    var effective = resolver.Resolve<PersistenceConfig>();

                    return new BinaryJournalService(
                        Path.Combine(ResolveSaveDirectory(resolver, effective), effective.JournalFileName),
                        effective.DurabilityMode,
                        effective.EnableFileLock
                    );
                },
                Reuse.Singleton
            );

            container.RegisterDelegate<ISnapshotService>(
                static resolver =>
                {
                    var effective = resolver.Resolve<PersistenceConfig>();

                    return new SnapshotService(
                        ResolveSaveDirectory(resolver, effective),
                        effective.SnapshotFileSuffix,
                        effective.DurabilityMode
                    );
                },
                Reuse.Singleton
            );

            container.RegisterDelegate<IReadOnlyList<IPersistenceSeeder>>(
                static resolver =>
                {
                    var recorded = resolver.Resolve<List<PersistenceSeederRegistration>>(IfUnresolved.ReturnDefault);

                    return recorded is null
                               ? []
                               : recorded.Select(registration => registration.Resolve(resolver)).ToList();
                },
                Reuse.Singleton
            );

            return container.RegisterStdService<IPersistenceService, PersistenceService>(-1);
        }
    }

    private static string ResolveSaveDirectory(IResolverContext resolver, PersistenceConfig config)
        => !string.IsNullOrWhiteSpace(config.SaveDirectory)
               ? config.SaveDirectory
               : resolver.Resolve<DirectoriesConfig>().RegisterDirectory("save");
}
