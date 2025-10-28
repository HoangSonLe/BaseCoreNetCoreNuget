using BaseNetCore.Core.src.Main.Utils;

namespace BaseNetCore.Core.src.Main.DAL.Models.Entities
{
    /// <summary>
    /// Interface for entities that support automatic searchable string generation.
    /// 
    /// Usage (Fully Automatic):
    /// <code>
    /// [SearchableEntity]
    /// public class Product : BaseSearchableEntity
    /// {
    ///     [SearchableField(Order = 1)]
    ///  public string Name { get; set; }
    ///     
    ///     [SearchableField(Order = 2)]
    ///     public string Code { get; set; }
    /// }
    /// 
    /// // Usage - No manual code needed!
    /// var product = new Product { Name = "iPhone 15 Pro", Code = "IP15-PRO" };
    /// _unitOfWork.Repository&lt;Product&gt;().Add(product);
    /// await _unitOfWork.SaveChangesAsync(); // ✅ Auto-generated here!
    /// </code>
    /// </summary>
    public interface ISearchableEntity
    {
        /// <summary>
        /// Non-unicode search string for full-text search without Vietnamese diacritics.
        /// Automatically generated on Add/Update operations.
        /// </summary>
        string NonUnicodeSearchString { get; set; }

        /// <summary>
        /// Generates the search string from [SearchableField] properties.
        /// Called automatically by PostgresDBContext.SaveChangesAsync() and Repository.Add/Update().
        /// </summary>
        void GenerateSearchString();
    }

    /// <summary>
    /// Base class for searchable entities with automatic search string generation.
    /// 
    /// Usage:
    /// <code>
    /// [SearchableEntity]
    /// public class Product : BaseSearchableEntity
    /// {
    ///     public int Id { get; set; }
    ///     
    ///     [SearchableField(Order = 1)]
    ///   public string Name { get; set; }
    ///   
    ///     [SearchableField(Order = 2)]
    ///     public string Code { get; set; }
    ///     
    ///     public decimal Price { get; set; }
    /// }
    /// 
    /// // Usage - Search string automatically generated!
    /// var product = new Product { Name = "Điện thoại iPhone 15", Code = "IP15" };
    /// _unitOfWork.Repository&lt;Product&gt;().Add(product);
    /// await _unitOfWork.SaveChangesAsync();
    /// // Result: NonUnicodeSearchString = "dien thoai iphone 15 ip15"
    /// </code>
    /// </summary>
    public abstract class BaseSearchableEntity : BaseAuditableEntity, ISearchableEntity
    {
        /// <summary>
        /// Non-unicode search string for efficient full-text search.
        /// Auto-generated from [SearchableField] properties.
        /// Example: "iphone 15 pro ip15-pro apple"
        /// </summary>
        public string NonUnicodeSearchString { get; set; } = string.Empty;

        /// <summary>
        /// Initializes NonUnicodeSearchString to empty string.
        /// Actual value generated automatically on SaveChanges.
        /// </summary>
        protected BaseSearchableEntity()
        {
            NonUnicodeSearchString = string.Empty;
        }

        /// <summary>
        /// Generates search string from [SearchableField] properties.
        /// Called automatically by DbContext.SaveChangesAsync() and Repository.Add/Update().
        /// 
        /// Override for custom logic:
        /// <code>
        /// public override void GenerateSearchString()
        /// {
        ///     base.GenerateSearchString();
        ///     if (!string.IsNullOrEmpty(CustomField))
        ///     NonUnicodeSearchString += " " + SearchFieldUtils.RemoveVietnameseDiacritics(CustomField);
        /// }
        /// </code>
        /// </summary>
        public virtual void GenerateSearchString()
        {
            NonUnicodeSearchString = SearchFieldUtils.BuildString(this);
        }
    }
}
