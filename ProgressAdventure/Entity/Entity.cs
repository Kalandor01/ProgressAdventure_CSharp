﻿using PACommon;
using PACommon.Enums;
using PACommon.Extensions;
using PACommon.JsonUtils;
using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;
using ProgressAdventure.WorldManagement;
using System.Reflection;
using System.Text;
using Attribute = ProgressAdventure.Enums.Attribute;
using PACTools = PACommon.Tools;

namespace ProgressAdventure.Entity
{
    /// <summary>
    /// A representation of an entity.<br/>
    /// Classes implementing this class MUST create a (protected) constructor, with signiture protected Type([return type from "FromJsonInternal()"] entityData) for FromJson<T>() to work.
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
        public List<EnumValue<Attribute>> attributes;
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
        public Entity(
            string name,
            EntityManagerStatsDTO stats,
            List<AItem>? drops = null
        )
            : this(
                new GenericEntityConstructorParametersDTO()
                {
                    name = name,
                    baseMaxHp = stats.baseMaxHp,
                    currentHp = null,
                    baseAttack = stats.baseAttack,
                    baseDefence = stats.baseDefence,
                    baseAgility = stats.baseAgility,
                    originalTeam = stats.originalTeam,
                    currentTeam = stats.currentTeam,
                    attributes = stats.attributes,
                    drops = drops,
                    position = null,
                    facing = null
                },
                false
            )
        { }
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
            if (RandomStates.Instance.MainRandom.GenerateDouble() < 0.2)
            {
                var oldFacing = facing;
                var angle = RandomStates.Instance.MainRandom.Triangular(-180, 0, 180);
                var newFacing = EntityUtils.RotateFacing(oldFacing, angle);

                if (newFacing is not null && newFacing != oldFacing)
                {
                    facing = (Facing)newFacing;
                    PACSingletons.Instance.Logger.Log("Entity turned", $"name: {FullName}, {oldFacing} -> {facing}", LogSeverity.DEBUG);
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
            var moveRaw = EntityUtils.FacingToMovementVectorMap[facing ?? this.facing];
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
            PACSingletons.Instance.Logger.Log("Entity moved", $"name: {FullName}, {oldPosition} -> {this.position}", LogSeverity.DEBUG);
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
            PACSingletons.Instance.Logger.Log("Attack log", $"{FullName} attacked {target.FullName}");
            // attacker dead
            if (CurrentHp == 0)
            {
                PACSingletons.Instance.Logger.Log("Attack log", $"{FullName}(attacker) is dead");
                return AttackResponse.ENTITY_DEAD;
            }
            // enemy dead
            else if (target.CurrentHp == 0)
            {
                PACSingletons.Instance.Logger.Log("Attack log", $"{FullName}(attacked) is already dead");
                return AttackResponse.TARGET_DEAD;
            }
            // enemy dodge
            else if (RandomStates.Instance.MiscRandom.GenerateDouble() > Agility * 1.0 / target.Agility - 0.1)
            {
                PACSingletons.Instance.Logger.Log("Attack log", $"{target.FullName} dodged");
                return AttackResponse.TARGET_DOGDED;
            }
            // attack
            else
            {
                var attack = (int)RandomStates.Instance.MiscRandom.GenerateInRange(1, 7) + Attack;
                var damage = attack - target.Defence;
                // block
                if (damage <= 0)
                {
                    PACSingletons.Instance.Logger.Log("Attack log", $"{target.FullName} blocked attack");
                    return AttackResponse.TARGET_BLOCKED;
                }
                // hit
                else
                {
                    target.TakeDamage(damage);
                    PACSingletons.Instance.Logger.Log("Attack log", $"{target.FullName} took {damage} damage ({target.CurrentHp})");
                    // kill
                    if (target.CurrentHp == 0)
                    {
                        PACSingletons.Instance.Logger.Log("Attack log", $"{FullName} defeated {target.FullName}");
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
                var (maxHp, attack, defence, agility) = EntityUtils.AttributeStatChangeMap[attribute];
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
        /// <param name="invokedFromFromJson">Whether this constructor was called using the invoke method, as a part of loading an entity from json, and so the entity's attributes shouldn't be set yet.</param>
        protected Entity(GenericEntityConstructorParametersDTO entityData, bool invokedFromFromJson)
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
                var entityStats = GetBaseStats();
                baseMaxHp = entityData.baseMaxHp ?? entityStats.baseMaxHp;
                baseAttack = entityData.baseAttack ?? entityStats.baseAttack;
                baseDefence = entityData.baseDefence ?? entityStats.baseDefence;
                baseAgility = entityData.baseAgility ?? entityStats.baseAgility;
                originalTeam = entityData.originalTeam ?? entityStats.originalTeam;
                currentTeam = entityData.currentTeam ?? entityStats.currentTeam;
                attributes = entityData.attributes ?? entityStats.attributes;
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

            if (!invokedFromFromJson)
            {
                SetupAttributes(entityData.currentHp);
                UpdateFullName();
            }
        }
        #endregion

        #region Methods
        public virtual JsonDictionary ToJson()
        {
            // attributes
            var attributesProcessed = attributes.Select(a => (JsonObject?)a).ToList();
            // drops
            var dropsJson = drops.Select(drop => (JsonObject?)drop.ToJson()).ToList();
            // properties
            return new JsonDictionary
            {
                [Constants.JsonKeys.Entity.TYPE] = EntityUtils.GetEntityTypeName(this),
                [Constants.JsonKeys.Entity.NAME] = name,
                [Constants.JsonKeys.Entity.BASE_MAX_HP] = baseMaxHp,
                [Constants.JsonKeys.Entity.CURRENT_HP] = CurrentHp,
                [Constants.JsonKeys.Entity.BASE_ATTACK] = baseAttack,
                [Constants.JsonKeys.Entity.BASE_DEFENCE] = baseDefence,
                [Constants.JsonKeys.Entity.BASE_AGILITY] = baseAgility,
                [Constants.JsonKeys.Entity.ORIGINAL_TEAM] = originalTeam,
                [Constants.JsonKeys.Entity.CURRENT_TEAM] = currentTeam,
                [Constants.JsonKeys.Entity.ATTRIBUTES] = attributesProcessed,
                [Constants.JsonKeys.Entity.DROPS] = dropsJson,
                [Constants.JsonKeys.Entity.X_POSITION] = position.x,
                [Constants.JsonKeys.Entity.Y_POSITION] = position.y,
                [Constants.JsonKeys.Entity.FACING] = (int)facing,
            };
        }

        /// <summary>
        /// Converts the misc data of the entity from a json version.
        /// </summary>
        /// <param name="miscJson">The json representation of the misc data for the specific entity type.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <returns>If it was succesful without any warnings.</returns>
        protected virtual bool FromMiscJson(JsonDictionary miscJson, string fileVersion)
        {
            return true;
        }
        #endregion

        #region Public functions
        /// <summary>
        /// Tries to convert the json representation of the entity to a specific entity object.
        /// </summary>
        /// <typeparam name="T">The type of the entity to try to convert into.</typeparam>
        /// <param name="entityJson">The json representation of the entity.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="entityObject">The converted entity object.</param>
        /// <exception cref="ArgumentNullException">Thrown if the entity type couldn't be converted from json with the required constructor.</exception>
        /// <returns>If it was succesful without any warnings.</returns>
        public static bool FromJsonWithoutGeneralCorrection<T>(JsonDictionary? entityJson, string fileVersion, out T? entityObject)
            where T : Entity<T>
        {
            if (entityJson is null)
            {
                entityObject = null;
                return false;
            }

            var success = AnyEntityFromJsonPrivate(typeof(T), entityJson, fileVersion, out Entity? entity);
            entityObject = (T?)entity;
            return success;
        }

        /// <summary>
        /// Tries to convert any entity json, from a json format, into an entity object (if it implements the nececary protected constructor).
        /// </summary>
        /// <param name="entityJson">The json representation of an entity.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="entityObject">The converted entity object.</param>
        /// <returns>If it was succesful without any warnings.</returns>
        public static bool AnyEntityFromJson(JsonDictionary? entityJson, string fileVersion, out Entity? entityObject)
        {
            entityObject = null;
            if (entityJson is null)
            {
                PACTools.LogJsonNullError<Entity>(nameof(Entity), null, true);
                return false;
            }

            if (!(
                PACTools.TryParseJsonValue<Entity, string>(
                    entityJson,
                    Constants.JsonKeys.Entity.TYPE,
                    out var entityTypeValue,
                    isCritical: true
                ) &&
                EntityUtils.EntityTypeMap.TryGetValue(entityTypeValue ?? "", out Type? entityType) &&
                entityType is not null
            ))
            {
                PACTools.LogJsonError<Entity>("invalid entity type", true);
                return false;
            }

            return AnyEntityFromJsonPrivate(entityType, entityJson, fileVersion, out entityObject);
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
            return [ItemUtils.CreateCompoundItem(ItemType.Misc.COIN, Material.COPPER)];
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Tries to convert the json representation of the entity to a specific entity object.
        /// </summary>
        /// <param name="entityType">The type of the entity to try to convert into.</param>
        /// <param name="entityJson">The json representation of the entity.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="entityObject">The converted entity object.</param>
        /// <exception cref="ArgumentNullException">Thrown if the entity type couldn't be converted from json with the required constructor.</exception>
        /// <returns>If it was succesful without any warnings.</returns>
        private static bool AnyEntityFromJsonPrivate(Type entityType, JsonDictionary entityJson, string fileVersion, out Entity? entityObject)
        {
            var success = CommonAttributesFromJson(entityJson, fileVersion, out var entityData);

            // get entity
            var constructor = entityType.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.CreateInstance | BindingFlags.Instance,
                null,
                [entityData.GetType()],
                null
            ) ?? throw new ArgumentNullException(message: $"Couldn't find required entity constructor for type \"{entityType}\"!", null);
            var entity = constructor.Invoke([entityData]) ?? throw new ArgumentNullException(message: $"Couldn't create entity object from type \"{entityType}\"!", null);
            entityObject = (Entity?)entity;
            if (entityObject is null)
            {
                return false;
            }

            success &= entityObject.FromMiscJson(entityJson, fileVersion);
            entityObject.SetupAttributes(entityData.currentHp);
            entityObject.UpdateFullName();

            return success && entityObject is not null;
        }

        /// <summary>
        /// Tries to convert the json representation of the <c>Entity</c> to a format that can easily be turned to an <c>Entity</c> object.
        /// </summary>
        /// <param name="entityJson">The json representation of the <c>Entity</c>.</param>
        /// <param name="fileVersion">The version number of the loaded file.</param>
        /// <param name="entityData">The basic data of the entity.</param>
        /// <returns>If it was succesful without any warnings.</returns>
        private static bool CommonAttributesFromJson(
            JsonDictionary entityJson,
            string fileVersion,
            out GenericEntityConstructorParametersDTO entityData
        )
        {
            var success = true;
            success &= PACTools.TryParseJsonValue<Entity, string?>(entityJson, Constants.JsonKeys.Entity.NAME, out var name);
            success &= PACTools.TryParseJsonValue<Entity, int?>(entityJson, Constants.JsonKeys.Entity.BASE_MAX_HP, out var baseMaxHp);
            success &= PACTools.TryParseJsonValue<Entity, int?>(entityJson, Constants.JsonKeys.Entity.CURRENT_HP, out var currentHp);
            success &= PACTools.TryParseJsonValue<Entity, int?>(entityJson, Constants.JsonKeys.Entity.BASE_ATTACK, out var baseAttack);
            success &= PACTools.TryParseJsonValue<Entity, int?>(entityJson, Constants.JsonKeys.Entity.BASE_DEFENCE, out var baseDefence);
            success &= PACTools.TryParseJsonValue<Entity, int?>(entityJson, Constants.JsonKeys.Entity.BASE_AGILITY, out var baseAgility);
            success &= PACTools.TryParseJsonValue<Entity, int?>(entityJson, Constants.JsonKeys.Entity.ORIGINAL_TEAM, out var originalTeam);
            success &= PACTools.TryParseJsonValue<Entity, int?>(entityJson, Constants.JsonKeys.Entity.CURRENT_TEAM, out var currentTeam);
            success &= PACTools.TryParseJsonListValue<Entity, EnumValue<Attribute>>(entityJson, Constants.JsonKeys.Entity.ATTRIBUTES,
                attribute => {
                    var attributeSuccess = PACTools.TryParseValueForJsonParsing<Entity, EnumValue<Attribute>>(attribute, out var value, logParseWarnings: false);
                    success &= attributeSuccess;
                    return (attributeSuccess, value);
                },
                out var attributes);
            success &= PACTools.TryParseJsonListValue<Entity, AItem>(entityJson, Constants.JsonKeys.Entity.DROPS,
                dropJson => {
                    var dropSuccess = PACTools.TryFromJson(dropJson as JsonDictionary, fileVersion, out AItem? dropObject);
                    success &= dropSuccess;
                    return (dropSuccess, dropObject);
                },
                out var drops);

            (long x, long y)? position =
                PACTools.TryParseJsonValue<Entity, long>(entityJson, Constants.JsonKeys.Entity.X_POSITION, out var xPosition) &&
                PACTools.TryParseJsonValue<Entity, long>(entityJson, Constants.JsonKeys.Entity.Y_POSITION, out var yPosition) ?
                (xPosition, yPosition) : null;
            if (position is null)
            {
                PACTools.LogJsonParseError<Entity>(nameof(position));
                success = false;
            }

            success &= PACTools.TryParseJsonValue<Entity, Facing?>(entityJson, Constants.JsonKeys.Entity.FACING, out var facing);

            entityData = new GenericEntityConstructorParametersDTO
            {
                name = name,
                baseMaxHp = baseMaxHp,
                currentHp = currentHp,
                baseAttack = baseAttack,
                baseDefence = baseDefence,
                baseAgility = baseAgility,
                originalTeam = originalTeam,
                currentTeam = currentTeam,
                attributes = attributes,
                drops = drops,
                position = position,
                facing = facing
            };
            return success;
        }
        #endregion
        #endregion
    }
}
