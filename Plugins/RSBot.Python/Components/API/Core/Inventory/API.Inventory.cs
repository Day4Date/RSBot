using Python.Runtime;
using RSBot.Core;
using RSBot.Core.Objects;
using RSBot.Python.Components.API.Interface;
using RSBot.Python.Views;
using System.Collections.Generic;

namespace RSBot.Python.Components.API.Core.Inventory
{
    /// <summary>
    /// Dieses Plugin stellt Inventar-Funktionen für Python bereit,
    /// z.B. Items verschieben, Inventar aktualisieren, usw.
    /// </summary>
    public class InventoryAPI : IPythonPlugin
    {
        private Main _main;

        /// <summary>
        /// Eindeutiger Name des Plugins.
        /// </summary>
        public string ModuleName => "inventory";

        /// <summary>
        /// Init wird einmal aufgerufen, um die main zu übergeben.
        /// </summary>
        public void Init(Main main)
        {
            _main = main;
        }
        private PyList BuildItemList(IEnumerable<InventoryItem> items)
        {
            var list = new PyList();

            foreach (var item in items)
            {
                if (item == null)
                    continue;

                var pyItem = new PyDict();
                pyItem.SetItem(new PyString("model"), new PyInt(item.ItemId));
                pyItem.SetItem(new PyString("servername"), new PyString(item.Record.CodeName));
                pyItem.SetItem(new PyString("name"), new PyString(item.Record.GetRealName()));
                pyItem.SetItem(new PyString("quantity"), new PyInt(item.Amount));
                pyItem.SetItem(new PyString("plus"), new PyInt(item.OptLevel));
                pyItem.SetItem(new PyString("rarity"), new PyString(item.Record.GetRarityName()));
                pyItem.SetItem(new PyString("durability"), new PyInt(item.Durability));
                pyItem.SetItem(new PyString("slot"), new PyInt(item.Slot));

                list.Append(pyItem);
            }

            return list;
        }
        private PyDict GetInventory()
        {
            using (Py.GIL())
            {
                var result = new PyDict();

                if (Game.Player == null || Game.Player.Inventory == null)
                {
                    result.SetItem(new PyString("size"), new PyInt(0));
                    result.SetItem(new PyString("gold"), new PyString("0"));
                    result.SetItem(new PyString("equipped"), new PyList());
                    result.SetItem(new PyString("inventory"), new PyList());
                    result.SetItem(new PyString("avatar"), new PyList());
                    return result;
                }
                var inventorySize = Game.Player.Inventory.Capacity;
                var gold = Game.Player.Gold;
                result.SetItem(new PyString("size"), new PyInt(inventorySize));
                result.SetItem(new PyString("gold"), new PyString(gold.ToString()));                

                var itemsEquipped = Game.Player.Inventory.GetEquippedPartItems();
                var itemsInventory = Game.Player.Inventory.GetNormalPartItems();
                var itemsAvatar = Game.Player.Avatars;

                var pyEquippedList = BuildItemList(itemsEquipped);
                var pyInventoryList = BuildItemList(itemsInventory);
                var pyAvatarList = BuildItemList(itemsAvatar);

                result.SetItem(new PyString("equipped"), pyEquippedList);
                result.SetItem(new PyString("items"), pyInventoryList);
                result.SetItem(new PyString("avatar"), pyAvatarList);

                return result;
            }
        }
        private PyList GetStorage()
        {
            using (Py.GIL())
            {
                var result = new PyList();

                if (Game.Player == null || Game.Player.Storage == null)
                {
                    return result;
                }
                var storage = Game.Player.Storage;
                result = BuildItemList(storage);

                return result;

            }
        }
        private PyList GetGuildStorage()
        {
            using (Py.GIL())
            {
                var result = new PyList();

                if (Game.Player == null || Game.Player.GuildStorage == null)
                {
                    return result;
                }
                var storage = Game.Player.GuildStorage;
                result = BuildItemList(storage);

                return result;

            }
        }
        private PyList GetPetStorage()
        {
            using (Py.GIL())
            {
                var result = new PyList();

                if (Game.Player == null || !Game.Player.HasActiveAbilityPet)
                {
                    return result;
                }
                var storage = Game.Player.AbilityPet.Inventory;
                result = BuildItemList(storage);

                return result;

            }
        }
        private PyDict GetJobPouch()
        {
            using (Py.GIL())
            {
                var result = new PyDict();

                if (Game.Player == null || Game.Player.Job2SpecialtyBag == null)
                {
                    return result;
                }
                var storage = Game.Player.Job2SpecialtyBag;
                result.SetItem(new PyString("size"), new PyInt(storage.Capacity));
                result.SetItem(new PyString("free_slots"), new PyInt(storage.FreeSlots));
                var list = new PyList();

                foreach (var item in storage)
                {
                    if (item == null)
                        continue;

                    var pyItem = new PyDict();
                    pyItem.SetItem(new PyString("servername"), new PyString(item.Record.CodeName));
                    pyItem.SetItem(new PyString("name"), new PyString(item.Record.GetRealName()));
                    pyItem.SetItem(new PyString("quantity"), new PyInt(item.Amount));
                    pyItem.SetItem(new PyString("slot"), new PyInt(item.Slot));

                    list.Append(pyItem);
                }
                result.SetItem(new PyString("items"), list);
                return result;

            }
        }
        public PyDict get_inventory()
        {
            return GetInventory();
        }
        public PyList get_storage()
        {
            return GetStorage();
        }
        public PyList get_guild_storage()
        {
            return GetGuildStorage();
        }
        public PyDict get_job_pouch()
        {
            return GetJobPouch();
        }
        public PyList get_pet_storage()
        {
            return GetPetStorage();
        }
    }
}
