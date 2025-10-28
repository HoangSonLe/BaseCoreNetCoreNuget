namespace BaseNetCore.Core.src.Main.Utils
{
    public static class LinqUtils
    {
        public static List<T> ToSingleList<T>(this T data)
        {
            return new List<T> { data };
        }
    }
}
