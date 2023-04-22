using ProgressAdventure.Enums;
using ProgressAdventure.ItemManagement;
using Attribute = ProgressAdventure.Enums.Attribute;

namespace ProgressAdventure.Entity
{
    public class Player : Entity
    {
        #region Public constructors
        public Player(string name = "You", Inventory? inventory = null, (int x, int y)? position = null, Facing? facing = null)
            :this(EntityUtils.EntityManager(
                    (14, 20, 26),
                    (7, 10, 13),
                    (7, 10, 13),
                    (1, 10, 20),
                    0,
                    0,
                    0,
                    name
                )
            )
        { }

        public Player(
            string name,
            int baseHp,
            int baseAttack,
            int baseDefence,
            int baseSpeed,
            Inventory? inventory = null,
            (int x, int y)? position = null,
            Facing? facing = null
        )
            :base(name, baseHp, baseAttack, baseDefence, baseSpeed, 0, 0, new List<Attribute>(), new List<Item>())
        {
            //if string.IsNullOrWhiteSpace(name):
            //    name = "You"
            //if base_hp is None or base_attack is None or base_defence is None or base_speed is None:
            //    super().__init__(*entity_master(range(14, 26), range(7, 13), range(7, 13), range(1, 20), 0, 0, 0, 0, 0, name))
            //else:
            //    super().__init__(name, base_hp, base_attack, base_defence, base_speed)
            //if inventory is None:
            //    inventory = Inventory()
            //if position is None:
            //    position = (0, 0)
            //if rotation is None:
            //    rotation = Rotation.NORTH
            //self.inventory = inventory
            //self.pos:tuple[int, int] = (int(position[0]), int(position[1]))
            //self.rotation:Rotation = rotation
            //self.update_full_name()
        }
        #endregion

        #region Private constructors
        private Player(
            (string name, int baseHpValue, int baseAttackValue, int baseDefenceValue, int baseSpeedValue, int originalTeam, int currentTeam, List<Attribute> attributes) stats,
            Inventory? inventory = null,
            (int x, int y)? position = null,
            Facing? facing = null
        )
            : this(
                  stats.name,
                  stats.baseHpValue,
                  stats.baseAttackValue,
                  stats.baseDefenceValue,
                  stats.baseSpeedValue,
                  inventory,
                  position,
                  facing
                )
        { }
        #endregion


        //def weighted_turn(self):
        //    """Turns the player in a random direction, that is weighted in the direction that it's already going towards."""
        //    # turn
        //    if main_seed.rand() > 0.75:
        //        old_rot = self.rotation
        //        move_vec = _facing_to_movement_vector(self.rotation)
        //        # back
        //        if main_seed.rand() > 0.75:
        //            new_dir = _movement_vector_to_facing(vector_multiply(move_vec, (-1, -1), True))
        //        else:
        //            new_dir = _movement_vector_to_facing((move_vec[1], move_vec[0]))
        //            new_dir = _movement_vector_to_facing((move_vec[1], move_vec[0]))

        //        if new_dir is not None:
        //            self.rotation = new_dir
        //            logger("Player turned", f"{old_rot} -> {self.rotation}", Log_type.DEBUG)


        //def move(self, amount:tuple[int, int]|None= None, direction:Rotation|None= None):
        //    """
        //    Moves the player in the direction it's facing.\n
        //    If `direction` is specified, it will move in that direction instead.\n
        //    the amount the player is moved in the x and y direction is specified by the `amount` tuple.
        //    """
        //    old_pos = self.pos
        //    if direction is None:
        //        direction = self.rotation
        //    if amount is None:
        //        amount = (1, 1)
        //    move_raw = _facing_to_movement_vector(direction)
        //    move = vector_multiply(move_raw, amount)
        //    self.pos = vector_add(self.pos, move, True)
        //    logger("Player moved", f"{old_pos} -> {self.pos}", Log_type.DEBUG)


        //def to_json(self):
        //    """Returns a json representation of the `Entity`."""
        //    player_json = super().to_json()
        //    player_json["x_pos"] = self.pos[0]
        //    player_json["y_pos"] = self.pos[1]
        //    player_json["rotation"] = self.rotation.value
        //    player_json["inventory"] = self.inventory.to_json()
        //    return player_json


        //def __str__(self) :
        //    return f"{super().__str__()}\n{self.inventory}\nPosition: {self.pos}\nRotation: {self.rotation}"
    }
}
