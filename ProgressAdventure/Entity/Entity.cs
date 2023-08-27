using ProgressAdventure.Enums;
using ProgressAdventure.Extensions;
using ProgressAdventure.ItemManagement;
using ProgressAdventure.WorldManagement;
using System.Collections;
using System.Reflection;
using System.Text;
using Attribute = ProgressAdventure.Enums.Attribute;

namespace ProgressAdventure.Entity
{
    /// <summary>
    /// A representation of an entity.<br/>
    /// Classes implementing this class MUST create a (protected) constructor, with signiture protected Type([return type from "FromJsonInternal()"] entityData, IDictionary<string, object?>? miscData) for FromJson<T>() to work.
    /// </summary>
    public abstract class Entity : IJsonReadable
    {
        #region Public fields
        /// <summary>
        /// The name of the <c>Entity</c>.
        /// </summary>
        public string name;
        /// <summary>
        /// The base hp of the <c>Entity</c>.
        /// </summary>
        public int baseMaxHp;
        /// <summary>
        /// The base attack of the <c>Entity</c>.
        /// </summary>
        public int baseAttack;
        /// <summary>
        /// The base defence of the <c>Entity</c>.
        /// </summary>
        public int baseDefence;
        /// <summary>
        /// The base agility of the <c>Entity</c>.
        /// </summary>
        public int baseAgility;
        /// <summary>
        /// The original team that the <c>Entity</c> is a part of.
        /// </summary>
        public int originalTeam;
        /// <summary>
        /// The current team that the <c>Entity</c> is a part of.
        /// </summary>
        public int currentTeam;
        /// <summary>
        /// The list of attributes that the <c>Entity</c> has.
        /// </summary>
        public List<Attribute> attributes;
        /// <summary>
        /// The list of items that the <c>Entity</c> will drop on death.
        /// </summary>
        public List<AItem> drops;
        /// <summary>
        /// The position of the entity in the world.
        /// </summary>
        public (long x, long y) position;
        /// <summary>
        /// The facing direction of the entity.
        /// </summary>
        public Facing facing;
        #endregion

        #region Private fields
        /// <summary>
        /// The maximum hp of the <c>Entity</c>.
        /// </summary>
        private int _maxHp;
        /// <summary>
        /// The current hp of the <c>Entity</c>.
        /// </summary>
        private int _currentHp;
        #endregion

        #region Public properties
        /// <summary>
        /// The full name of the <c>Entity</c>.
        /// </summary>
        public string FullName { get; protected set; }
        /// <summary>
        /// <inheritdoc cref="_maxHp" path="//summary"/>
        /// </summary>
        public int MaxHp
        {
            get
            {
                return _maxHp;
            }
            protected set
            {
                _maxHp = value >= 0 ? value : 0;
            }
        }
        /// <summary>
        /// <inheritdoc cref="_currentHp" path="//summary"/>
        /// </summary>
        public int CurrentHp
        {
            get
            {
                return _currentHp;
            }
            protected set
            {
                _currentHp = Math.Clamp(value, 0, MaxHp);
            }
        }
        /// <summary>
        /// The current attack of the <c>Entity</c>.
        /// </summary>
        public int Attack { get; protected set; }
        /// <summary>
        /// The current defence of the <c>Entity</c>.
        /// </summary>
        public int Defence { get; protected set; }
        /// <summary>
        /// The current agility of the <c>Entity</c>.
        /// </summary>
        public int Agility { get; protected set; }
        #endregion

        #region Public constructors
        /// <summary>
        /// <inheritdoc cref="Entity"/><br/>
        /// Can be used for creating a new <c>Entity</c>, from the result of the EntityManager function.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        /// <param name="stats">The tuple of stats, representin all other values from the other constructor, other than drops.</param>
        /// <param name="drops"><inheritdoc cref="drops" path="//summary"/></param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        public Entity(
            string name,
            EntityManagerStatsDTO stats,
            List<AItem>? drops = null,
            string fileVersion = Constants.SAVE_VERSION
        )
            :this(
                (
                    name,
                    stats.baseMaxHp,
                    null,
                    stats.baseAttack,
                    stats.baseDefence,
                    stats.baseAgility,
                    stats.originalTeam,
                    stats.currentTeam,
                    stats.attributes,
                    drops,
                    null,
                    null
                ),
                null,
                fileVersion
            ) { }
        #endregion

