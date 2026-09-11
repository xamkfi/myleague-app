using System.Text;

namespace JoomleagueImporter.Sql;

/// <summary>
/// Parsed rows of a single table from a MySQL dump: column names plus one string?[] per row
/// (NULL values are null, everything else is the raw/unescaped text).
/// </summary>
public class ParsedTable
{
    public List<string> Columns { get; } = [];
    public List<string?[]> Rows { get; } = [];

    public int ColumnIndex(string name)
    {
        if (TryColumnIndex(name, out int idx))
            return idx;
        throw new InvalidOperationException($"Column '{name}' not found. Available: {string.Join(", ", Columns)}");
    }

    public bool TryColumnIndex(string name, out int index)
    {
        index = Columns.FindIndex(c => string.Equals(c, name, StringComparison.OrdinalIgnoreCase));
        return index >= 0;
    }
}

/// <summary>
/// Minimal MySQL dump parser: extracts the tuples of `INSERT INTO `table` (cols) VALUES (...),(...);`
/// statements for a requested set of tables. Handles quoted strings with backslash escapes and
/// doubled quotes, NULL literals and numeric values. No MySQL installation required.
/// </summary>
public static class SqlDumpParser
{
    public static Dictionary<string, ParsedTable> Parse(string dumpFilePath, IReadOnlyCollection<string> tableNames)
    {
        HashSet<string> wanted = new(tableNames, StringComparer.OrdinalIgnoreCase);
        Dictionary<string, ParsedTable> result = new(StringComparer.OrdinalIgnoreCase);
        foreach (string t in wanted)
            result[t] = new ParsedTable();

        string sql = File.ReadAllText(dumpFilePath, Encoding.UTF8);
        int pos = 0;

        const string insertMarker = "INSERT INTO `";
        while (true)
        {
            int idx = sql.IndexOf(insertMarker, pos, StringComparison.Ordinal);
            if (idx < 0) break;

            pos = idx + insertMarker.Length;
            int nameEnd = sql.IndexOf('`', pos);
            if (nameEnd < 0) break;

            string tableName = sql[pos..nameEnd];
            pos = nameEnd + 1;

            if (!wanted.Contains(tableName))
                continue; // next IndexOf will skip past this statement's body safely enough

            ParsedTable table = result[tableName];

            // Column list: `col1`, `col2`, ...
            pos = sql.IndexOf('(', pos);
            if (pos < 0) break;
            pos++;

            bool firstInsertForTable = table.Columns.Count == 0;
            List<string> statementColumns = [];
            while (true)
            {
                int colStart = sql.IndexOf('`', pos);
                int listEnd = sql.IndexOf(')', pos);
                if (colStart < 0 || (listEnd >= 0 && listEnd < colStart))
                {
                    pos = listEnd + 1;
                    break;
                }
                int colEnd = sql.IndexOf('`', colStart + 1);
                statementColumns.Add(sql[(colStart + 1)..colEnd]);
                pos = colEnd + 1;
            }
            if (firstInsertForTable)
                table.Columns.AddRange(statementColumns);

            int valuesIdx = sql.IndexOf("VALUES", pos, StringComparison.OrdinalIgnoreCase);
            if (valuesIdx < 0) break;
            pos = valuesIdx + "VALUES".Length;

            // Tuples: (v1, v2, ...), (...) ... ;
            while (true)
            {
                SkipWhitespace(sql, ref pos);
                if (pos >= sql.Length) break;
                if (sql[pos] == ';') { pos++; break; }
                if (sql[pos] == ',') { pos++; continue; }
                if (sql[pos] != '(') break; // unexpected; bail out of this statement

                pos++; // consume '('
                string?[] row = ParseTuple(sql, ref pos, statementColumns.Count);
                table.Rows.Add(row);
            }
        }

        return result;
    }

    private static string?[] ParseTuple(string sql, ref int pos, int expectedCount)
    {
        List<string?> values = new(expectedCount);

        while (true)
        {
            SkipWhitespace(sql, ref pos);
            char c = sql[pos];

            if (c == ')')
            {
                pos++;
                break;
            }
            if (c == ',')
            {
                pos++;
                continue;
            }

            if (c == '\'')
            {
                pos++;
                values.Add(ParseQuotedString(sql, ref pos));
            }
            else
            {
                int start = pos;
                while (pos < sql.Length && sql[pos] != ',' && sql[pos] != ')')
                    pos++;
                string token = sql[start..pos].Trim();
                values.Add(string.Equals(token, "NULL", StringComparison.OrdinalIgnoreCase) ? null : token);
            }
        }

        return values.ToArray();
    }

    private static string ParseQuotedString(string sql, ref int pos)
    {
        StringBuilder sb = new();
        while (pos < sql.Length)
        {
            char c = sql[pos];

            if (c == '\\' && pos + 1 < sql.Length)
            {
                char next = sql[pos + 1];
                sb.Append(next switch
                {
                    'n' => '\n',
                    'r' => '\r',
                    't' => '\t',
                    '0' => '\0',
                    'Z' => '\u001a',
                    _ => next, // \' \" \\ and anything else -> literal char
                });
                pos += 2;
                continue;
            }

            if (c == '\'')
            {
                // Doubled quote ('') is an escaped single quote inside the string.
                if (pos + 1 < sql.Length && sql[pos + 1] == '\'')
                {
                    sb.Append('\'');
                    pos += 2;
                    continue;
                }
                pos++;
                break;
            }

            sb.Append(c);
            pos++;
        }
        return sb.ToString();
    }

    private static void SkipWhitespace(string sql, ref int pos)
    {
        while (pos < sql.Length && char.IsWhiteSpace(sql[pos]))
            pos++;
    }
}
