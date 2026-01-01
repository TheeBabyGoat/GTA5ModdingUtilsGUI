using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace GTA5ModdingUtilsGUI
{
    internal static class YtypArchetypeScanner
    {
        // Matches archetype names inside: <Item type="CBaseArchetypeDef"> ... <name>foo</name>
        // This intentionally ignores other <name> nodes (e.g., the YTYP name itself).
        private static readonly Regex ArchetypeRegex = new Regex(
            "<Item\\s+type=\\\"CBaseArchetypeDef\\\">[\\s\\S]*?<name>([^<]+)</name>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static List<string> LoadArchetypeNamesFromYtypDirectory(string ytypDirectory)
        {
            var results = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(ytypDirectory) || !Directory.Exists(ytypDirectory))
                return new List<string>();

            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(ytypDirectory, "*.ytyp.xml", SearchOption.AllDirectories);
            }
            catch
            {
                // Some directories might be inaccessible; fall back to non-recursive.
                files = Directory.EnumerateFiles(ytypDirectory, "*.ytyp.xml", SearchOption.TopDirectoryOnly);
            }

            foreach (var file in files)
            {
                try
                {
                    var info = new FileInfo(file);
                    if (info.Exists && info.Length == 0)
                        continue;

                    string xml = File.ReadAllText(file);

                    foreach (Match m in ArchetypeRegex.Matches(xml))
                    {
                        if (!m.Success || m.Groups.Count < 2)
                            continue;

                        var name = (m.Groups[1].Value ?? string.Empty).Trim();
                        if (name.Length == 0)
                            continue;

                        // The Python YtypParser lowercases names; keep the GUI consistent.
                        results.Add(name.ToLowerInvariant());
                    }
                }
                catch
                {
                    // Ignore a single bad file; continue scanning.
                }
            }

            return results.OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}
