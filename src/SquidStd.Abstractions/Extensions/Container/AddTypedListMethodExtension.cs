using DryIoc;

namespace SquidStd.Abstractions.Extensions.Container;

/// <summary>
/// Extension methods for registering typed lists in the dependency injection container.
/// </summary>
public static class AddTypedListMethodExtension
{
    /// <param name="container">The DryIoc container.</param>
    extension(IContainer container)
    {
        /// <summary>
        /// Adds an entity to a typed list in the DryIoc container.
        /// If the list doesn't exist, it creates and registers a new one.
        /// </summary>
        /// <typeparam name="TListEntity">The type of entities in the list.</typeparam>
        /// <param name="entity">The entity to add to the list.</param>
        /// <returns>The same container for chaining.</returns>
        public IContainer AddToRegisterTypedList<TListEntity>(TListEntity entity)
        {
            // Try resolve existing list
            if (container.IsRegistered<List<TListEntity>>())
            {
                var typedList = container.Resolve<List<TListEntity>>();
                typedList.Add(entity);
            }
            else
            {
                var typedList = new List<TListEntity> { entity };
                container.RegisterInstance(typedList);
            }

            return container;
        }
    }
}
