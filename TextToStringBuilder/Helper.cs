namespace TextToStringBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class Helper
    {
        public static string CreateStringBuilderAppendLines(string data, string prefix = null, string suffix = null)
        {
            var sb = new StringBuilder();
            var delayedEmptyLines = new StringBuilder();

            prefix ??= "";
            suffix ??= "";

            foreach (string line in data.Split('\n'))
            {
                var newLine = Helper.ExpandTabs(line.TrimEnd().TrimEnd('\t'), 4);

                newLine = newLine.Replace("\\", "\\\\");
                newLine = newLine.Replace("\"", "\\\"");

                if (string.IsNullOrEmpty(newLine))
                {
                    if (sb.Length > 0)
                    {
                        delayedEmptyLines
                            .Append(prefix)
                            .Append(".AppendLine()")
                            .AppendLine(suffix);
                    }
                }
                else
                {
                    if (sb.Length > 0)
                    {
                        sb.AppendLine();
                    }

                    if (delayedEmptyLines.Length > 0)
                    {
                        sb.Append(delayedEmptyLines);
                        delayedEmptyLines = new StringBuilder();
                    }

                    sb.Append(prefix)
                        .Append(".AppendLine(\"")
                        .Append(newLine)
                        .Append("\")")
                        .Append(suffix);
                }
            }

            return sb.ToString();
        }

        public static string UnwrapAppends(string data)
        {
            const string startTag = ".Append(";
            const string startTagLine = ".AppendLine(";

            int tagLength = startTag.Length;
            int tagLengthLine = startTagLine.Length;

            var result = new StringBuilder();
            int cursor = 0;

            do
            {
                bool isLine = false;

                int next = data.IndexOf(startTag, cursor, StringComparison.InvariantCulture);
                int nextLine = data.IndexOf(startTagLine, cursor, StringComparison.InvariantCulture);

                if (next < 0 && nextLine < 0)
                {
                    break;
                }

                if (nextLine < next && nextLine >= 0 || nextLine >= 0 && next < 0)
                {
                    isLine = true;
                    next = nextLine;
                }

                cursor = next + (isLine ? tagLengthLine : tagLength);

                bool verbatim = false;

                if (IsFollowedBy(data, "\"", cursor))
                {
                    cursor++;
                }
                else if (IsFollowedBy(data, "$\"", cursor))
                {
                    cursor += 2;
                }
                else if (IsFollowedBy(data, "@\"", cursor))
                {
                    cursor += 2;
                    verbatim = true;
                }
                else if (IsFollowedBy(data, ")", cursor))
                {
                    cursor++;

                    if (isLine)
                    {
                        result.AppendLine();
                    }

                    continue;
                }
                else
                {
                    continue;
                }

                int? endPos = FindEndQuote(data, cursor, verbatim);

                if (endPos == null)
                {
                    break;
                }

                string line = data.Substring(cursor, (endPos ?? cursor + 1) - cursor);

                cursor = (endPos ?? cursor + 1) + 1;

                if (!verbatim)
                {
                    line = line.Replace("\\\\", "\\");
                }

                line = line.Replace("\\\"", "\"");

                result.Append(line);

                if (isLine)
                {
                    result.AppendLine();
                }
            }
            while (cursor < data.Length - 1);

            return result.ToString();
        }

        public static string ExpandTabs(string input, int tabLength)
        {
            string[] parts = input.Split('\t');
            int count = 0;
            int maxpart = parts.Count() - 1;

            foreach (string part in parts)
            {
                if (count < maxpart)
                    parts[count] = part + new string(' ', tabLength - (part.Length % tabLength));
                count++;
            }

            return (String.Join("", parts));
        }

        public static int? FindEndQuote(string data, int position, bool verbatim)
        {
            int? endPos = null;
            int i = position;


            while (data.Length >= i)
            {
                if (!verbatim && IsFollowedBy(data, "\\\"", i))
                {
                    i += 2;
                    continue;
                }

                if (!verbatim && IsFollowedBy(data, "\\\\", i))
                {
                    i += 2;
                    continue;
                }

                if (IsFollowedBy(data, "\"", i))
                {
                    endPos = i;
                    break;
                }

                i++;
            }

            return endPos;
        }

        public static bool IsFollowedBy(string data, string searchString, int position)
        {
            if (data.Length < position + searchString.Length - 1)
            {
                return false;
            }

            return data.IndexOf(searchString, position, StringComparison.InvariantCulture) == position;
        }
    }
}
