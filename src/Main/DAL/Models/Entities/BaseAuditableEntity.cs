using BaseNetCore.Core.src.Main.Common.Enums;

namespace BaseNetCore.Core.src.Main.DAL.Models.Entities
{
    /// <summary>
    /// Base auditable entity WITHOUT automatic searchable support.
    /// To enable search functionality, mark your entity with [SearchableEntity] attribute
    /// and implement ISearchableEntity interface.
    /// 
    /// Example:
    /// [SearchableEntity]
    /// public class Product : BaseAuditableEntity, ISearchableEntity
    /// {
    ///     [SearchableField(Order = 1)]
    ///public string Name { get; set; }
    ///     
    ///     public string NonUnicodeSearchString { get; set; }
    ///     
    ///     public void GenerateSearchString()
    ///     {
    ///       NonUnicodeSearchString = SearchFieldUtils.BuildString(this);
    ///     }
    /// }
    /// </summary>
    public abstract class BaseAuditableEntity
    {
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public int CreatedBy { get; set; } = 1;

        public DateTime? UpdatedDate { get; set; } = DateTime.Now;

        public int? UpdatedBy { get; set; } = 1;

        public EState State { get; set; } = EState.Active;
    }
}
