using Microsoft.EntityFrameworkCore;

namespace DerMagereStudent.EntityFrameworkCore.Extensions; 

public static class DbContextExtensions {
	/// <summary>
	/// <para>
	///		Wrapper around the method <see cref="DbContext.FindAsync{TEntity}(object[])"/>. Gets the array of primary key values
	///		from the object instance using the entity type definitions from the DB context.
	/// </para>
	/// <para>
	///		Can be used the get the existing tracked entity for an instance with the same primary key data or to execute a query
	///		for an entry in the database without worrying about the order of columns within the definition of the composite primary key
	///		defined in the <paramref name="context"/>.
	/// </para>
	/// </summary>
	/// <param name="context">The DB context to get the reference from.</param>
	/// <param name="entity">The entity to get the primary key value/values from.</param>
	/// <typeparam name="TEntity">The type of the entity.</typeparam>
	/// <returns></returns>
	public static async ValueTask<TEntity?> FindTrackedAsync<TEntity>(this DbContext context, TEntity entity, CancellationToken cancellationToken = default) where TEntity : class {
		cancellationToken.ThrowIfCancellationRequested();
		
		// Get the EF Core model type definition for TEntity, which contains all the model information including keys, unique constraints, ...
		var entityType = context.Model.FindRuntimeEntityType(typeof(TEntity));
		
		// Get the collection pf properties (columns) that define the primary or composite primary key
		var keyProperties = entityType?.FindPrimaryKey()?.Properties;
		
		// If the entity has no primary or composite primary key, this method cannot continue
		if (keyProperties is null || keyProperties.Count == 0)
			return null;
		
		// Get all the values from the primary key properties of the entity instance
		var keyValues = keyProperties.Select(prop => prop.GetGetter().GetClrValue(entity)).ToArray();
		
		// Look for an existing tracked entity or execute a DB query to look for an entry with the given primary key values
		return await context.FindAsync<TEntity>(keyValues, cancellationToken);
	}
}