namespace BaseNetCore.Core.src.Main.Security.Algorithm
{
    public static class PasswordEncoder
    {
        private const int WorkFactor = 10;

        public static string Encode(string rawPassword)
        {
            return BCrypt.Net.BCrypt.HashPassword(rawPassword, WorkFactor);
        }

        public static bool Verify(string rawPassword, string encodedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(rawPassword, encodedPassword);
        }
    }
}
