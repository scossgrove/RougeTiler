using System;
using System.Collections.Generic;
using System.Linq;
using Coslen.RogueTiler.Domain.Engine.Common;

namespace Coslen.RogueTiler.Domain.Engine.Items
{
    /// A kind of [Item]. Each item will have a type that describes the item.
    public class ItemType : ICloneable
    {
        public int armor;

        /// The item's [Attack] or `null` if the item is not an equippable weapon.
        public Attack attack;

        /// The percent chance of the item breaking when thrown. `null` if the item
        /// can't be thrown.
        public int? breakage;

        /// The path to this item type in the hierarchical organization of items.
        /// 
        /// May be empty for uncategorized items.
        public List<string> categories;

        public string description;

        /// The name of the [Equipment] slot that [Item]s can be placed in. If `null`
        /// then this Item cannot be equipped.
        public string equipSlot;

        public Glyph glyph;

        public int id;

        /// True if this item is "treasure".
        /// 
        /// That means it just has a gold value. As soon as the hero steps on it, it
        /// increases the hero's gold and disappears.
        public bool isTreasure;

        /// The item's level.
        /// 
        /// Higher level items are found later in the game. Some items may not have
        /// a level.
        public int level;

        public string name;

        /// How much gold this item is worth.
        public int price;

        public string slug;

        public int sortIndex;
        public bool stackable;

        /// The item's [RangedAttack] when thrown or `null` if the item can't be
        /// thrown.
        public RangedAttack tossAttack;

        public ItemUse use;

        public ItemType(string name, Glyph glyph, int level, int sortIndex, List<string> categories, string equipSlot, ItemUse use, Attack attack, RangedAttack tossAttack, 
            int? breakage, int armor, int price, bool treasure, int id = 0, string description = null, string slug = null, bool stackable = false)
        {
            this.name = name;
            this.glyph = glyph;
            this.level = level;
            this.sortIndex = sortIndex;
            this.categories = categories;
            this.equipSlot = equipSlot;
            this.use = use;
            this.attack = attack;
            this.tossAttack = tossAttack;
            this.breakage = breakage;
            this.armor = armor;
            this.price = price;
            isTreasure = treasure;
            this.id = id;
            this.description = description;
            this.slug = slug;
            this.stackable = stackable;
        }

        /// A more precise categorization than [equipSlot]. For example, "dagger",
        /// or "cloak". May be `null`.
        public string category
        {
            get
            {
                if (categories.Count == 0)
                {
                    return null;
                }
                return categories.Last();
            }
        }

        public override string ToString()
        {
            return name;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        protected bool Equals(ItemType other)
        {
            return string.Equals(name, other.name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ItemType) obj);
        }

        public override int GetHashCode()
        {
            return (name != null ? name.GetHashCode() : 0);
        }
    }
}