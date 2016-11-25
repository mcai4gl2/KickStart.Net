namespace KickStart.Net.Extensions
{
    public static class ArrayExtensions
    {
        public static T[] Fill<T>(this T[] array, T valueToFill, int fromIndex = 0)
        {
            for (var index = fromIndex; index < array.Length; index++)
                array[index] = valueToFill;
            return array;
        }

        public static void SetValue<T>(this T[] array, int index, T value, bool fromStart = true)
        {
            if (fromStart)
                array[index] = value;
            else
                array[array.Length - 1 - index] = value;
        }
    }
}
