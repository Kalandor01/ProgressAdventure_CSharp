using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;

namespace ProgressAdventure.Entity
{
    public class Entity<TEntity> : Entity, IJsonConvertable<TEntity>
        where TEntity : Entity<TEntity>
    {
        public Entity(
            (
                string? name,
                int? baseMaxHp,
                int? currentHp,
                int? baseAttack,
                int? baseDefence,
                int? baseSpeed,
                int? originalTeam,
                int? currentTeam,
                List<Enums.Attribute>? attributes,
                List<Item>? drops,
                (long x, long y)? position,
                Facing? facing
            ) entityData,
            IDictionary<string, object?>? miscData
        ) : base(entityData, miscData) { }

        public Entity(
            string name,
            (
                int baseMaxHp,
                int baseAttack,
                int baseDefence,
                int baseSpeed,
                int originalTeam,
                int? team,
                List<Enums.Attribute>? attributes
            ) stats,
            List<Item>? drops = null
        ) : base(name, stats, drops) { }

        public static TEntity? FromJson(IDictionary<string, object?>? entityJson)
        {
            return FromJson<TEntity>(entityJson);
        }
    }
}
