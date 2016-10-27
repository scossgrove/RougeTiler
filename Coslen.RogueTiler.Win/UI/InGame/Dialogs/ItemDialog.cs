using System;
using System.Collections.Generic;
using System.Linq;
using Coslen.RogueTiler.Domain.Engine;
using Coslen.RogueTiler.Domain.Engine.Items;
using Coslen.RogueTiler.Domain.Engine.Items.Commands;
using Coslen.RogueTiler.Win.Utilities;
using Coslen.RogueTiler.Win.Utilities.BufferUtilities;

namespace Coslen.RogueTiler.Win.UI
{
    public enum ItemDialogUsage
    {
        None = 0,
        Drop,
        Use,
        Toss,
        PickUp
    }

    public class ItemDialogResult
    {
        public ItemCommand Command { get; set; }
        public Item Item { get; set; }
        public ItemLocation Location { get; set; }
        public int ItemIndex { get; set; }
        public bool RequiresTargetDialog { get; set; }
    }

    /// Modal dialog for letting the user perform an [Action] on an [Item]
    /// accessible to the [Hero].
    public class ItemDialog : DialogBase
    {
        /// The current location being shown to the player.
        private ItemLocation Location = ItemLocations.Inventory;

        public ItemDialog(Game game) : this("ItemDialog", 20, 10, Console.WindowHeight - 10, Console.WindowWidth - 20, -1, game)
        {
        }

        public new ItemDialogResult DialogResult { get; set; }

        public ItemDialog(string key, int left, int top, int bottom, int right, int renderOrder, Game game) : base(key, left, top, bottom, right, renderOrder)
        {
            Game = game;
        }

        public Game Game { get; }

        /// The command the player is trying to perform on an item.
        public ItemCommand Command { get; set; }

        public bool IsTransparent => true;

        /// True if the item dialog supports tabbing between item lists.
        public bool CanSwitchLocations => Command.AllowedLocations().Count > 1;

        public void Drop()
        {
            Command = new DropItemCommand();
        }

        public void Use()
        {
            Command = new UseItemCommand();
        }

        public void Toss()
        {
            Command = new TossItemCommand();
        }

        public void PickUp()
        {
            Command = new PickUpItemCommand();
            Location = ItemLocations.OnGround;
        }

        protected override bool HandleInput()
        {
            do
            {
                var input = GetPlayerInput();

                if (input == Inputs.cancel)
                {
                    return true;
                }
                if (ConvertKeyToNumber(input) != -1)
                {
                    SelectItem(ConvertKeyToNumber(input), Game);
                    return true;
                }

                if (input == Inputs.tab)
                {
                    AdvanceLocation();
                    return false;
                }
            } while (true);
        }

        public int ConvertKeyToNumber(ConsoleKeyInfo key)
        {
            var alphabet = "abcdefghijklmnopqrstuvwxyz";

            return alphabet.IndexOf(key.KeyChar);
        }

        public override void Draw(BufferContainer buffer)
        {
            buffer.Clear();
            FrameArea(buffer);

            // Header for Item Dialog
            var lineCounter = 0;
            WriteAt(buffer, 0, lineCounter++, Command.Query(Location));

            // Options for Item Dialog
            var selectItem = "[A-Z] Select item";
            var helpText = CanSwitchLocations ? ", [Tab] Switch view" : "";

            WriteAt(buffer, 0, lineCounter++, $"{selectItem}{helpText}", ConsoleColor.Gray);

            // List of Items
            DrawItems(buffer, 0, lineCounter + 2, GetItems(Game), item => Command.CanSelect(item));
        }

        private void DrawStat(BufferContainer buffer, int x, int y, string symbol, object stat, ConsoleColor light, ConsoleColor dark, bool enabled)
        {
            var statString = stat.ToString();

            WriteAt(buffer, x + 47 - statString.Length, y, symbol, enabled ? dark : ConsoleColor.DarkGray);
            WriteAt(buffer, x + 49 - statString.Length, y, statString, enabled ? light : ConsoleColor.DarkGray);
        }

        /// Converts an integer to a comma-grouped string like "123,456".
        private string PriceString(int price)
        {
            var result = price.ToString();
            if (price > 999999999)
            {
                result = result.Substring(0, result.Length - 9) + "," + result.Substring(result.Length - 9);
            }

            if (price > 999999)
            {
                result = result.Substring(0, result.Length - 6) + "," + result.Substring(result.Length - 6);
            }

            if (price > 999)
            {
                result = result.Substring(0, result.Length - 3) + "," + result.Substring(result.Length - 3);
            }

            return result;
        }

