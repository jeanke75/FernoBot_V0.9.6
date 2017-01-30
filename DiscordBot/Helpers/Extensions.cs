using System.Data;

namespace DiscordBot.Helpers
{
    static class Extensions 
    {
        public static T GetValueSafe<T>(this IDataReader reader, int colIndex)
        {
            return GetValueSafe(reader, colIndex, default(T));
        }

        public static T GetValueSafe<T>(this IDataReader reader, int colIndex, T defaultValue)
        {
            if (!reader.IsDBNull(colIndex))
                return (T)reader.GetValue(colIndex);
            else
                return defaultValue;
        }

        public static T GetValueSafe<T>(this IDataReader reader, string columnName, T defaultValue)
        {
            return GetValueSafe(reader, reader.GetOrdinal(columnName), defaultValue);
        }
    }
}
