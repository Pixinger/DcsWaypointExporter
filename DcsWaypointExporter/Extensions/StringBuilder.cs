// Copyright© 2024 / pixinger@github / MIT License https://choosealicense.com/licenses/mit/

using System.Text;

namespace DcsWaypointExporter.Extensions
{
    public static class StringBuilderExtensions
    {
        public const string INDENT_CHARACTERS = "\t";

        public static void AppendIndent(this StringBuilder stringBuilder, int count = 1, string indentCharacters = INDENT_CHARACTERS)
        {
            for (var i = 0; i < count; i++)
            {
                stringBuilder.Append(indentCharacters);
            }
        }
    }
}
