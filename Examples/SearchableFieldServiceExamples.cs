using BaseNetCore.Core.Examples;
using BaseNetCore.Core.src.Main.Common.Attributes;
using BaseNetCore.Core.src.Main.Common.Models;
using BaseNetCore.Core.src.Main.DAL.Models.Entities;
using BaseNetCore.Core.src.Main.DAL.Models.Specification;
using BaseNetCore.Core.src.Main.DAL.Repository;
using BaseNetCore.Core.src.Main.Utils;

namespace BaseNetCore.Core.Examples
{
    /// <summary>
    /// Example service demonstrating SearchableField usage in real scenarios.
    /// Shows how [SearchableEntity] attribute enables opt-in search functionality.
    /// Similar to Java Spring Service with @SearchableEntity annotation.
    /// </summary>
    internal class ProductService
    {
        private readonly IRepository<Product> _productRepo;
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IRepository<Product> productRepo, IUnitOfWork unitOfWork)
        {
            _productRepo = productRepo;
         _unitOfWork = unitOfWork;
        }

        #region CRUD Operations with Opt-in Search String Generation

        /// <summary>
        /// Create product - NonUnicodeSearchString is automatically generated
        /// ✅ Because Product has [SearchableEntity] attribute
        /// </summary>
        public async Task<int> CreateProduct(Product product)
        {
            // ✅ Product has [SearchableEntity] → Auto-generate search string
            // No need to manually call product.GenerateSearchString()
            _productRepo.Add(product);
            return await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Update product - NonUnicodeSearchString is automatically regenerated
        /// ✅ Because Product has [SearchableEntity] attribute
        /// </summary>
        public async Task<int> UpdateProduct(Product product)
        {
            // ✅ Product has [SearchableEntity] → Auto-regenerate search string
            _productRepo.Update(product);
            return await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Bulk insert - All search strings are automatically generated
        /// </summary>
        public async Task<int> BulkCreateProducts(List<Product> products)
        {
            // ✅ All Product entities will have search strings auto-generated
            _productRepo.AddRange(products);
            return await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region Search Operations

        /// <summary>
        /// Simple search by keyword (user can type with or without Vietnamese diacritics)
        /// Example searches that work:
        /// - "iphone" ? finds "iPhone 15 Pro"
        /// - "dien thoai" ? finds "?i?n tho?i Samsung"
        /// - "ip15" ? finds products with code "IP15-..."
        /// </summary>
        public async Task<List<Product>> SearchProducts(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<Product>();

            // Normalize the search keyword (remove diacritics, lowercase)
            var normalizedKeyword = SearchFieldUtils.NormalizeSearchText(keyword);

            // Search using NonUnicodeSearchString
            return await _productRepo.GetAllAsync(
             filter: p => p.NonUnicodeSearchString.Contains(normalizedKeyword),
                tracking: false
   );
        }

        /// <summary>
        /// Search with pagination using Specification Pattern
        /// </summary>
        public async Task<PageResponse<Product>> SearchProductsWithPaging(
         string keyword,
              int pageNumber,
     int pageSize)
        {
            var normalizedKeyword = SearchFieldUtils.NormalizeSearchText(keyword);

            var spec = new BaseSpecification<Product>()
        .WithCriteria(p => p.NonUnicodeSearchString.Contains(normalizedKeyword))
       .WithOrderByDescending(p => p.CreatedDate)
  .WithPagedResults(pageNumber, pageSize);

            return await _productRepo.GetWithPagingAsync(spec);
        }

        /// <summary>
        /// Advanced search with multiple filters
        /// Example: Search "iphone" in category "smartphone" with price range
        /// </summary>
        public async Task<PageResponse<Product>> AdvancedSearch(
     string keyword,
              string category = null,
       decimal? minPrice = null,
              decimal? maxPrice = null,
              int pageNumber = 1,
              int pageSize = 20)
        {
            var spec = new BaseSpecification<Product>();

            // Search by keyword
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var normalizedKeyword = SearchFieldUtils.NormalizeSearchText(keyword);
                spec.WithCriteria(p => p.NonUnicodeSearchString.Contains(normalizedKeyword));
            }

            // Filter by category
            if (!string.IsNullOrWhiteSpace(category))
            {
                var normalizedCategory = SearchFieldUtils.NormalizeSearchText(category);
                spec.AndCriteria(p => p.NonUnicodeSearchString.Contains(normalizedCategory));
            }

            // Filter by price range
            if (minPrice.HasValue)
            {
                spec.AndCriteria(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                spec.AndCriteria(p => p.Price <= maxPrice.Value);
            }

            // Apply ordering and paging
            spec.WithOrderByDescending(p => p.CreatedDate)
      .WithPagedResults(pageNumber, pageSize);

            return await _productRepo.GetWithPagingAsync(spec);
        }

        /// <summary>
        /// Search by multiple keywords (AND logic - must contain all keywords)
        /// Example: "iphone 15 pro" ? finds products containing all three terms
        /// </summary>
        public async Task<List<Product>> SearchByAllKeywords(params string[] keywords)
        {
            if (keywords == null || keywords.Length == 0)
                return new List<Product>();

            // Normalize all keywords
            var normalizedKeywords = keywords
             .Select(k => SearchFieldUtils.NormalizeSearchText(k))
          .Where(k => !string.IsNullOrEmpty(k))
   .ToArray();

            if (normalizedKeywords.Length == 0)
                return new List<Product>();

            // Build specification with AND logic
            var spec = new BaseSpecification<Product>();

            foreach (var keyword in normalizedKeywords)
            {
                spec.AndCriteria(p => p.NonUnicodeSearchString.Contains(keyword));
            }

            spec.WithOrderBy(p => p.Name);

            return await _productRepo.GetAsync(spec);
        }

        /// <summary>
        /// Search by multiple keywords (OR logic - must contain at least one keyword)
        /// Example: "iphone samsung xiaomi" ? finds products from any of these brands
        /// </summary>
        public async Task<List<Product>> SearchByAnyKeyword(params string[] keywords)
        {
            if (keywords == null || keywords.Length == 0)
                return new List<Product>();

            var normalizedKeywords = keywords
              .Select(k => SearchFieldUtils.NormalizeSearchText(k))
               .Where(k => !string.IsNullOrEmpty(k))
          .ToArray();

            if (normalizedKeywords.Length == 0)
                return new List<Product>();

            // Build specification with OR logic
            var spec = new BaseSpecification<Product>();

            // First keyword
            spec.WithCriteria(p => p.NonUnicodeSearchString.Contains(normalizedKeywords[0]));

            // Rest with OR
            for (int i = 1; i < normalizedKeywords.Length; i++)
            {
                var keyword = normalizedKeywords[i];
                spec.OrCriteria(p => p.NonUnicodeSearchString.Contains(keyword));
            }

            spec.WithOrderBy(p => p.Name);

            return await _productRepo.GetAsync(spec);
        }

        /// <summary>
        /// Get product suggestions for autocomplete
        /// Returns top 10 matching products
        /// </summary>
        public async Task<List<string>> GetProductSuggestions(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
                return new List<string>();

            var normalizedKeyword = SearchFieldUtils.NormalizeSearchText(keyword);

            var spec = new BaseSpecification<Product>()
                .WithCriteria(p => p.NonUnicodeSearchString.Contains(normalizedKeyword))
.WithOrderBy(p => p.Name)
     .WithPaging(0, 10);

            var products = await _productRepo.GetAsync(spec);

            return products.Select(p => p.Name).ToList();
        }

        #endregion

        #region Statistics & Analytics

        /// <summary>
        /// Count products matching search keyword
        /// </summary>
        public async Task<int> CountProductsByKeyword(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return 0;

            var normalizedKeyword = SearchFieldUtils.NormalizeSearchText(keyword);

            return await _productRepo.CountAsync(
               filter: p => p.NonUnicodeSearchString.Contains(normalizedKeyword)
                 );
        }

        /// <summary>
        /// Check if product exists by search criteria
        /// </summary>
        public async Task<bool> ProductExists(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return false;

            var normalizedKeyword = SearchFieldUtils.NormalizeSearchText(keyword);

            return await _productRepo.AnyAsync(
         filter: p => p.NonUnicodeSearchString.Contains(normalizedKeyword)
);
        }

        #endregion
    }

    /// <summary>
    /// Example: Customer search service
    /// </summary>
    internal class CustomerService
    {
 private readonly IRepository<Customer> _customerRepo;
     private readonly IUnitOfWork _unitOfWork;

        public CustomerService(IRepository<Customer> customerRepo, IUnitOfWork unitOfWork)
 {
  _customerRepo = customerRepo;
     _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Search customers by name, email, phone, company, or tax code
        /// User can search with ANY information without knowing which field contains it
        /// Examples that work:
        /// - "nguyen" ? finds "Nguy?n V?n A", "Nguy?n Th? B"
        /// - "0901234567" ? finds customer with this phone
        /// - "abc company" ? finds "Công ty TNHH ABC"
        /// - "0123456789" ? finds customer with this tax code
        /// </summary>
        public async Task<PageResponse<Customer>> SearchCustomers(
                 string keyword,
               int pageNumber = 1,
         int pageSize = 20)
        {
            var normalizedKeyword = SearchFieldUtils.NormalizeSearchText(keyword);

            var spec = new BaseSpecification<Customer>()
     .WithCriteria(c => c.NonUnicodeSearchString.Contains(normalizedKeyword))
  .WithOrderBy(c => c.FullName)
      .WithPagedResults(pageNumber, pageSize);

            return await _customerRepo.GetWithPagingAsync(spec);
        }

        /// <summary>
        /// Quick customer lookup for forms (autocomplete)
        /// </summary>
        public async Task<List<CustomerSuggestion>> GetCustomerSuggestions(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
                return new List<CustomerSuggestion>();

            var normalizedKeyword = SearchFieldUtils.NormalizeSearchText(keyword);

            var spec = new BaseSpecification<Customer>()
                .WithCriteria(c => c.NonUnicodeSearchString.Contains(normalizedKeyword))
                 .WithOrderBy(c => c.FullName)
                .WithPaging(0, 10);

            var customers = await _customerRepo.GetAsync(spec);

            return customers.Select(c => new CustomerSuggestion
            {
                Id = c.Id,
                FullName = c.FullName,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber
            }).ToList();
        }
    }

    /// <summary>
    /// DTO for customer suggestions
    /// </summary>
    internal class CustomerSuggestion
  {
     public Guid Id { get; set; }
     public string FullName { get; set; }
 public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }

    /// <summary>
    /// Example: Named Specification for Product Search
    /// This approach is cleaner for complex searches
    /// Only works because Product has [SearchableEntity] attribute
    /// </summary>
    internal class ProductBySearchTermSpec : BaseSpecification<Product>
    {
        public ProductBySearchTermSpec(string searchTerm)
        {
            var normalized = SearchFieldUtils.NormalizeSearchText(searchTerm);

  WithCriteria(p => p.NonUnicodeSearchString.Contains(normalized));
            WithOrderBy(p => p.Name);
        }
  }

    /// <summary>
    /// Example: Complex search specification with multiple filters
    /// </summary>
    internal class ProductSearchSpec : BaseSpecification<Product>
    {
  public ProductSearchSpec(
     string searchTerm,
           string category = null,
         string brand = null,
    decimal? minPrice = null,
         decimal? maxPrice = null,
  bool activeOnly = true)
  {
      // Base search criteria
      if (!string.IsNullOrWhiteSpace(searchTerm))
      {
        var normalized = SearchFieldUtils.NormalizeSearchText(searchTerm);
                WithCriteria(p => p.NonUnicodeSearchString.Contains(normalized));
            }

            // Category filter
            if (!string.IsNullOrWhiteSpace(category))
            {
                var normalizedCategory = SearchFieldUtils.NormalizeSearchText(category);
                AndCriteria(p => p.NonUnicodeSearchString.Contains(normalizedCategory));
            }

            // Brand filter
            if (!string.IsNullOrWhiteSpace(brand))
            {
                var normalizedBrand = SearchFieldUtils.NormalizeSearchText(brand);
                AndCriteria(p => p.NonUnicodeSearchString.Contains(normalizedBrand));
            }

            // Price range
            if (minPrice.HasValue)
            {
                AndCriteria(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                AndCriteria(p => p.Price <= maxPrice.Value);
            }

            // Active filter
            if (activeOnly)
            {
                AndCriteria(p => p.IsActive);
            }

            WithOrderByDescending(p => p.CreatedDate);
        }
    }

    /// <summary>
    /// Example: Order service WITHOUT search (performance optimized)
    /// Shows the benefit of opt-in design
    /// </summary>
    internal class OrderService
    {
  private readonly IRepository<SalesOrder> _orderRepo;
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IRepository<SalesOrder> orderRepo, IUnitOfWork unitOfWork)
        {
      _orderRepo = orderRepo;
     _unitOfWork = unitOfWork;
     }

        /// <summary>
        /// Create order - NO search string generation
        /// ❌ SalesOrder does NOT have [SearchableEntity] attribute
        /// → Better performance for high-volume transactional data
        /// </summary>
        public async Task<int> CreateOrder(SalesOrder order)
        {
            // ❌ SalesOrder doesn't have [SearchableEntity] → No search string generation
            // → Faster insert performance
            _orderRepo.Add(order);
            return await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Bulk insert orders - NO search overhead
        /// Perfect for high-volume batch processing
        /// </summary>
        public async Task<int> BulkCreateOrders(List<SalesOrder> orders)
        {
            // ❌ No search string generation → Maximum insert performance
            _orderRepo.AddRange(orders);
            return await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Find orders by order number (direct property search)
        /// No need for NonUnicodeSearchString for exact matches
        /// </summary>
        public async Task<SalesOrder> FindByOrderNumber(string orderNumber)
        {
            return await _orderRepo.FindAsync(o => o.OrderNumber == orderNumber);
        }

        /// <summary>
        /// Find orders by date range (structured query)
        /// Transactional data is better queried by structured fields
        /// </summary>
        public async Task<List<SalesOrder>> FindByDateRange(DateTime startDate, DateTime endDate)
        {
            var spec = new BaseSpecification<SalesOrder>()
         .WithCriteria(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
        .WithOrderByDescending(o => o.OrderDate);

            return await _orderRepo.GetAsync(spec);
        }
    }

    /// <summary>
    /// Example: Mixed service handling both searchable and non-searchable entities
    /// </summary>
    internal class ECommerceService
    {
     private readonly IRepository<Product> _productRepo;
        private readonly IRepository<SalesOrder> _orderRepo;
   private readonly IRepository<Customer> _customerRepo;
        private readonly IUnitOfWork _unitOfWork;

        public ECommerceService(
        IRepository<Product> productRepo,
 IRepository<SalesOrder> orderRepo,
                 IRepository<Customer> customerRepo,
          IUnitOfWork unitOfWork)
        {
       _productRepo = productRepo;
            _orderRepo = orderRepo;
          _customerRepo = customerRepo;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Create complete order with products
        /// Shows performance difference between searchable and non-searchable entities
        /// </summary>
        public async Task<int> CreateOrderWithDetails(SalesOrder order, List<SalesOrderItem> items)
        {
            // ❌ SalesOrder: No [SearchableEntity] → Fast insert
            _orderRepo.Add(order);

            // ❌ SalesOrderItem: No [SearchableEntity] → Fast bulk insert
            // (Assume SalesOrderItem repository)
            // _orderItemRepo.AddRange(items);

            // Total: Very fast because no search string generation overhead
            return await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Universal search across searchable entities
        /// Only searches entities marked with [SearchableEntity]
        /// </summary>
        public async Task<UniversalSearchResult> UniversalSearch(string keyword, int pageSize = 20)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new UniversalSearchResult();

            var normalizedKeyword = SearchFieldUtils.NormalizeSearchText(keyword);

            // ✅ Product has [SearchableEntity] → Can use NonUnicodeSearchString
            var productSpec = new BaseSpecification<Product>()
                 .WithCriteria(p => p.NonUnicodeSearchString.Contains(normalizedKeyword))
             .WithPaging(0, pageSize);

            // ✅ Customer has [SearchableEntity] → Can use NonUnicodeSearchString
            var customerSpec = new BaseSpecification<Customer>()
           .WithCriteria(c => c.NonUnicodeSearchString.Contains(normalizedKeyword))
     .WithPaging(0, pageSize);

            // ❌ SalesOrder does NOT have [SearchableEntity] → Use structured search
            var orderSpec = new BaseSpecification<SalesOrder>()
                  .WithCriteria(o => o.OrderNumber.Contains(keyword))  // Direct property search
                .WithPaging(0, pageSize);

            var products = await _productRepo.GetAsync(productSpec);
            var customers = await _customerRepo.GetAsync(customerSpec);
            var orders = await _orderRepo.GetAsync(orderSpec);

            return new UniversalSearchResult
            {
                Products = products,
                Customers = customers,
                Orders = orders
            };
        }

        /// <summary>
        /// Get statistics showing the benefit of opt-in design
        /// </summary>
        public async Task<PerformanceStats> GetPerformanceStats()
        {
            // Entities WITH [SearchableEntity] - slower inserts but searchable
            var productsCount = await _productRepo.CountAsync();
            var customersCount = await _customerRepo.CountAsync();

            // Entities WITHOUT [SearchableEntity] - faster inserts
            var ordersCount = await _orderRepo.CountAsync();

            return new PerformanceStats
            {
                SearchableEntities = productsCount + customersCount,
                NonSearchableEntities = ordersCount,
                Message = $"Searchable: {productsCount + customersCount}, Non-searchable: {ordersCount} (faster)"
            };
        }
    }

    #region Supporting Classes

    internal class UniversalSearchResult
    {
        public List<Product> Products { get; set; } = new();
        public List<Customer> Customers { get; set; } = new();
        public List<SalesOrder> Orders { get; set; } = new();
    }

    internal class PerformanceStats
    {
        public int SearchableEntities { get; set; }
        public int NonSearchableEntities { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// SalesOrder entity WITHOUT [SearchableEntity] attribute
    /// High-volume transactional data that doesn't need full-text search
 /// </summary>
    internal class SalesOrder : BaseAuditableEntity
    {
 public int Id { get; set; }
        public string OrderNumber { get; set; }
    public decimal TotalAmount { get; set; }
        public DateTime OrderDate { get; set; }
      public Guid CustomerId { get; set; }

        // ❌ No NonUnicodeSearchString property
        // ❌ No search string generation overhead
        // ✅ Faster insert/update operations
        // ✅ Less storage space
    }

    /// <summary>
    /// SalesOrderItem entity WITHOUT [SearchableEntity] attribute
    /// Child entity that doesn't need independent search
    /// </summary>
    internal class SalesOrderItem : BaseAuditableEntity
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
   public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        // ❌ No search support needed
        // Search through SalesOrder or Product is sufficient
    }

    #endregion
}
