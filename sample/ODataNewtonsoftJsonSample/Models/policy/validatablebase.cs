namespace Microsoft.Naas.Contracts.ControlPlane.MsGraphModels.Validations;

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;

public abstract class ValidatableBase : IValidatable
{
    /// <summary>
    /// A binded entity should contain only one property, which is an identifier property.
    /// </summary>
    public const string BindedEntityIndentifierPropertyName = "Id";

    /// <summary>
    /// The properties that are allowed to be updated for this entity.
    /// Defualt behavior is that non of the properties are allowed for update (empty set).
    /// </summary>
    [InternalProperty]
    public virtual ISet<string> AllowedPropertiesForUpdate { get => new HashSet<string>(); }

    /// <summary>
    /// True if this is a binded entity.
    /// If so, the entity should have only ID property.
    /// By default, an entity is not a binded entity.
    /// </summary>
    [InternalProperty]
    private bool? IsBinded { get; set; } = null;

    public virtual void ValidateEntityCreation(ILogger logger)
    {
        ValidateRequiredProperties(logger);
        ValidateEntityProperties(logger);
        ValidateEntityStructure(logger);
    }

    public virtual void ValidateEntityDelta(ILogger logger)
    {
    }

    /// <summary>
    /// Checks if the entity is binded, meaning it is a link to an existing entity and does not represent a new entity.
    /// All properies of a binded entity are null besides the ID.
    /// </summary>
    public bool IsBindedEntity(ILogger logger)
    {
        logger.LogDebug("Checking whether this is a binded entity.");

        if (IsBinded is not null)
        {
            return IsBinded.Value;
        }

        PropertyInfo[] properties = GetType().GetProperties(); // Reflection is needed as Policy is an abstract type.
        bool idExists = false, onlyIdExist = true;

        foreach (PropertyInfo property in properties)
        {
            if (property.Name.Equals(BindedEntityIndentifierPropertyName))
            {
                idExists = property.GetValue(this) is not null;
            }
            else if (IsPropertyValueExistsInEntityModelPrespective(property))
            {
                onlyIdExist = false;
            }
        }

        if (idExists && onlyIdExist)
        {
            logger.LogDebug("This entity is a binded entity.");
            IsBinded = true;
            return IsBinded.Value;
        }

        if (idExists)
        {
            // than some properties are not null.
            throw new Exception($"A binding (linking) action of an entity ID is not allowed with other entity properties");
        }

        logger.LogDebug("This entity is not a binded entity.");
        IsBinded = false;
        return IsBinded.Value;
    }

    /// <summary>
    /// Checks if the entity is hidden.
    /// If so, the entity should not be part of the controllers api responses.
    /// By default, an entity is not a hidden entity.
    /// </summary>
    public virtual bool IsHiddenEntity()
    {
        return false;
    }

    protected abstract void ValidateEntityProperties(ILogger logger);

    protected abstract void ValidateEntityStructure(ILogger logger);

    protected void ValidateRequiredProperties(ILogger logger)
    {
    }

    /// <summary>
    /// A property of an entity model "exists" if it is not an interanl property and it's value is not null.
    /// </summary>
    private bool IsPropertyValueExistsInEntityModelPrespective(PropertyInfo property)
    {
        // TODO: remove version from this is after Task 1888183: Handle CP entities versioning.
        return !Attribute.IsDefined(property, typeof(InternalPropertyAttribute)) &&
            property.GetValue(this) is not null &&
            !property.Name.Equals("version", StringComparison.OrdinalIgnoreCase);
    }
}