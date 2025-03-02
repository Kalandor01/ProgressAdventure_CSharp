using PACommon.Enums;
using ProgressAdventure.EntityManagement;
using ProgressAdventure.Enums;

namespace ProgressAdventure.WorldManagement
{
    public class PopulationManager
    {
        private Dictionary<EnumValue<EntityType>, int> unloadedEntities;
        private Dictionary<EnumValue<EntityType>, List<Entity>> loadedEntities;

        public PopulationManager()
        {

        }
    }
}
