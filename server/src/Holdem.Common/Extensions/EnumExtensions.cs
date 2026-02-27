using System;

namespace Holdem.Common.Extensions
{
    public static class EnumExtensions
    {
        public static TEnum Next<TEnum>(this TEnum current)
            where TEnum : struct, Enum
        {
            var values = (TEnum[])Enum.GetValues(typeof(TEnum));
            var index = Array.IndexOf(values, current);
            var next = (index + 1) % values.Length;
            return values[next];
        }
    }
}