        #region Public methods
        /// <summary>
        /// Updates the full name of the entity.
        /// </summary>
        public void UpdateFullName()
        {
            var attributeNames = new StringBuilder();
            foreach (var attribute in attributes)
            {
                attributeNames.Append(attribute.ToString() + " ");
            }
            FullName = (attributeNames.ToString() + name).Capitalize();
        }

        /// <summary>
        /// Turns the entity in a random direction, that is weighted in the direction that it's already going towards.
        /// </summary>
        public void WeightedTurn()
        {
            if (RandomStates.MainRandom.GenerateDouble() < 0.2)
            {
                var oldFacing = facing;
                var angle = RandomStates.MainRandom.Triangular(-180, 0, 180);
                var newFacing = EntityUtils.RotateFacing(oldFacing, angle);
                
                if (newFacing is not null && newFacing != oldFacing)
                {
                    facing = (Facing)newFacing;
                    Logger.Log("Entity turned", $"name: {FullName}, {oldFacing} -> {facing}", LogSeverity.DEBUG);
                }
            }
        }

        /// <summary>
        /// Moves the entity in the direction it's facing.
        /// </summary>
        /// <param name="multiplierVector">The multiplier to move the entity by.</param>
        /// <param name="facing">If not null, it will move in that direction instead.</param>
        public void Move((double x, double y)? multiplierVector = null, Facing? facing = null)
        {
            var moveRaw = EntityUtils.facingToMovementVectorMap[facing ?? this.facing];
            var move = Utils.VectorMultiply(moveRaw, multiplierVector ?? (1, 1));
            EntityUtils.EntityMover(this, ((long x, long y))move);
        }

        /// <summary>
        /// Modifies the position of the entity.
        /// </summary>
        /// <param name="position">The position to move to.</param>
        public virtual void SetPosition((long x, long y) position)
        {
            SetPosition(position, false);
        }

        /// <summary>
        /// Modifies the position of the entity.
        /// </summary>
        /// <param name="position">The position to move to.</param>
        /// <param name="updateWorld">Whether to update the tile, the entitiy is on.</param>
        public virtual void SetPosition((long x, long y) position, bool updateWorld)
        {
            var oldPosition = this.position;
            this.position = position;
            Logger.Log("Entity moved", $"name: {FullName}, {oldPosition} -> {this.position}", LogSeverity.DEBUG);
            if (updateWorld)
            {
                World.TryGetTileAll(this.position, out _);
            }
        }

        /// <summary>
        /// Makes the entity attack another one.
        /// </summary>
        /// <param name="target">The target entity.</param>
        public AttackResponse AttackEntity(Entity target)
        {
            Logger.Log("Attack log", $"{FullName} attacked {target.FullName}");
            // attacker dead
            if (CurrentHp == 0)
            {
                Logger.Log("Attack log", $"{FullName}(attacker) is dead");
                return AttackResponse.ENTITY_DEAD;
            }
            // enemy dead
            else if (target.CurrentHp == 0)
            {
                Logger.Log("Attack log", $"{FullName}(attacked) is already dead");
                return AttackResponse.TARGET_DEAD;
            }
            // enemy dodge
            else if (RandomStates.MiscRandom.GenerateDouble() > Agility * 1.0 / target.Agility - 0.1)
            {
                Logger.Log("Attack log", $"{target.FullName} dodged");
                return AttackResponse.TARGET_DOGDED;
            }            
            // attack
            else
            {
                var attack = (int)RandomStates.MiscRandom.GenerateInRange(1, 7) + Attack;
                var damage = attack - target.Defence;
                // block
                if (damage <= 0)
                {
                    Logger.Log("Attack log", $"{target.FullName} blocked attack");
                    return AttackResponse.TARGET_BLOCKED;
                }
                // hit
                else
                {
                    target.TakeDamage(damage);
                    Logger.Log("Attack log", $"{target.FullName} took {damage} damage ({target.CurrentHp})");
                    // kill
                    if (target.CurrentHp == 0)
                    {
                        Logger.Log("Attack log", $"{FullName} defeated {target.FullName}");
                        return AttackResponse.TARGET_KILLED;
                    }
                    return AttackResponse.TARGET_HIT;
                }
            }
        }

