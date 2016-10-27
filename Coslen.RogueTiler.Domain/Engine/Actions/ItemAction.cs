using System;
using Coslen.RogueTiler.Domain.Engine.Items;

namespace Coslen.RogueTiler.Domain.Engine.Actions
{
    /// Base class for an [Action] that works with an existing [Item] in the game.
    public abstract class ItemAction : Action
    {
        /// The index of the item in the collection where its located.
        public int itemIndex;

        /// The location of the item in the game.
        public ItemLocation location;

        public ItemAction(ItemLocation location, int itemIndex)
        {
            this.location = location;
            this.itemIndex = itemIndex;
        }

        /// Gets the referenced item.
        public Item item
        {
            get
            {
                if (location == ItemLocations.OnGround)
                {
                    return Game.CurrentStage.itemsAt(Actor.Position)[itemIndex];
                }
                if (location == ItemLocations.Inventory)
                {
                    return hero.Inventory[itemIndex];
                }
                if (location == ItemLocations.Equipment)
                {
                    return hero.Equipment[itemIndex];
                }
                throw new ApplicationException("unreachable");
            }
        }

        /// Removes the item from its current location so it can be placed elsewhere.
        public Item removeItem()
        {
            Item item = null;

            if (location == ItemLocations.OnGround)
            {
                item = Game.CurrentStage.itemsAt(Actor.Position)[itemIndex];
                Game.CurrentStage.removeItem(item);
            }

            if (location == ItemLocations.Inventory)
            {
                item = hero.Inventory.removeAt(itemIndex);
            }

            if (location == ItemLocations.Equipment)
            {
                item = hero.Equipment.removeAt((EquipementSlot) itemIndex);
            }

            item.Position = Actor.Position;
            return item;
        }
    }

    /// [Action] for picking up an [Item] off the ground.
    public class PickUpAction : Action
    {
        private readonly int index;

        public PickUpAction(int index)
        {
            this.index = index;
        }

        public override ActionResult OnPerform()
        {
            if (index == -1)
            {
                return Fail("There is nothing here.");
            }

            var item = Game.CurrentStage.itemsAt(Actor.Position)[index];

            if (!hero.Inventory.tryAdd(item, false))
            {
                return Fail("{1} [don't|doesn't] have room for {2}.", Actor.NounText, item.NounText);
            }

            Game.CurrentStage.removeItem(item);
            item.SetActive(false);

            Log("{1} pick[s] up {2}.", Actor.NounText, item.NounText);

            //game.quest.pickUpItem(game, item);
            return Succeed();
        }
    }

    /// [Action] for dropping an [Item] from the [Hero]'s [Inventory] or [Equipment]
    /// onto the ground.
    public class DropAction : ItemAction
    {
        public DropAction(ItemLocation location, int index) : base(location, index)
        {
        }

        public override ActionResult OnPerform()
        {
            var dropped = removeItem();
            Game.CurrentStage.Items.Add(dropped);

            if (location == ItemLocations.Equipment)
            {
                return Succeed("{1} take[s] off and drop[s] {2}.", Actor.NounText, dropped.NounText);
            }
            return Succeed("{1} drop[s] {2}.", Actor.NounText, dropped.NounText);
        }
    }

    /// [Action] for moving an [Item] from the [Hero]'s [Inventory] to his
    /// [Equipment]. May cause a currently equipped Item to become unequipped. If
    /// there is no room in the Inventory for that Item, it will drop to the ground.
    public class EquipAction : ItemAction
    {
        public EquipAction(ItemLocation location, int index) : base(location, index)
        {
        }

        public override ActionResult OnPerform()
        {
            // If it's already equipped, unequip it.
            if (location == ItemLocations.Equipment)
            {
                return alternate(new UnequipAction(location, itemIndex));
            }

            if (!hero.Equipment.canEquip(item))
            {
                return Fail("{1} cannot equip {2}.", Actor.NounText, item.NounText);
            }

            var equipped = removeItem();
            var unequipped = hero.Equipment.equip(equipped);

            // Add the previously equipped item to inventory.
            if (unequipped != null)
            {
                if (hero.Inventory.tryAdd(unequipped, true))
                {
                    Log("{1} unequip[s] {2}.", Actor.NounText, unequipped.NounText);
                }
                else
                {
                    // No room in inventory, so drop it.
                    unequipped.Position = Actor.Position;
                    Game.CurrentStage.Items.Add(unequipped);
                    Log("{1} [don't|doesn't] have room for {2} and {2 he} drops to the ground.", Actor.NounText, unequipped.NounText);
                }
            }

            return Succeed("{1} equip[s] {2}.", Actor.NounText, equipped.NounText);
        }
    }

    /// [Action] for moving an [Item] from the [Hero]'s [Equipment] to his
    /// [Inventory]. If there is no room in the inventory, it will drop to the
    /// ground.
    public class UnequipAction : ItemAction
    {
        public UnequipAction(ItemLocation location, int index) : base(location, index)
        {
        }

        public override ActionResult OnPerform()
        {
            var item = removeItem();

            if (hero.Inventory.tryAdd(item, true))
            {
                return Succeed("{1} unequip[s] {2}.", Actor.NounText, item.NounText);
            }

            // No room in inventory, so drop it.
            item.Position = Actor.Position;
            Game.CurrentStage.Items.Add(item);
            return Succeed("{1} [don't|doesn't] have room for {2} and {2 he} drops to the ground.", Actor.NounText, item.NounText);
        }
    }

    /// [Action] for using an [Item] from the [Hero]'s [Inventory] or the ground.
    /// If the Item is equippable, then using it means equipping it.
    public class UseAction : ItemAction
    {
        public UseAction(ItemLocation location, int index) : base(location, index)
        {
        }

        public override ActionResult OnPerform()
        {
            // If it's equippable, then using it just equips it.
            if (item.canEquip)
            {
                return alternate(new EquipAction(location, itemIndex));
            }

            if (!item.canUse)
            {
                return Fail("{1} can't be used.", item.NounText);
            }

            return alternate(removeItem().use());
        }
    }
}