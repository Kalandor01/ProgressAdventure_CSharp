using System.Collections.ObjectModel;
using PACommon.JsonUtils;
using ProgressAdventure.ConfigManagement;
using ProgressAdventure.EntityManagement;

namespace ProgressAdventure
{
    public interface ISaveData<TSelf> : IJsonConvertable<TSelf>, IDisposable
        where TSelf : IJsonConvertable<TSelf>, ISaveData<TSelf>
    {
        #region Public properties
        /// <summary>
        /// The name of the save folder.
        /// </summary>
        public string SaveName { get; set; }
        /// <summary>
        /// The save name to display.
        /// </summary>
        public string DisplaySaveName { get; set; }
        /// <summary>
        /// The last time, the save file was saved.
        /// </summary>
        public DateTime LastSave { get; }
        /// <summary>
        /// The last time, the save file was loaded.
        /// </summary>
        public DateTime LastLoad { get; }
        /// <summary>
        /// The last time, the save file was saved.
        /// </summary>
        public TimeSpan Playtime { get; }

        /// <summary>
        /// A refrence to the player object.
        /// </summary>
        public Entity PlayerRef { get; }

        /// <summary>
        /// The list of the configs that were enabled the last time the save was saved to file.
        /// </summary>
        public ReadOnlyCollection<LoadedConfigData> LastLoadedConfigs { get; }
        #endregion

        #region Public methods
        /// <summary>
        /// Returns the total amount of playtime (the sum of the times between each save - each load).
        /// </summary>
        public TimeSpan GetPlaytime();
        #endregion
    }
}