        /// <summary>
        /// Makes the entity take damage.
        /// </summary>
        /// <param name="damage">The amount of damage to take.</param>
        public void TakeDamage(int damage)
        {
            CurrentHp -= damage;
        }

        /// <summary>
        /// Returns the full name of the entity with their species in parentheses, unless it's a player.
        /// </summary>
        public string GetFullNameWithSpecies()
        {
            return FullName + (GetType() != typeof(Player) ? $" ({EntityUtils.GetEntityTypeName(this)})" : "");
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Sets up the entity's stats acording to its base attributes.
        /// </summary>
        private void SetupAttributes(int? currentHp = null)
        {
            double tempMaxHp = baseMaxHp;
            double tempAttack = baseAttack;
            double tempDefence = baseDefence;
            double tempAgility = baseAgility;

            foreach (var attribute in attributes)
            {
                var (maxHp, attack, defence, agility) = EntityUtils.attributeStatChangeMap[attribute];
                tempMaxHp *= maxHp;
                tempAttack *= attack;
                tempDefence *= defence;
                tempAgility *= agility;
            }
            MaxHp = (int)Math.Clamp(tempMaxHp, int.MinValue, int.MaxValue);
            CurrentHp = currentHp ?? MaxHp;
            Attack = (int)Math.Clamp(tempAttack, int.MinValue, int.MaxValue);
            Defence = (int)Math.Clamp(tempDefence, int.MinValue, int.MaxValue);
            Agility = (int)Math.Clamp(tempAgility, int.MinValue, int.MaxValue);
        }
        #endregion

        #region Public overrides
        public override string ToString()
        {
            var originalTeamStr = originalTeam == 0 ? "Player" : originalTeam.ToString();
            var teamStr = currentTeam == 0 ? "Player" : currentTeam.ToString();
            var typeLine = GetType() != typeof(Player) ? $"\nSpecies: {EntityUtils.GetEntityTypeName(this)}" : "";
            return $"Name: {name}{typeLine}\nFull name: {FullName}\nHp: {MaxHp}\nAttack: {Attack}\nDefence: {Defence}\nAgility: {Agility}\nAttributes: {string.Join(", ", attributes)}\nOriginal team: {originalTeamStr}\nCurrent team: {teamStr}\nDrops: {string.Join("\n, ", drops.Select(d => $"\t{d}"))}";
        }
        #endregion

        #region JsonConvert
        #region Constructors
        /// <summary>
        /// <inheritdoc cref="Entity"/><br/>
        /// Can be used for loading the <c>Entity</c> from json.
        /// </summary>
        /// <param name="entityData">The entity data, from <c>FromJsonInternal</c>.</param>
        /// <param name="miscData">The json data, that can be used for loading extra data, specific to an entity type.<br/>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// Should only be null, if entity creation called this constructor.</param>
        protected Entity((
            string? name,
            int? baseMaxHp,
            int? currentHp,
            int? baseAttack,
            int? baseDefence,
            int? baseAgility,
            int? originalTeam,
            int? currentTeam,
            List<Attribute>? attributes,
            List<AItem>? drops,
            (long x, long y)? position,
            Facing? facing
        ) entityData, IDictionary<string, object?>? miscData, string fileVersion)
        {
            name = entityData.name ?? GetDefaultName();
            position = entityData.position ?? (0, 0);
            facing = entityData.facing ?? Facing.NORTH;
            if (
                entityData.baseMaxHp is null ||
                entityData.baseAttack is null ||
                entityData.baseDefence is null ||
                entityData.baseAgility is null ||
                entityData.originalTeam is null ||
                entityData.currentTeam is null ||
                entityData.attributes is null
            )
            {
                var ems = GetBaseStats();
                baseMaxHp = entityData.baseMaxHp ?? ems.baseMaxHp;
                baseAttack = entityData.baseAttack ?? ems.baseAttack;
                baseDefence = entityData.baseDefence ?? ems.baseDefence;
                baseAgility = entityData.baseAgility ?? ems.baseAgility;
                originalTeam = entityData.originalTeam ?? ems.originalTeam;
                currentTeam = entityData.currentTeam ?? ems.currentTeam;
                attributes = entityData.attributes ?? ems.attributes;
            }
            else
            {
                baseMaxHp = (int)entityData.baseMaxHp;
                baseAttack = (int)entityData.baseAttack;
                baseDefence = (int)entityData.baseDefence;
                baseAgility = (int)entityData.baseAgility;
                originalTeam = (int)entityData.originalTeam;
                currentTeam = (int)entityData.currentTeam;
                attributes = entityData.attributes;
            }
            drops = entityData.drops ?? GetDefaultDrops();
            // not new entity call
            if (miscData is not null)
            {
                FromMiscJson(miscData, fileVersion);
            }
            // adjust properties
            SetupAttributes(entityData.currentHp);
            UpdateFullName();
        }
        #endregion

        #region Methods
        public virtual Dictionary<string, object?> ToJson()
        {
            // attributes
            var attributesProcessed = attributes.Select(a => a.ToString()).ToList();
            // drops
            var dropsJson = drops.Select(drop => drop.ToJson()).ToList();
            // properties
            return new Dictionary<string, object?>
            {
                ["type"] = EntityUtils.GetEntityTypeName(this),
                ["name"] = name,
                ["base_max_hp"] = baseMaxHp,
                ["current_hp"] = CurrentHp,
                ["base_attack"] = baseAttack,
                ["base_defence"] = baseDefence,
                ["base_agility"] = baseAgility,
                ["original_team"] = originalTeam,
                ["current_team"] = currentTeam,
                ["attributes"] = attributesProcessed,
                ["drops"] = dropsJson,
                ["x_position"] = position.x,
                ["y_position"] = position.y,
                ["facing"] = (int)facing,
            };
        }

        /// <summary>
        /// Converts the misc data of the entity from a json version.
        /// </summary>
        /// <param name="miscJson">The json representation of the misc data for the specific entity type.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        protected virtual void FromMiscJson(IDictionary<string, object?> miscJson, string fileVersion) { }
        #endregion

        #region Functions
        /// <summary>
        /// Tries to convert the json representation of the entity to a specific entity object, and returns if it was succesful without any warnings.
        /// </summary>
        /// <typeparam name="T">The type of the entity to try to convert into.</typeparam>
        /// <param name="entityJson">The json representation of the entity.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="entityObject">The converted entity object.</param>
        /// <exception cref="ArgumentNullException">Thrown if the entity type couldn't be converted from json with the required constructor.</exception>
        public static bool FromJson<T>(IDictionary<string, object?>? entityJson, string fileVersion, out T? entityObject)
            where T : Entity<T>
        {
            var success = AnyEntityFromJsonPrivate(typeof(T), entityJson, fileVersion, out Entity? entity);
            entityObject = (T?)entity;
            return success;
        }

        /// <summary>
        /// Tries to convert any entity json, from a json format, into an entity object (if it implements the nececary protected constructor), and returns if it was succesful without any warnings.
        /// </summary>
        /// <param name="entityJson">The json representation of an entity.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="entityObject">The converted entity object.</param>
        public static bool AnyEntityFromJson(IDictionary<string, object?>? entityJson, string fileVersion, out Entity? entityObject)
        {
            if (entityJson is null)
            {
                Logger.Log("Entity parse error", "entity json is null", LogSeverity.ERROR);
                entityObject = null;
                return false;
            }

            if (
                entityJson.TryGetValue("type", out object? entityTypeValue) &&
                entityTypeValue is not null
            )
            {
                if (
                    EntityUtils.entityTypeMap.TryGetValue(entityTypeValue.ToString() ?? "", out Type? entityType) &&
                    entityType is not null
                )
                {
                    return AnyEntityFromJsonPrivate(entityType, entityJson, fileVersion, out entityObject);
                }
                else
                {
                    Logger.Log("Entity parse error", "invalid entity type", LogSeverity.ERROR);
                    entityObject = null;
                    return false;
                }
            }
            else
            {
                Logger.Log("Entity parse error", "entity type json is null", LogSeverity.ERROR);
                entityObject = null;
                return false;
            }
        }

        /// <summary>
        /// Returns a newly generated name of this entity, specific to this entity type.
        /// </summary>
        public static string GetDefaultName()
        {
            return EntityUtils.GetEntityNameFromClass(1);
        }

        /// <summary>
        /// Returns the newly rolled stats, specific to this entity type.
        /// </summary>
        public static EntityManagerStatsDTO GetBaseStats()
        {
            return EntityUtils.EntityManager(5, 5, 5, 5);
        }

        /// <summary>
        /// Returns the newly generated drops, specific to this entity type.
        /// </summary>
        public static List<AItem> GetDefaultDrops()
        {
            return new List<AItem> { ItemUtils.CreateCompoumdItem(ItemType.Misc.COIN, Material.COPPER) };
        }

        /// <summary>
        /// Tries to convert the json representation of the entity to a specific entity object, and returns if it was succesful without any warnings.
        /// </summary>
        /// <param name="entityType">The type of the entity to try to convert into.</param>
        /// <param name="entityJson">The json representation of the entity.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="entityObject">The converted entity object.</param>
        /// <exception cref="ArgumentNullException">Thrown if the entity type couldn't be converted from json with the required constructor.</exception>
        private static bool AnyEntityFromJsonPrivate(Type entityType, IDictionary<string, object?>? entityJson, string fileVersion, out Entity? entityObject)
        {
            var success = FromJsonPrivate(entityJson, fileVersion, out (
                string? name,
                int? baseMaxHp,
                int? currentHp,
                int? baseAttack,
                int? baseDefence,
                int? baseAgility,
                int? originalTeam,
                int? currentTeam,
                List<Attribute>? attributes,
                List<AItem>? drops,
                (long x, long y)? position,
                Facing? facing
            )? entityData);

            if (entityData is null || entityJson is null)
            {
                entityObject = null;
                return false;
            }

            // get entity
            var constructor = entityType.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.CreateInstance | BindingFlags.Instance,
                null,
                new[] { entityData.GetType(), entityJson.GetType(), fileVersion.GetType() },
                null
            ) ?? throw new ArgumentNullException(message: $"Couldn't find required entity constructor for type \"{entityType}\"!", null);
            var entity = constructor.Invoke(new object[] { entityData, entityJson, fileVersion }) ?? throw new ArgumentNullException(message: $"Couldn't create entity object from type \"{entityType}\"!", null);
            entityObject = (Entity?)entity;
            return success && entityObject is not null;
        }

