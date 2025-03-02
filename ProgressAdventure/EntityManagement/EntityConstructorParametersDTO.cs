using PACommon.Enums;
using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;

namespace ProgressAdventure.EntityManagement
{
    public struct EntityConstructorParametersDTO
    {
        public EnumValue<EntityType> type;
        public string name;
        public int baseMaxHp;
        public int currentHp;
        public int baseAttack;
        public int baseDefence;
        public int baseAgility;
        public int originalTeam;
        public int currentTeam;
        public List<EnumValue<Enums.Attribute>> attributes;
        public List<AItem> drops;
        public (long x, long y) position;
        public Facing facing;
    }
}
