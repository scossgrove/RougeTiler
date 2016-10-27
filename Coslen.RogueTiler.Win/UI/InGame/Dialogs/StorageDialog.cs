using System;
using System.Collections.Generic;
using System.Linq;
using Coslen.RogueTiler.Domain.Content;
using Coslen.RogueTiler.Domain.Engine;
using Coslen.RogueTiler.Domain.Engine.Actions;
using Coslen.RogueTiler.Domain.Engine.Entities.Heroes.Commands;
using Coslen.RogueTiler.Domain.Engine.Items;
using Coslen.RogueTiler.Domain.Engine.Items.Commands;
using Coslen.RogueTiler.Domain.Utilities;
using Coslen.RogueTiler.Win.Utilities;
using Coslen.RogueTiler.Win.Utilities.BufferUtilities;
using Debugger = System.Diagnostics.Debugger;

namespace Coslen.RogueTiler.Win.UI.InGame.Dialogs
{
    public enum StorageLocation
    {
        BackPack = 0,
        Inventory = 1,
        Equipment = 2,
        Crucible = 3
    }

    public enum StorageLocationAction
    {
        None = 0,
        Transfer = 1
    }


    /// Modal dialog for letting the user perform an [Action] on an [Item]
    /// accessible to the [Hero].
    public class StorageDialog : DialogBase
    {
        public StorageLocation LeftPane { get; set; }
        public StorageLocation RightPane { get; set; }

        public bool RightHasFocus { get; set; }

        public StorageDialog(Game game) :
            this("StorageDialog", 2, 5, Console.WindowHeight - 5, Console.WindowWidth - 2, -1, game)
        { }

        public StorageDialog(string key, int left, int top, int bottom, int right, int renderOrder, Game game)
            : base(key, left, top, bottom, right, renderOrder)
        {
            Game = game;

            LeftPane = StorageLocation.BackPack;
            RightPane = StorageLocation.Inventory;
        }

        public Game Game { get; }

        /// If the crucible contains a complete recipe, this will be it. Otherwise,
        /// this will be `null`.
        private Recipe completeRecipe;

        private StorageLocationAction currentAction;

        public override void Draw(BufferContainer buffer)
        {

            buffer.Clear();
            FrameArea(buffer);

            DrawLeftPane(buffer);
            DrawRightPane(buffer);

            var gold = PriceString(Game.Hero.Gold);
            WriteAt(buffer, 1, MaxHeight - 3, "Gold:");
            WriteAt(buffer, 7, MaxHeight - 3, gold, ConsoleColor.DarkYellow);

            // Options for Item Dialog
            var helpText = "";

            if (currentAction == StorageLocationAction.None)
            {
                var transferVerb = "Transfer";
                helpText = $"[↔] Select column, [↕] Select source, [{transferVerb[0]}] {transferVerb}, [Esc] Exit";
            }

            if (currentAction != StorageLocationAction.None)
            {
                helpText = $"[A-Z] Select item, [Esc/T] Cancel tansfer";
            }

            WriteAt(buffer, 1, MaxHeight - 2, $"{helpText}", ConsoleColor.DarkGray);
        }

        private void DrawRightPane(BufferContainer buffer)
        {
            int xOffset = ((Right - Left) / 2);
            WriteAt(buffer, xOffset, 0, $"{RightPane.ToString()}");

            DrawItems(buffer, xOffset, 2, GetItems(RightPane), RightPane, LeftPane, RightHasFocus);
        }

        private void DrawLeftPane(BufferContainer buffer)
        {
            WriteAt(buffer, 0, 0, $"{LeftPane.ToString()}");

            DrawItems(buffer, 0, 2, GetItems(LeftPane), LeftPane, RightPane, !RightHasFocus);
        }

