using System;

namespace Common.Extensions
{
    public static class EnumExtensions
    {
        public static TEnum Next<TEnum>(this TEnum current)
            where TEnum : struct, Enum
        {
            var values = Enum.GetValues<TEnum>();
            var index = Array.IndexOf(values, current);
            var next = (index + 1) % values.Length;
            return values[next];
        }
    }
}
