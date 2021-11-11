using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Christmas_bot
{
    internal static class AuthHandle
    {
        public static bool TryGetToken(out string token)
        {
            var path = PathHandle.GetConfigPath();

            var jr = new Utf8JsonReader(File.ReadAllBytes(path), new JsonReaderOptions{ AllowTrailingCommas = true });

            if (JsonDocument.TryParseValue(ref jr, out var json) &&
                json.RootElement.TryGetProperty("token", out var element))
            {
                token = element.GetString();
                return true;
            }
            else
            {
                token = string.Empty;
                return false;
            }
        }
    }
}