        /// <summary>
        /// Tries to convert the json representation of the <c>Entity</c> to a format that can easily be turned to an <c>Entity</c> object, and returns if it was succesful without any warnings.
        /// </summary>
        /// <param name="entityJson">The json representation of the <c>Entity</c>.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="entityData">The basic data of the entity.</param>
        private static bool FromJsonPrivate(IDictionary<string, object?>? entityJson, string fileVersion, out (
            string? name,
            int? baseMaxHp,
            int? currentHp,
            int? baseAttack,
            int? baseDefence,
            int? baseAgility,
            int? originalTeam,
            int? currentTeam,
            List<Attribute>? attributes,
            List<AItem>? drops,
            (long x, long y)? position,
            Facing? facing
        )? entityData)
        {
            if (entityJson is null)
            {
                Logger.Log("Entity parse error", "entity json is null", LogSeverity.ERROR);
                entityData = null;
                return false;
            }


            //correct data
            if (!Tools.IsUpToDate(Constants.SAVE_VERSION, fileVersion))
            {
                Logger.Log("Entity json data is old", "correcting data");
                // 2.1 -> 2.1.1
                var newFileVersion = "2.1.1";
                if (!Tools.IsUpToDate(newFileVersion, fileVersion))
                {
                    // renamed speed to agility
                    if (entityJson.TryGetValue("baseSpeed", out object? baseSpeedValue))
                    {
                        entityJson["baseAgility"] = baseSpeedValue;
                    }

                    Logger.Log("Corrected entity json data", $"{fileVersion} -> {newFileVersion}", LogSeverity.DEBUG);
                    fileVersion = newFileVersion;
                }
                // 2.1.1 -> 2.2
                newFileVersion = "2.2";
                if (!Tools.IsUpToDate(newFileVersion, fileVersion))
                {
                    // snake case keys
                    if (entityJson.TryGetValue("baseMaxHp", out object? baseMaxHpRename))
                    {
                        entityJson["base_max_hp"] = baseMaxHpRename;
                    }
                    if (entityJson.TryGetValue("currentHp", out object? chRename))
                    {
                        entityJson["current_hp"] = chRename;
                    }
                    if (entityJson.TryGetValue("baseAttack", out object? baRename))
                    {
                        entityJson["base_attack"] = baRename;
                    }
                    if (entityJson.TryGetValue("baseDefence", out object? bdRename))
                    {
                        entityJson["base_defence"] = bdRename;
                    }
                    if (entityJson.TryGetValue("baseAgility", out object? ba2Rename))
                    {
                        entityJson["base_agility"] = ba2Rename;
                    }
                    if (entityJson.TryGetValue("originalTeam", out object? otRename))
                    {
                        entityJson["original_team"] = otRename;
                    }
                    if (entityJson.TryGetValue("currentTeam", out object? ctRename))
                    {
                        entityJson["current_team"] = ctRename;
                    }
                    if (entityJson.TryGetValue("xPos", out object? xpRename))
                    {
                        entityJson["x_position"] = xpRename;
                    }
                    if (entityJson.TryGetValue("yPos", out object? ypRnename))
                    {
                        entityJson["y_position"] = ypRnename;
                    }

                    Logger.Log("Corrected entity json data", $"{fileVersion} -> {newFileVersion}", LogSeverity.DEBUG);
                    fileVersion = newFileVersion;
                }
                Logger.Log("Entity json data corrected");
            }

            //convert
            var success = true;

            // name
            string? name = null;
            if (
                entityJson.TryGetValue("name", out var playerName) &&
                playerName is not null
            )
            {
                name = playerName.ToString();
            }
            else
            {
                Logger.Log("Entity parse error", "couldn't parse entity name", LogSeverity.WARN);
                success = false;
            }

            // stats
            // max hp
            int? baseMaxHp = null;
            if (
                entityJson.TryGetValue("base_max_hp", out var baseMaxHpValue) &&
                int.TryParse(baseMaxHpValue?.ToString(), out int maxHpValue)
            )
            {
                baseMaxHp = maxHpValue;
            }
            else
            {
                Logger.Log("Entity parse error", "couldn't parse entity base max hp", LogSeverity.WARN);
                success = false;
            }

            // current hp
            int? currentHp = null;
            if (
                entityJson.TryGetValue("current_hp", out var currentHpValueStr) &&
                int.TryParse(currentHpValueStr?.ToString(), out int currentHpValue)
            )
            {
                currentHp = currentHpValue;
            }
            else
            {
                Logger.Log("Entity parse error", "couldn't parse entity current hp", LogSeverity.WARN);
                success = false;
            }

            // attack
            int? baseAttack = null;
            if (
                entityJson.TryGetValue("base_attack", out var baseAttackValue) &&
                int.TryParse(baseAttackValue?.ToString(), out int attackValue)
            )
            {
                baseAttack = attackValue;
            }
            else
            {
                Logger.Log("Entity parse error", "couldn't parse entity base attack", LogSeverity.WARN);
                success = false;
            }

            // defence
            int? baseDefence = null;
            if (
                entityJson.TryGetValue("base_defence", out var baseDefenceValue) &&
                int.TryParse(baseDefenceValue?.ToString(), out int defenceValue)
            )
            {
                baseDefence = defenceValue;
            }
            else
            {
                Logger.Log("Entity parse error", "couldn't parse entity base defence", LogSeverity.WARN);
                success = false;
            }

            // agility
            int? baseAgility = null;
            if (
                entityJson.TryGetValue("base_agility", out var baseAgilityValue) &&
                int.TryParse(baseAgilityValue?.ToString(), out int agilityValue)
            )
            {
                baseAgility = agilityValue;
            }
            else
            {
                Logger.Log("Entity parse error", "couldn't parse entity base agility", LogSeverity.WARN);
                success = false;
            }

            // original team
            int? originalTeam = null;
            if (
                entityJson.TryGetValue("original_team", out var originalTeamValueStr) &&
                int.TryParse(originalTeamValueStr?.ToString(), out int originalTeamValue)
            )
            {
                originalTeam = originalTeamValue;
            }
            else
            {
                Logger.Log("Entity parse error", "couldn't parse entity original team", LogSeverity.WARN);
                success = false;
            }

            // current team
            int? currentTeam = null;
            if (
                entityJson.TryGetValue("current_team", out var currentTeamValueStr) &&
                int.TryParse(currentTeamValueStr?.ToString(), out int currentTeamValue)
            )
            {
                currentTeam = currentTeamValue;
            }
            else
            {
                Logger.Log("Entity parse error", "couldn't parse entity current team", LogSeverity.WARN);
                success = false;
            }

            // attributes
            List<Attribute>? attributes = null;
            if (
                entityJson.TryGetValue("attributes", out var attributesJson) &&
                attributesJson is IEnumerable attributeList
            )
            {
                attributes = new List<Attribute>();
                foreach (var attribute in attributeList)
                {
                    if (
                        Enum.TryParse(attribute?.ToString(), out Attribute attributeEnum) &&
                        Enum.IsDefined(attributeEnum)
                    )
                    {
                        attributes.Add(attributeEnum);
                    }
                    else
                    {
                        Logger.Log("Entity parse error", "entity attribute parse error", LogSeverity.WARN);
                        success = false;
                    }
                }
            }
            else
            {
                Logger.Log("Entity parse error", "couldn't parse entity attributes list", LogSeverity.WARN);
                success = false;
            }

            // drops
            List<AItem>? drops = null;
            if (
                entityJson.TryGetValue("drops", out var dropsJson) &&
                dropsJson is IEnumerable dropList
            )
            {
                drops = new List<AItem>();
                foreach (var dropJson in dropList)
                {
                    success &= AItem.FromJson(dropJson as IDictionary<string, object?>, fileVersion, out AItem? itemObject);
                    if (itemObject is not null)
                    {
                        drops.Add(itemObject);
                    }
                }
            }
            else
            {
                Logger.Log("Entity parse error", "couldn't parse drops list from json", LogSeverity.WARN);
                success = false;
            }

            // position
            (long x, long y)? position = null;
            if (
                entityJson.TryGetValue("x_position", out var xPositionValue) &&
                entityJson.TryGetValue("y_position", out var yPositionValue) &&
                long.TryParse(xPositionValue?.ToString(), out long xPosition) &&
                long.TryParse(yPositionValue?.ToString(), out long yPosition)
            )
            {
                position = (xPosition, yPosition);
            }
            else
            {
                Logger.Log("Entity parse error", "couldn't parse entity position", LogSeverity.WARN);
                success = false;
            }

            // facing
            Facing? facing = null;
            if (
                entityJson.TryGetValue("facing", out var facingValue) &&
                Enum.TryParse(typeof(Facing), facingValue?.ToString(), out object? facingEnum) &&
                Enum.IsDefined(typeof(Facing), (Facing)facingEnum)
            )
            {
                facing = (Facing)facingEnum;
            }
            else
            {
                Logger.Log("Entity parse error", "couldn't parse entity facing", severity: LogSeverity.WARN);
                success = false;
            }

            entityData = (
                name,
                baseMaxHp,
                currentHp,
                baseAttack,
                baseDefence,
                baseAgility,
                originalTeam,
                currentTeam,
                attributes,
                drops,
                position,
                facing
            );
            return success;
        }
        #endregion
        #endregion
    }
}
