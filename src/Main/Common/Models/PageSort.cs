namespace BaseNetCore.Core.src.Main.Common.Models
{
    public static class PageSort
    {
        private const string ASC = "ascend";
        private const string DESC = "descend";
        private const string DEFAULT_FIELD = "CreatedAt";

        /// <summary>
        /// Parse sort string in format "field_direction" (e.g. "name_ascend" or "age_descend")
        /// into a tuple containing field and direction.
        /// </summary>
        public static (string Field, bool Descending) From(string? sorter)
        {
            if (string.IsNullOrWhiteSpace(sorter))
                return (DEFAULT_FIELD, true);

            var parts = sorter.Split('_', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                var field = parts[0];
                var direction = parts[1].ToLowerInvariant();

                return direction switch
                {
                    ASC => (field, false),
                    DESC => (field, true),
                    _ => (DEFAULT_FIELD, true)
                };
            }

            return (DEFAULT_FIELD, true);
        }
    }
}
