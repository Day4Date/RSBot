using Python.Runtime;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Extensions;
using RSBot.Core.Objects.Spawn;
using RSBot.Python.Components.API.Interface;
using RSBot.Python.Views;
using System.Collections.Generic;

namespace RSBot.Python.Components.API.Core.Entity
{
    public class InventoryAPI : IPythonPlugin
    {
        private Main _main;

        /// <summary>
        /// Eindeutiger Name des Plugins.
        /// </summary>
        public string ModuleName => "entity";

        /// <summary>
        /// Init wird einmal aufgerufen, um die main zu übergeben.
        /// </summary>
        public void Init(Main main)
        {
            _main = main;
        }
        private PyList BuildMonsterList(IEnumerable<SpawnedMonster> monster)
        {
            var list = new PyList();

            foreach (var entry in monster)
            {
                if (entry == null)
                    continue;

                var pyItem = new PyDict();
                pyItem.SetItem(new PyString("uid"), new PyInt(entry.UniqueId));
                pyItem.SetItem(new PyString("model"), new PyInt(entry.Id));
                pyItem.SetItem(new PyString("servername"), new PyString(entry.Record.CodeName));
                pyItem.SetItem(new PyString("name"), new PyString(entry.Record.GetRealName()));
                pyItem.SetItem(new PyString("type"), new PyString(entry.Rarity.GetName()));
                pyItem.SetItem(new PyString("x"), new PyString(entry.Position.X.ToString()));
                pyItem.SetItem(new PyString("y"), new PyString(entry.Position.Y.ToString()));
                pyItem.SetItem(new PyString("z"), new PyString(entry.Position.ZOffset.ToString()));
                pyItem.SetItem(new PyString("region"), new PyString(entry.Position.Region.ToString()));
                pyItem.SetItem(new PyString("region_name"), new PyString(Game.ReferenceManager.GetTranslation(entry.Position.Region.ToString())));
                pyItem.SetItem(new PyString("distance"), new PyString(entry.DistanceToPlayer.ToString()));
                pyItem.SetItem(new PyString("hp"), new PyString(entry.Health.ToString()));
                pyItem.SetItem(new PyString("max_hp"), new PyString(entry.MaxHealth.ToString()));                
                list.Append(pyItem);
            }

            return list;
        }
        private PyList BuildNPCList(IEnumerable<SpawnedNpc> npc)
        {
            var list = new PyList();

            foreach (var entry in npc)
            {
                if (entry == null)
                    continue;

                var pyItem = new PyDict();
                pyItem.SetItem(new PyString("uid"), new PyInt(entry.UniqueId));
                pyItem.SetItem(new PyString("model"), new PyInt(entry.Id));
                pyItem.SetItem(new PyString("servername"), new PyString(entry.Record.CodeName));
                pyItem.SetItem(new PyString("name"), new PyString(entry.Record.GetRealName()));
                pyItem.SetItem(new PyString("x"), new PyString(entry.Position.X.ToString()));
                pyItem.SetItem(new PyString("y"), new PyString(entry.Position.Y.ToString()));
                pyItem.SetItem(new PyString("z"), new PyString(entry.Position.ZOffset.ToString()));
                pyItem.SetItem(new PyString("region"), new PyString(entry.Position.Region.ToString()));
                pyItem.SetItem(new PyString("region_name"), new PyString(Game.ReferenceManager.GetTranslation(entry.Position.Region.ToString())));
                pyItem.SetItem(new PyString("distance"), new PyString(entry.DistanceToPlayer.ToString()));
                list.Append(pyItem);
            }

            return list;
        }
        private PyList GetMonsters()
        {
            using (Py.GIL())
            {
                var result = new PyList();
                if (Game.Player == null)
                {
                    return result;
                }
                if (SpawnManager.TryGetEntities<SpawnedMonster>(out var monsters))
                {

                    result = BuildMonsterList(monsters);
                }
                return result;
            }
        }
        private PyList GetNPCs()
        {
            using (Py.GIL())
            {
                var result = new PyList();
                if (Game.Player == null)
                {
                    return result;
                }    
                if (SpawnManager.TryGetEntities<SpawnedNpc>(out var npc))
                {
                    result = BuildNPCList(npc);
                }
                return result;
            }
        }
        private PyDict GetCharacter()
        {
            using (Py.GIL())
            {
                var result = new PyDict();
                if (Game.Player == null)
                {
                    return result;
                }
                
                result.SetItem(new PyString("name"), new PyString(Game.Player.Name));
                result.SetItem(new PyString("uid"), new PyInt(Game.Player.UniqueId));
                result.SetItem(new PyString("model"), new PyInt(Game.Player.Id));
                result.SetItem(new PyString("x"), new PyFloat(Game.Player.Position.X));
                result.SetItem(new PyString("y"), new PyFloat(Game.Player.Position.Y));
                result.SetItem(new PyString("z"), new PyString(Game.Player.Position.ZOffset.ToString()));
                result.SetItem(new PyString("region"), new PyString(Game.Player.Position.Region.ToString()));
                result.SetItem(new PyString("region_name"), new PyString(Game.ReferenceManager.GetTranslation(Game.Player.Position.Region.ToString())));
                result.SetItem(new PyString("hp"), new PyInt(Game.Player.Health));
                result.SetItem(new PyString("mp"), new PyInt(Game.Player.Mana));
                result.SetItem(new PyString("max_hp"), new PyInt(Game.Player.MaximumHealth));
                result.SetItem(new PyString("max_mp"), new PyInt(Game.Player.MaximumMana));
                result.SetItem(new PyString("exp"), new PyInt(Game.Player.Experience));
                result.SetItem(new PyString("max_exp"), new PyInt(Game.ReferenceManager.GetRefLevel(Game.Player.Level).Exp_C));
                result.SetItem(new PyString("sp"), new PyInt(Game.Player.SkillPoints));
                result.SetItem(new PyString("level"), new PyInt(Game.Player.Level));
                result.SetItem(new PyString("berserker"), new PyInt(Game.Player.BerzerkPoints));
                return result;
            }
        }
        public PyList get_monsters()
        {
            return GetMonsters();
        }
        public PyList get_npcs()
        {
            return GetNPCs();
        }
        public PyDict get_character()
        {
            return GetCharacter();
        }
    }
}
