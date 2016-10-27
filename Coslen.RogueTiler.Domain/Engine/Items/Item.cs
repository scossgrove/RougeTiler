using System;
using System.Text;
using Coslen.RogueTiler.Domain.Engine.Common;
using Coslen.RogueTiler.Domain.Engine.Entities;
using Action = Coslen.RogueTiler.Domain.Engine.Actions.Action;

namespace Coslen.RogueTiler.Domain.Engine.Items
{
    public class Item : Thing, IComparable
    {
        // TODO: this needs to be implemented
        public int Id;
        public int InventorySlotId;
        public Affix prefix;
        public Affix suffix;
        public ItemType type;

        public Item() : this(null, null, null)
        {
            Id = -1;
            InventorySlotId = -1;
        }

        public Item(ItemType type, Affix prefix = null, Affix suffix = null) : base(Vector.zero, null, null)
        {
            this.type = type;
            this.prefix = prefix;
            this.suffix = suffix;

            base.NounText = NounText;
            InventorySlotId = -1;
            //var defaultSlug = "Treasure_Chest_64";
            Id = -1;
        }

        public new Appearence Appearance
        {
            get { return new Appearence { Glyph = type.glyph.Appearance, ForeGroundColor = type.glyph.Fore, Position = Position}; }
        }

        public bool isRanged
        {
            get { return type.attack is RangedAttack; }
        }

        public bool canEquip
        {
            get { return !string.IsNullOrWhiteSpace(equipSlot); }
        }

        public string equipSlot
        {
            get { return type.equipSlot; }
        }

        /// Whether the item can be used or not.
        public bool canUse
        {
            get { return type.use != null; }
        }

        /// Whether the item can be thrown or not.
        public bool canToss
        {
            get { return type.tossAttack != null; }
        }

        /// Gets the melee [Attack] for the item, taking into account any [Affixes]s
        // it has.
        public Attack attack
        {
            get
            {
                if (type.attack == null)
                {
                    return null;
                }

                var attack = type.attack;
                if (prefix != null && prefix.attack != null)
                {
                    attack = attack.Combine(prefix.attack);
                }

                if (suffix != null && suffix.attack != null)
                {
                    attack = attack.Combine(suffix.attack);
                }

                return attack;
            }
        }

        /// The amount of protected provided by the item when equipped.
        public int armor
        {
            get { return type.armor; }
        }

        public string Description
        {
            get { return "Need to implement descriptions"; }
        }

        public string Title
        {
            get { return NounText; }
        }

        public string Slug
        {
            get { return "Need to implement slugs"; }
        }

        public bool Stackable
        {
            get { return true; }
        }

        public new string NounText
        {
            get
            {
                var name = new StringBuilder();
                name.Append("a ");

                if (prefix != null)
                {
                    name.Append(prefix.name);
                    name.Append(" ");
                }

                if (type != null)
                {
                    name.Append(type.name);
                }

                if (suffix != null)
                {
                    name.Append(" ");
                    name.Append(suffix.name);
                }

                var result = name.ToString().Trim();

                if (result == "a")
                {
                    result = "Unknown Item";
                }

                return result;
            }
        }

        // TODO: Take affixes into account.
        public int price
        {
            get { return type.price; }
        }

        public bool isTreasure
        {
            get { return type.isTreasure; }
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            var other = obj as Item;

            // TODO: Take into account affixes.
            return type.sortIndex.CompareTo(other.type.sortIndex);
        }

        public Action use()
        {
            return type.use();
        }

        /// Creates a new [Item] with the same type and affixes as this one.
        public Item Clone()
        {
            return new Item(type, prefix, suffix);
        }

        public void SetActive(bool isActive)
        {
            // TODO: GameObject.SetActive(IsActive);
        }
    }

    public delegate Action ItemUse();

    public delegate void AddItem(Item item);
}