        private void DrawItems(BufferContainer buffer, int x, int y, List<Item> items, StorageLocation current, StorageLocation target, bool isActive = false)
        {
            var i = 0;
            foreach (var item in items)
            {
                var alphabet = "abcdefghijklmnopqrstuvwxyz";
                var itemY = i + y;

                var borderColor = ConsoleColor.DarkGray;
                var letterColor = ConsoleColor.DarkGray;
                var textColor = ConsoleColor.DarkGray;
                var priceColor = ConsoleColor.DarkGray;
                var glyphColor = ConsoleColor.DarkGray;
                var attackColor = ConsoleColor.DarkGray;
                var armourColor = ConsoleColor.DarkGray;

                var enabled = true;

                if (isActive)
                {
                    switch (target)
                    {
                        case StorageLocation.Equipment:
                            {
                                var command = new EquipItemCommand();
                                var canUse = command.CanSelect(item);
                                if (canUse)
                                {
                                    borderColor = ConsoleColor.Gray;
                                    letterColor = ConsoleColor.Yellow;
                                    textColor = item == null ? ConsoleColor.Gray : ConsoleColor.White;
                                    priceColor = ConsoleColor.DarkYellow;
                                    glyphColor = item == null
                                        ? ConsoleColor.Gray
                                        : ColourUtilities.ConvertToConsoleColor(item.Appearance.ForeGroundColor);
                                    attackColor = ConsoleColor.Yellow;
                                    armourColor = ConsoleColor.Green;
                                }
                                break;
                            }
                        case StorageLocation.Crucible:
                            {
                                var canUse = false;
                                if (item != null)
                                {
                                    canUse = CanUseItemInRecipe(item);
                                }
                                if (canUse)
                                {
                                    borderColor = ConsoleColor.Gray;
                                    letterColor = ConsoleColor.Yellow;
                                    textColor = item == null ? ConsoleColor.Gray : ConsoleColor.White;
                                    priceColor = ConsoleColor.DarkYellow;
                                    glyphColor = item == null
                                        ? ConsoleColor.Gray
                                        : ColourUtilities.ConvertToConsoleColor(item.Appearance.ForeGroundColor);
                                    attackColor = ConsoleColor.Yellow;
                                    armourColor = ConsoleColor.Green;
                                }
                                break;
                            }
                        default:
                            {
                                borderColor = ConsoleColor.Gray;
                                letterColor = ConsoleColor.Yellow;
                                textColor = item == null ? ConsoleColor.Gray : ConsoleColor.White;
                                priceColor = ConsoleColor.DarkYellow;
                                glyphColor = item == null
                                    ? ConsoleColor.Gray
                                    : ColourUtilities.ConvertToConsoleColor(item.Appearance.ForeGroundColor);
                                attackColor = ConsoleColor.Yellow;
                                armourColor = ConsoleColor.Green;
                                break;
                            }
                    }
                }

                WriteAt(buffer, x, itemY, " )                                               ", borderColor);
                WriteAt(buffer, x, itemY, alphabet[i].ToString(), letterColor);

                if (item == null)
                {
                    // what is the location?
                    if (current == StorageLocation.Equipment)
                    {
                        var text = ((EquipementSlot)i) + " slot is empty";
                        WriteAt(buffer, x + 3, itemY, text, textColor);
                    }
                }
                else
                {
                    if (enabled)
                    {
                        WriteAt(buffer, x + 3, itemY, item.Appearance.Glyph, glyphColor);
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
                        DrawStat(buffer, x, itemY, "»", item.attack.AverageDamage, attackColor, attackColor, enabled);
                    }
                    else if (item.armor != 0)
                    {
                        DrawStat(buffer, x, itemY, "•", item.armor, armourColor, armourColor, enabled);
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

            // If this is the crucible then maybe a recipe has been completed.
            if (current == StorageLocation.Crucible)
            {
                if (completeRecipe != null)
                {
                    i++;
                    i++;

                    var textColour = ConsoleColor.Yellow;
                    if (isActive)
                    {
                        textColour = ConsoleColor.DarkGray;
                    }
                    var csv = string.Join(", ", completeRecipe.Produces.ToArray() );
                    WriteAt(buffer, 0, y + i++, $"This recipe {csv}!", textColour);
                    WriteAt(buffer, 0, y + i++, "Press[Space] to forge item!", textColour);
                }
            }
        }

        private bool CanUseItemInRecipe(Item item)
        {
            bool canUse = false;
            foreach (var recipe in GameContent.Instance.Recipes)
            {
                var currentItems = new List<Item>();
                currentItems.Add(item);
                currentItems.AddRange(GetItems(StorageLocation.Crucible));

                canUse = canUse || recipe.Allows(currentItems);
            }
            return canUse;
        }

        private void DrawStat(BufferContainer buffer, int x, int y, string symbol, object stat, ConsoleColor light, ConsoleColor dark, bool enabled)
        {
            var statString = stat.ToString();

            WriteAt(buffer, x + 47 - statString.Length, y, symbol, enabled ? dark : ConsoleColor.DarkGray);
            WriteAt(buffer, x + 49 - statString.Length, y, statString, enabled ? light : ConsoleColor.DarkGray);
        }

        protected override bool HandleInput()
        {
            do
            {
                var input = GetPlayerInput();

                if (input == Inputs.cancel)
                {
                    // cancelling a command.
                    if (currentAction != StorageLocationAction.None)
                    {
                        currentAction = StorageLocationAction.None;
                        return false;
                    }
                    return true;
                }

                // Switch Columns
                if (input == Inputs.e && currentAction != StorageLocationAction.Transfer)
                {
                    RightHasFocus = true;
                    return false;
                }

                if (input == Inputs.w && currentAction != StorageLocationAction.Transfer)
                {
                    RightHasFocus = false;
                    return false;
                }

                // Switch View in Column
                if (input == Inputs.n && currentAction != StorageLocationAction.Transfer)
                {
                    SwitchToNextView();
                    return false;
                }

                if (input == Inputs.s && currentAction != StorageLocationAction.Transfer)
                {
                    SwitchToPreviousView();
                    return false;
                }

                // Verbs
                if (input.Key == ConsoleKey.T)
                {
                    if (currentAction == StorageLocationAction.None)
                    {
                        currentAction = StorageLocationAction.Transfer;
                        return false;
                    }
                    else
                    {
                        currentAction = StorageLocationAction.None;
                        return false;
                    }
                }

                if (currentAction == StorageLocationAction.Transfer)
                {
                    if (ConvertKeyToNumber(input) != -1)
                    {
                        SelectItem(ConvertKeyToNumber(input), Game);

                        if (RightPane == StorageLocation.Crucible || LeftPane == StorageLocation.Crucible)
                        {
                            refreshRecipe();
                        }

                        return false;
                    }
                }

                if (input.Key == ConsoleKey.Spacebar)
                {
                    if (RightPane == StorageLocation.Crucible || LeftPane == StorageLocation.Crucible)
                    {
                        CraftItem();
                        return false;
                    }
                }

            } while (true);
        }

        public int ConvertKeyToNumber(ConsoleKeyInfo key)
        {
            var alphabet = "abcdefghijklmnopqrstuvwxyz";

            return alphabet.IndexOf(key.KeyChar);
        }

        private void SwitchToPreviousView()
        {
            var currentRight = (int)RightPane;
            int currentLeft = (int)LeftPane;
            if (RightHasFocus)
            {
                currentRight = currentRight + 1;

                if (currentRight == currentLeft)
                {
                    currentRight = currentRight + 1;
                }

                if (currentRight > 3)
                {
                    currentRight = 0;
                    // This is to cover the reset of the above statement
                    if (currentRight == currentLeft)
                    {
                        currentRight = currentRight + 1;
                    }
                }

                RightPane = (StorageLocation)currentRight;
            }
            else
            {
                currentLeft = currentLeft + 1;

                if (currentRight == currentLeft)
                {
                    currentLeft = currentLeft + 1;
                }

                if (currentLeft > 3)
                {
                    currentLeft = 0;
                    // This is to cover the reset of the above statement
                    if (currentRight == currentLeft)
                    {
                        currentLeft = currentLeft + 1;
                    }
                }

                LeftPane = (StorageLocation)currentLeft;
            }
        }

        private void SwitchToNextView()
        {
            var currentRight = (int)RightPane;
            int currentLeft = (int)LeftPane;

            if (RightHasFocus)
            {
                currentRight = currentRight - 1;

                if (currentRight == currentLeft)
                {
                    currentRight = currentRight - 1;
                }

                if (currentRight < 0)
                {
                    currentRight = 3;
                    if (currentRight == currentLeft)
                    {
                        currentRight = currentRight - 1;
                    }
                }

                RightPane = (StorageLocation)currentRight;
            }
            else
            {
                currentLeft = currentLeft - 1;
                if (currentRight == currentLeft)
                {
                    currentLeft = currentLeft - 1;
                }

                if (currentLeft < 0)
                {
                    currentLeft = 3;
                    if (currentRight == currentLeft)
                    {
                        currentLeft = currentLeft - 1;
                    }
                }
                LeftPane = (StorageLocation)currentLeft;
            }
        }

        private bool SelectItem(int index, Game game)
        {
            var activeLocation = RightHasFocus ? RightPane : LeftPane;
            var targetLocation = !RightHasFocus ? RightPane : LeftPane;

            var items = GetItems(activeLocation);
            if (index >= items.Count)
            {
                return false;
            }

            switch (targetLocation)
            {
                case StorageLocation.Equipment:
                    {
                        var equipItemCommand = new EquipItemCommand();
                        if (!equipItemCommand.CanSelect(items[index]))
                        {
                            return false;
                        }
                        else
                        {
                            var targetItem = items[index];
                            var unequipedItem = Game.Hero.Equipment.equip(targetItem);
                            if (unequipedItem != null)
                            {
                                RemoveTransferedItem(index, activeLocation);
                                TryToTransfer(unequipedItem, activeLocation, true);
                                return false;
                            }
                            else
                            {
                                RemoveTransferedItem(index, activeLocation);
                                return false;
                            }
                        }
                        return false;
                    }
                case StorageLocation.Crucible:
                    {
                        Item targetItem = GetTargetItem(index, activeLocation);
                        var canUse = CanUseItemInRecipe(targetItem);
                        if (!canUse)
                        {
                            return false;
                        }

                        if (TryToTransfer(targetItem, targetLocation, activeLocation == StorageLocation.Equipment))
                        {
                            RemoveTransferedItem(index, activeLocation);
                            return false;
                        }
                        else
                        {
                            return false;
                        }
                    }
                default:
                    {
                        Item targetItem = GetTargetItem(index, activeLocation);
                        if (TryToTransfer(targetItem, targetLocation, activeLocation == StorageLocation.Equipment))
                        {
                            RemoveTransferedItem(index, activeLocation);
                            return false;
                        }
                        else
                        {
                            return false;
                        }
                    }
            }
        }

        private void RemoveTransferedItem(int index, StorageLocation location)
        {
            switch (location)
            {
                case StorageLocation.Inventory:
                    {
                        Game.Hero.Inventory.RemoveAt(index);
                        break;
                    }
                case StorageLocation.Equipment:
                    {
                        Game.Hero.Equipment.removeAt((EquipementSlot)index);
                        break;
                    }
                case StorageLocation.BackPack:
                    {
                        Game.Hero.BackPack.RemoveAt(index);
                        break;
                    }
                case StorageLocation.Crucible:
                    {
                        Game.Hero.Crucible.RemoveAt(index);
                        break;
                    }
                default:
                    {
                        throw new ApplicationException("Invalid Location requested");
                    }
            }


        }

        private Item GetTargetItem(int index, StorageLocation location)
        {
            switch (location)
            {
                case StorageLocation.Inventory:
                    {
                        return Game.Hero.Inventory[index];
                    }
                case StorageLocation.Equipment:
                    {
                        return Game.Hero.Equipment[index];
                    }
                case StorageLocation.BackPack:
                    {
                        return Game.Hero.BackPack[index];
                    }
                case StorageLocation.Crucible:
                    {
                        return Game.Hero.Crucible[index];
                    }
            }

            throw new ApplicationException("Invalid Location requested");
        }

        private bool TryToTransfer(Item item, StorageLocation location, bool wasEquiped)
        {
            switch (location)
            {
                case StorageLocation.Inventory:
                    {
                        return Game.Hero.Inventory.tryAdd(item, wasEquiped);
                    }
                case StorageLocation.Equipment:
                    {
                        return Game.Hero.Equipment.tryAdd(item);
                    }
                case StorageLocation.BackPack:
                    {
                        return Game.Hero.BackPack.tryAdd(item, wasEquiped);
                    }
                case StorageLocation.Crucible:
                    {
                        return Game.Hero.Crucible.tryAdd(item, wasEquiped);
                    }
            }

            throw new ApplicationException("Invalid Location requested");
        }

        private List<Item> GetItems(StorageLocation location)
        {
            switch (location)
            {
                case StorageLocation.Inventory:
                    {
                        return Game.Hero.Inventory;
                    }
                case StorageLocation.Equipment:
                    {
                        return Game.Hero.Equipment.Items;
                    }
                case StorageLocation.BackPack:
                    {
                        return Game.Hero.BackPack;
                    }
                case StorageLocation.Crucible:
                    {
                        return Game.Hero.Crucible;
                    }
            }

            throw new ApplicationException("Should not be able to get to here!");
        }

        /// Converts an integer to a comma-grouped string like "123,456".
        private string PriceString(int price)
        {
            var result = price.ToString();
            if (price > 999999999)
            {
                result = result.Substring(0, result.Length - 9) + "," +
                    result.Substring(result.Length - 9);
            }

            if (price > 999999)
            {
                result = result.Substring(0, result.Length - 6) + "," +
                    result.Substring(result.Length - 6);
            }

            if (price > 999)
            {
                result = result.Substring(0, result.Length - 3) + "," +
                    result.Substring(result.Length - 3);
            }

            return result;
        }

        /// Sees if the crucible currently contains a complete recipe.
        private void refreshRecipe()
        {
            foreach (var recipe in GameContent.Instance.Recipes)
            {
                if (recipe.IsComplete(GetItems(StorageLocation.Crucible)))
                {
                    completeRecipe = recipe;
                    return;
                }
            }

            completeRecipe = null;
        }

        private void CraftItem()
        {
            if (completeRecipe != null)
            {
                Game.Hero.Crucible.Clear();
                completeRecipe.Result.SpawnDrop(Game.Hero.Crucible.tryAdd);
                completeRecipe = null;
                refreshRecipe();
            }
        }
    }
}