        /// Draws a list of [items] on [terminal] at [x], [y].
        /// 
        /// This is used both on the [ItemScreen] and in game for things like using and
        /// dropping items.
        /// 
        /// Items can be drawn in one of three states:
        /// 
        /// * If [canSelect] is `null`, then item list is just being viewed and no
        /// items in particular are highlighted.
        /// * If [canSelect] returns `true`, the item is highlighted as being
        /// selectable.
        /// * If [canSelect] returns `false`, the item cannot be selected and is
        /// grayed out.
        /// 
        /// An item row looks like this:
        /// 1         2         3         4
        /// 01234567890123456789012345678901234567890123456789
        /// a) = a Glimmering War Hammer of Wo... »29 992,106
        private void DrawItems(BufferContainer buffer, int x, int y, List<Item> items, Func<Item, bool> canSelect)
        {
            var i = 0;
            foreach (var item in items)
            {
                var alphabet = "abcdefghijklmnopqrstuvwxyz";
                var itemY = i + y;

                var borderColor = ConsoleColor.DarkGray;
                var letterColor = ConsoleColor.Gray;
                var textColor = ConsoleColor.White;
                var priceColor = ConsoleColor.Gray;
                var enabled = true;

                if (canSelect != null)
                {
                    if (canSelect(item))
                    {
                        borderColor = ConsoleColor.Gray;
                        letterColor = ConsoleColor.Yellow;
                        textColor = ConsoleColor.White;
                        priceColor = ConsoleColor.DarkYellow;
                    }
                    else
                    {
                        borderColor = ConsoleColor.Black;
                        letterColor = ConsoleColor.Black;
                        textColor = ConsoleColor.DarkGray;
                        priceColor = ConsoleColor.DarkGray;
                        enabled = false;
                    }
                }


                WriteAt(buffer, x, itemY, " )                                               ", borderColor);
                WriteAt(buffer, x, itemY, alphabet[i].ToString(), letterColor);

                if (item == null)
                {
                    // what is the location?
                    if (Location == ItemLocations.Equipment)
                    {
                        var text = ((EquipementSlot) i) + " slot is empty";
                        WriteAt(buffer, x + 3, itemY, text, textColor);
                    }
                }
                else
                {
                    if (enabled)
                    {
                        WriteAt(buffer, x + 3, itemY, item.Appearance.Glyph, ColourUtilities.ConvertToConsoleColor(item.Appearance.ForeGroundColor));
                    }

                    var text = item.NounText;
                    if (text.Length > 32)
                    {
                        text = text.Substring(0, 29) + "...";
                    }
                    WriteAt(buffer, x + 5, itemY, text, textColor);

                    // TODO: Eventually need to handle equipment that gives both an armor and attack bonus.
                    if (item.attack != null)
                    {
                        DrawStat(buffer, x, itemY, "»", item.attack.AverageDamage, ConsoleColor.Yellow, ConsoleColor.DarkYellow, enabled);
                    }
                    else if (item.armor != 0)
                    {
                        DrawStat(buffer, x, itemY, "•", item.armor, ConsoleColor.Green, ConsoleColor.DarkGreen, enabled);
                    }

                    if (item.price != 0)
                    {
                        var price = PriceString(item.price);
                        WriteAt(buffer, x + 49 - price.Length, itemY, price, priceColor);
                    }
                }

                // Increment the item counter
                i++;
            }
        }

        private void SelectItem(int index, Game game)
        {
            var items = GetItems(game).ToList();
            if (index >= items.Count)
            {
                return;
            }
            if (!Command.CanSelect(items[index]))
            {
                return;
            }

            bool requiresTargetDialog = false;
            if (Command is TossItemCommand)
            {
                requiresTargetDialog = true;
            }

            DialogResult = new ItemDialogResult()
            {
                Command = Command,
                Item = items[index],
                Location = Location,
                ItemIndex = index,
                RequiresTargetDialog = requiresTargetDialog
            };
        }

        private List<Item> GetItems(Game game)
        {
            if (Location == ItemLocations.Inventory)
            {
                return game.Hero.Inventory;
            }
            if (Location == ItemLocations.Equipment)
            {
                return game.Hero.Equipment.Items;
            }
            if (Location == ItemLocations.OnGround)
            {
                return game.CurrentStage.itemsAt(game.Hero.Position);
            }

            throw new ApplicationException("unreachable");
        }

        /// Rotates through the viewable locations the player can select an item from.
        public void AdvanceLocation()
        {
            var index = Command.AllowedLocations().IndexOf(Location);
            Location = Command.AllowedLocations()[(index + 1)%Command.AllowedLocations().Count];
        }
    }
}