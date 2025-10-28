using BaseNetCore.Core.src.Main.Common.Attributes;
using BaseNetCore.Core.src.Main.DAL.Models.Entities;
using BaseNetCore.Core.src.Main.Utils;

namespace BaseNetCore.Core.Examples
{
    /// <summary>
    /// Example entity demonstrating SearchableField usage with [SearchableEntity] attribute.
    /// This is similar to Java's @SearchableEntity + @SearchableField annotation pattern.
    /// 
    /// ✅ Approach 1: Inherit from BaseSearchableEntity (Quick & Easy)
    /// </summary>
    [SearchableEntity]  // ← Must have this attribute to enable search
    internal class Product : BaseSearchableEntity
    {
        public int Id { get; set; }

        /// <summary>
        /// Product name - Searchable with Order 1 (appears first in search string)
        /// Example: "Điện thoại iPhone 15 Pro Max"
        /// </summary>
        [SearchableField(Order = 1)]
        public string Name { get; set; }

        /// <summary>
        /// Product code/SKU - Searchable with Order 2
        /// Example: "IP15-PM-256-BLK"
        /// </summary>
        [SearchableField(Name = "ProductCode", Order = 2)]
        public string Code { get; set; }

        /// <summary>
        /// Brand name - Searchable with Order 3
        /// Example: "Apple"
        /// </summary>
        [SearchableField(Order = 3)]
        public string Brand { get; set; }

        /// <summary>
        /// Product description - Searchable with Order 4
        /// Example: "Điện thoại cao cấp với chip A17 Pro"
        /// </summary>
        [SearchableField(Order = 4)]
        public string Description { get; set; }

        /// <summary>
        /// Category - Searchable with Order 5
        /// Example: "Điện thoại thông minh"
        /// </summary>
        [SearchableField(Order = 5)]
        public string Category { get; set; }

        // Non-searchable properties (no SearchableField attribute)
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// NonUnicodeSearchString will be automatically generated from above searchable fields:
        /// Example result: "dien thoai iphone 15 pro max ip15-pm-256-blk apple dien thoai cao cap voi chip a17 pro dien thoai thong minh"
        /// 
        /// This allows searching with:
        /// - "iphone" → Found ✓
        /// - "dien thoai" → Found ✓ (without diacritics)
        /// - "ip15" → Found ✓
        /// - "apple cao cap" → Found ✓ (multiple terms)
        /// </summary>
    }

    /// <summary>
    /// Example: Customer entity with searchable fields
    /// ✅ Approach 2: Implement ISearchableEntity directly (Full Control)
    /// </summary>
    [SearchableEntity]  // ← Must have this attribute
    internal class Customer : BaseAuditableEntity, ISearchableEntity
    {
        public Guid Id { get; set; }

        [SearchableField(Order = 1)]
        public string FullName { get; set; }  // "Nguyễn Văn An"

        [SearchableField(Order = 2)]
        public string Email { get; set; }  // "nguyenvanan@email.com"

        [SearchableField(Order = 3)]
        public string PhoneNumber { get; set; }  // "0901234567"

        [SearchableField(Order = 4)]
        public string CompanyName { get; set; }  // "Công ty TNHH ABC"

        [SearchableField(Order = 5)]
        public string TaxCode { get; set; }  // "0123456789"

        // Non-searchable
        public string Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public CustomerType Type { get; set; }

        // ✅ Required by ISearchableEntity
        public string NonUnicodeSearchString { get; set; }

        // ✅ Required by ISearchableEntity
        public void GenerateSearchString()
        {
            NonUnicodeSearchString = SearchFieldUtils.BuildString(this);
        }

        /// <summary>
        /// NonUnicodeSearchString example:
        /// "nguyen van an nguyenvanan@email.com 0901234567 cong ty tnhh abc 0123456789"
        /// 
        /// Search examples:
        /// - "nguyen van" → Found ✓
        /// - "0901234567" → Found ✓
        /// - "cong ty abc" → Found ✓
        /// </summary>
    }

    /// <summary>
    /// Example: Employee with custom search string generation
    /// ✅ Approach 3: Inherit BaseSearchableEntity and override GenerateSearchString
    /// </summary>
    [SearchableEntity]
    internal class Employee : BaseSearchableEntity
    {
        public int Id { get; set; }

        [SearchableField(Order = 1)]
        public string EmployeeCode { get; set; }  // "EMP001"

        [SearchableField(Order = 2)]
        public string FullName { get; set; }  // "Trần Thị Bình"

        [SearchableField(Order = 3)]
        public string Department { get; set; }  // "Phòng Kỹ Thuật"

        [SearchableField(Order = 4)]
        public string Position { get; set; }// "Lập Trình Viên Senior"

        [SearchableField(Order = 5)]
        public string Skills { get; set; }  // "C#, .NET Core, SQL Server, Azure"

        public string Email { get; set; }
        public decimal Salary { get; set; }
        public DateTime HireDate { get; set; }

        /// <summary>
        /// Override to add custom logic for search string generation
        /// </summary>
        public override void GenerateSearchString()
        {
            // Call base to generate from SearchableField attributes
            base.GenerateSearchString();

            // You can add custom logic here if needed
            // For example, add email to search string even if it doesn't have [SearchableField]
            if (!string.IsNullOrEmpty(Email))
            {
                var emailNormalized = SearchFieldUtils.RemoveVietnameseDiacritics(Email);
                NonUnicodeSearchString = $"{NonUnicodeSearchString} {emailNormalized}".Trim();
            }
        }

        /// <summary>
        /// NonUnicodeSearchString example:
        /// "emp001 tran thi binh phong ky thuat lap trinh vien senior c#, .net core, sql server, azure tran.binh@company.com"
        /// </summary>
    }

    /// <summary>
    /// Example: Order entity WITHOUT search support
    /// ❌ No [SearchableEntity] attribute = No automatic search string generation
    /// </summary>
    internal class Order : BaseAuditableEntity
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }

        // This entity will NOT have search string generation
        // Even if you add [SearchableField], it won't work without [SearchableEntity]
    }

    /// <summary>
    /// Example: Category with disabled search
    /// ❌ [SearchableEntity(Enabled = false)] = Search disabled
    /// </summary>
    [SearchableEntity(Enabled = false)]
    internal class Category : BaseSearchableEntity
    {
        public int Id { get; set; }

        [SearchableField]
        public string Name { get; set; }

        // Search is explicitly disabled, so GenerateSearchString won't be called
    }

    internal enum CustomerType
    {
        Individual = 1,
        Corporate = 2,
        Government = 3
    }
}
