using PACommon;

namespace ProgressAdventureTests
{
    internal class VersionStringZipPathComparer : Comparer<string>
    {
        public override int Compare(string? zipPath1, string? zipPath2)
        {
            if (zipPath1 is null || zipPath2 is null || zipPath1 == zipPath2)
            {
                return 0;
            }
            var versionString1 = Path.GetFileNameWithoutExtension(zipPath1);
            var versionString2 = Path.GetFileNameWithoutExtension(zipPath2);
            return Utils.IsUpToDate(versionString2, versionString1) ? 1 : -1;
        }
    }
}
