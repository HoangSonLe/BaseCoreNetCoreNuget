namespace BaseNetCore.Core.src.Main.Common.Attributes
{
    /// <summary>
    /// Marks a property as searchable field for non-unicode search string generation.
    /// Similar to Java @SearchableField annotation.
    /// Used to automatically build search strings from entity properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class SearchableFieldAttribute : Attribute
    {
        /// <summary>
        /// Custom name for the searchable field. If not provided, property name is used.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Order of the field in search string concatenation. Lower values appear first.
        /// </summary>
        public int Order { get; set; } = 0;

        /// <summary>
        /// Initializes a new instance of SearchableFieldAttribute.
        /// </summary>
        public SearchableFieldAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of SearchableFieldAttribute with custom name.
        /// </summary>
        /// <param name="name">Custom name for the field</param>
        public SearchableFieldAttribute(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Marks an entity class as searchable, enabling automatic search string generation.
    /// Similar to Java @Entity + @SearchableEntity pattern.
    /// Only entities with this attribute will have search strings generated.
    /// 
    /// Usage:
    /// [SearchableEntity]
    /// public class Product : BaseAuditableEntity
    /// {
    ///     [SearchableField(Order = 1)]
    ///     public string Name { get; set; }
    /// }
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class SearchableEntityAttribute : Attribute
    {
        /// <summary>
        /// Enable or disable search string generation for this entity.
        /// Default: true
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of SearchableEntityAttribute.
        /// </summary>
        public SearchableEntityAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance with enabled flag.
        /// </summary>
        /// <param name="enabled">Enable search string generation</param>
        public SearchableEntityAttribute(bool enabled)
        {
            Enabled = enabled;
        }
    }
}
