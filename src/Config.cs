using System.Text;
using System.Text.Json;

namespace Todo
{
    public class Config
    {
        /// <summary>
        /// Internal storage.
        /// </summary>
        private static Dictionary<string, string> Storage = new();

        /// <summary>
        /// Get value from storage.
        /// </summary>
        /// <param name="key">Key to get value from.</param>
        /// <param name="defaultValue">Default value, if key is not found.</param>
        /// <returns>Value.</returns>
        public static string? Get(string key, string? defaultValue = null)
        {
            if (Storage.ContainsKey(key))
            {
                return Storage[key];
            }

            if (defaultValue == null)
            {
                return null;
            }

            Set(key, defaultValue);
            Save();

            return defaultValue;
        }

        /// <summary>
        /// Load config from disk.
        /// </summary>
        /// <returns>Success.</returns>
        public static bool Load()
        {
            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "config.json");

            if (!File.Exists(path))
            {
                return true;
            }

            try
            {
                Storage = JsonSerializer.Deserialize<Dictionary<string, string>>(
                    File.ReadAllText(path, Encoding.UTF8));

                return true;
            }
            catch (Exception ex)
            {
                ConsoleEx.WriteException(ex);
                return false;
            }
        }

        /// <summary>
        /// Save config to disk.
        /// </summary>
        public static void Save()
        {
            var path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "config.json");

            try
            {
                File.WriteAllText(
                    path,
                    JsonSerializer.Serialize(Storage),
                    Encoding.UTF8);
            }
            catch (Exception ex)
            {
                ConsoleEx.WriteException(ex);
            }
        }

        /// <summary>
        /// Add value to storage.
        /// </summary>
        /// <param name="key">Key to add.</param>
        /// <param name="value">Value for key.</param>
        public static void Set(string key, string value)
        {
            if (Storage.ContainsKey(key))
            {
                Storage[key] = value;
            }
            else
            {
                Storage.Add(key, value);
            }
        }
    }
}