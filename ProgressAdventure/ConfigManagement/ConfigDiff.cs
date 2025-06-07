using System.Text;

namespace ProgressAdventure.ConfigManagement
{
    public struct ConfigDiff
    {
        public ConfigData[] added;
        public LoadedConfigData[] removed;
        public (ConfigData config, string oldVersion)[] versionChanged;
        public (ConfigData config, int oldIndex, int newIndex)[] orderChanged;

        public readonly bool IsEmpty =>
            added.Length == 0 &&
            removed.Length == 0 &&
            versionChanged.Length == 0 &&
            orderChanged.Length == 0;

        public override readonly string? ToString()
        {
            var txt = new StringBuilder();
            if (added.Length > 0)
            {
                txt.Append("added:\n\t-");
                txt.Append(string.Join("\n\t-", added.Select(a => a.Namespace)));
                txt.Append('\n');
            }
            if (removed.Length > 0)
            {
                txt.Append("removed:\n\t-");
                txt.Append(string.Join("\n\t-", removed.Select(r => r.Namespace)));
                txt.Append('\n');
            }
            if (versionChanged.Length > 0)
            {
                txt.Append("version changed:\n\t-");
                txt.Append(string.Join("\n\t-", versionChanged.Select(v => $"{v.config.Namespace}: {v.oldVersion} -> {v.config.Version}")));
                txt.Append('\n');
            }
            if (orderChanged.Length > 0)
            {
                txt.Append("order changed:\n\t-");
                txt.Append(string.Join("\n\t-", orderChanged.Select(o => $"{o.config.Namespace}: {o.oldIndex + 1} -> {o.newIndex + 1}")));
                txt.Append('\n');
            }
            return txt.ToString();
        }
    }
}
