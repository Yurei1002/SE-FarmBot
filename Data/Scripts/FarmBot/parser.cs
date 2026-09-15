using System;
using System.Collections.Generic;
using System.Text;

namespace FarmBot
{
    public static class Parser
    {
        public static Dictionary<string, string> JsonParser(string json)
        {
            Dictionary<string, string> result =
                new Dictionary<string, string>();

            if (string.IsNullOrEmpty(json))
                return result;

            int position = 0;

            while (position < json.Length)
            {
                SkipWhitespace(json, ref position);

                if (position >= json.Length)
                    break;

                if (json[position] == '{' || json[position] == ',')
                {
                    position++;
                    continue;
                }

                if (json[position] == '}')
                    break;

                string key = ReadString(json, ref position);

                if (key == null)
                    break;

                SkipWhitespace(json, ref position);

                if (position >= json.Length || json[position] != ':')
                    break;

                position++;

                SkipWhitespace(json, ref position);

                string value = ReadString(json, ref position);

                if (value == null)
                    break;

                result[key] = value;
            }

            return result;
        }

        private static string ReadString(string json, ref int position)
        {
            SkipWhitespace(json, ref position);

            if (position >= json.Length || json[position] != '"')
                return null;

            position++;

            StringBuilder result =
                new StringBuilder();

            while (position < json.Length)
            {
                char character = json[position++];

                if (character == '"')
                    return result.ToString();

                if (character != '\\')
                {
                    result.Append(character);
                    continue;
                }

                if (position >= json.Length)
                    return null;

                character = json[position++];

                switch (character)
                {
                    case '"': result.Append('"');
                        break;
                    case '\\': result.Append('\\');
                        break;
                    case '/': result.Append('/');
                        break;
                    case 'n': result.Append('\n');
                        break;
                    case 'r': result.Append('\r');
                        break;
                    case 't': result.Append('\t');
                        break;
                    case 'b': result.Append('\b');
                        break;
                    case 'f': result.Append('\f');
                        break;
                    default: result.Append(character);
                        break;
                }
            }
            return null;
        }

        private static void SkipWhitespace(string json, ref int position)
        {
            while (position < json.Length && char.IsWhiteSpace(json[position]))
                position++;
        }
    }
}