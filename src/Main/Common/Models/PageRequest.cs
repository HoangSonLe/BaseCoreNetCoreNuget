namespace BaseNetCore.Core.src.Main.Common.Models
{
    public class PageRequest
    {
        private const int MAX_PAGE_SIZE = 500;
        private const int MIN_PAGE_SIZE = 1;
        private const int DEFAULT_PAGE_SIZE = 50;

        public int Page { get; init; } = 1;
        public int Size { get; init; } = DEFAULT_PAGE_SIZE;

        public PageRequest()
        {
        }

        public PageRequest(int page, int size)
        {
            Page = page < 1 ? 1 : page;
            // Ensure size is between MIN_PAGE_SIZE and MAX_PAGE_SIZE
            Size = size < MIN_PAGE_SIZE ? MIN_PAGE_SIZE : (size > MAX_PAGE_SIZE ? MAX_PAGE_SIZE : size);
        }

        public static PageRequest Of(int page, int size)
        {
            // Normalize size before creating instance
            if (size < MIN_PAGE_SIZE)
            {
                size = MIN_PAGE_SIZE;
            }
            else if (size > MAX_PAGE_SIZE)
            {
                size = MAX_PAGE_SIZE;
            }
            return new PageRequest(page, size);
        }

        public int CurrentPage => Page;
        public int Skip => (Page - 1) * Size;
        public int Take => Size;
    }
}
