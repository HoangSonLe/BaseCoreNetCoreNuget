namespace BaseNetCore.Core.src.Main.Utils
{
    /// <summary>
    /// Provides utility methods for working with enum types.
    /// </summary>
    public static class EnumUtils
    {
        /// <summary>
        /// Gets the description attribute of an enum value, or its string representation if no description is found.
        /// </summary>
        /// <typeparam name="TEnum">The enum type.</typeparam>
        /// <param name="enumValue">The enum value.</param>
        /// <returns>The description of the enum value, or its string representation.</returns>
        public static String GetDescription<TEnum>(this TEnum enumValue) where TEnum : Enum
        {
            var enumType = typeof(TEnum);
            var memberInfo = enumType.GetMember(enumValue.ToString());
            if (memberInfo.Length > 0)
            {
                var attributes = memberInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
                if (attributes.Length > 0)
                {
                    return ((System.ComponentModel.DescriptionAttribute)attributes[0]).Description;
                }
            }
            return enumValue.ToString();
        }
    }
}
