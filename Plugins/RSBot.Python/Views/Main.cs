using Python.Runtime;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Network;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Spawn;
using RSBot.Python.Components.API.GUI;
using RSBot.Python.Components.API.ModuleLoader;
using RSBot.Python.Components.Loader;
using RSBot.Python.Plugins;
using SDUI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using static Python.Runtime.TypeSpec;

namespace RSBot.Python.Views;

[ToolboxItem(false)]
public partial class Main : DoubleBufferedControl
{
    //private Thread pythonThread;
    //private PeriodicTimer _pluginTimer;
    //private CancellationTokenSource _cts;
    //private List<PyObject> _loadedPlugins = new List<PyObject>();
    private readonly PythonRuntimeManager _pyRuntime = new();
    private readonly PythonPluginManager _pyPlugins = new();
    private string projectDir = Directory.GetParent(Application.StartupPath).FullName;
    public Main()
    {
        InitializeComponent();
        ModuleLoader.InitAll(this);
        PythonRuntimeManager.GenerateStub(Directory.GetParent(Application.StartupPath).FullName);
        WireEvents();
        SubscribeEvents();
        InitPythonRuntime();
    }
    private void SubscribeEvents()
    {
        EventManager.SubscribeEvent("OnSpawnPlayer", OnSpawnPlayer);
        EventManager.SubscribeEvent("OnTeleportComplete", OnTeleportComplete);
        EventManager.SubscribeEvent("OnTeleportStart", OnTeleportStart);
        EventManager.SubscribeEvent("OnClientPacketReceive", OnClientPacketReceive);
        EventManager.SubscribeEvent("OnServerPacketReceive", OnServerPacketReceive);
    }
    private void WireEvents()
    {
        dgvPlugin.CellValueChanged += (s, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 0) return;

            var row = dgvPlugin.Rows[e.RowIndex];
            bool isEnabled = Convert.ToBoolean(row.Cells[0].Value);
            string fileName = row.Cells[5].Value.ToString(); // Versteckte Spalte mit Dateinamen
            if (isEnabled)
            {
                AppendLog($"Aktiviere Plugin: {fileName}");
                RunPythonPlugin(fileName);
            }
            else
            {
                AppendLog($"Deaktiviere Plugin: {fileName}");
                ResetPythonPlugins();
            }
        };
    }
    public void AppendLog(string text)
    {
        if (InvokeRequired)
        {
            Invoke(new System.Action(() => AppendLog(text)));
            return;
        }

        tbPluginLog.AppendText(Environment.NewLine + text);
        //tbPluginLog.SelectionStart = tbPluginLog.TextLength;  // Caret ans Ende
        //tbPluginLog.SelectionLength = 0;
        tbPluginLog.ScrollToCaret();
    }




    #region PythonRuntime
    private void InitPythonRuntime()
    {


        _pyRuntime.Initialize(projectDir, AppendLog);

        // UI füllen
        ReloadPluginGrid();
    }
    private void ReloadPluginGrid()
    {
        dgvPlugin.Rows.Clear();

        var plugins = _pyPlugins.ScanPlugins(projectDir, AppendLog);

        foreach (var info in plugins)
        {
            bool enabled = false; // später aus Config
            dgvPlugin.Rows.Add(enabled, info.Name, info.Description, info.Author, info.Version, info.FileName);
        }
    }
    private void RunPythonPlugin(string fileName)
    {
        string projectDir = Directory.GetParent(Application.StartupPath).FullName;
        _pyPlugins.RunPlugin(projectDir, fileName, AppendLog);
    }

    private void ResetPythonPlugins()
    {
        _pyPlugins.ResetPlugins();
    }

    #endregion
    #region OldInit

    //    private void InitPythonRuntime()
    //    {
    //        // 1. Pfade setzen (gleich wie bisher)
    //        string projectDir = Directory.GetParent(Application.StartupPath).FullName;
    //        string pythonHome = Path.Combine(projectDir,"Data","Python", "PyRuntime");
    //        string pythonDll = Directory.GetFiles(pythonHome, "python31*.dll").FirstOrDefault();

    //        if (pythonDll == null)
    //        {
    //            AppendLog("Keine Python-DLL gefunden!");
    //            return;
    //        }

    //        Runtime.PythonDLL = pythonDll;
    //        PythonEngine.PythonHome = pythonHome;
    //        PythonEngine.PythonPath = pythonHome + ";" + Path.Combine(pythonHome, "Lib");

    //        PythonEngine.Initialize();
    //        PythonEngine.BeginAllowThreads();

    //        // 2. API-Namespace nach Python exportieren
    //        using (Py.GIL())
    //        {
    //            try
    //            {
    //                PythonEngine.Exec(@"
    //import clr
    //import sys
    //import types

    //# C# Assembly laden
    //clr.AddReference('RSBot.Python')

    //# Richtige Accessor-Klasse importieren!
    //from RSBot.Python.API.ModuleLoader import PythonPluginAccessor

    //# Neues Modul erstellen
    //RSBot = types.ModuleType('RSBot')

    //# Plugin-Instanzen abrufen
    //plugins = PythonPluginAccessor.all()


    //# Export aller öffentlichen Methoden aller Plugins
    //for name, plugin in plugins.items():

    //    # Durch alle Attribute der Plugin-Klasse gehen
    //    for attr_name in dir(plugin):

    //        # interne/private Methoden überspringen
    //        if attr_name.startswith('_'):
    //            continue

    //        # Attribut abrufen
    //        attr = getattr(plugin, attr_name)

    //        # nur Funktionen exportieren
    //        if callable(attr):
    //            setattr(RSBot, attr_name, attr)

    //# Modul offiziell registrieren
    //sys.modules['RSBot'] = RSBot

    //from RSBot.Python.API.GUI import WFAPI
    //RSBot.GUI = WFAPI.GUI
    //");
    //                AppendLog("Python initialisiert und RSBot-Modul erstellt.");
    //            }
    //            catch (PythonException ex)
    //            {
    //                var formatted = PythonErrorHandler.FormatException(ex);
    //                AppendLog(formatted);          // ins Log
    //            }
    //            LoadPythonPlugins();

    //        }
    //    }
    //private void RunPythonPlugin(string fileName)
    //{
    //    Task.Run(() =>
    //    {
    //        try
    //        {
    //            string projectDir = Directory.GetParent(Application.StartupPath).FullName;
    //            string pluginsDir = Path.Combine(projectDir, "Data", "Python", "Plugins");
    //            string pluginPath = Path.Combine(pluginsDir, fileName);

    //            using (Py.GIL())
    //            {
    //                dynamic importlib = Py.Import("importlib.util");
    //                string moduleName = "plugin_" + Path.GetFileNameWithoutExtension(fileName);

    //                dynamic spec = importlib.spec_from_file_location(moduleName, pluginPath);
    //                dynamic module = importlib.module_from_spec(spec);
    //                spec.loader.exec_module(module);

    //                _loadedPlugins.Add(module);
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            AppendLog($"[Fehler] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
    //        }
    //    });
    //}
    //private void ResetPythonPlugins()
    //{
    //    if (_loadedPlugins.Count == 0)
    //        return;
    //    using (Py.GIL())
    //    {
    //        _loadedPlugins.Clear();
    //    }
    //}
    //private void LoadPythonPlugins()
    //{
    //    try
    //    {
    //        // Projektverzeichnis (z. B. bin/Debug/... → gehe 2 Ebenen hoch)
    //        string projectDir = Directory.GetParent(Application.StartupPath).FullName;


    //        // Plugin-Ordner
    //        string pluginDir = Path.Combine(projectDir, "Data", "Python", "Plugins");

    //        if (!Directory.Exists(pluginDir))
    //        {
    //            Directory.CreateDirectory(pluginDir);
    //        }

    //        // Alle .py-Dateien im Plugin-Ordner laden
    //        string[] pythonFiles = Directory.GetFiles(pluginDir, "*.py", SearchOption.TopDirectoryOnly);

    //        dgvPlugin.Rows.Clear();

    //        foreach (string file in pythonFiles)
    //        {
    //            // --- Metadaten statisch aus dem Datei-Text holen (KEIN Python-Exec!)
    //            PythonPluginInfo info = PluginMetaReader.ReadPythonPluginInfo(file);

    //            // --- Enabled Status über Dateiname bestimmen
    //            bool enabled = false;

    //            // --- Zeile hinzufügen
    //            // ACHTUNG: Reihenfolge muss exakt zu deinen Spalten passen
    //            dgvPlugin.Rows.Add(
    //                enabled,
    //                info.Name,
    //                info.Description,
    //                info.Author,
    //                info.Version,
    //                info.FileName   // <- nur wenn du eine versteckte ID-Spalte hast
    //            );
    //        }

    //        AppendLog($"Es wurden {pythonFiles.Length} Python-Plugins gefunden.");
    //    }
    //    catch (Exception ex)
    //    {
    //        MessageBox.Show("Fehler beim Laden der Plugins:\n" + ex.Message);
    //    }
    //}
    #endregion
    private void btnReload_Click(object sender, EventArgs e)
    {
        var gui = ModuleLoader.Get("gui") as WFAPI;
        if (gui != null)
        {
            gui.ResetAll();
        }
        ResetPythonPlugins();
        ReloadPluginGrid();
        string pluginFolder = Path.Combine(Directory.GetParent(Application.StartupPath).Parent.Parent.FullName, "Plugins");

    }

    private void OnSpawnPlayer(SpawnedPlayer player)
    {

    }
    private void OnTeleportStart()
    {
        _pyPlugins.CallPluginEvent("on_teleported", AppendLog, new PyInt(1));
    }
    private void OnTeleportComplete()
    {
        _pyPlugins.CallPluginEvent("on_teleported", AppendLog, new PyInt(2));
    }
    private void OnClientPacketReceive(Packet packet)
    {
        byte[] data = packet.GetBytes();
        string opcode = packet.HexCode;
        string data_hex = BitConverter.ToString(data).Replace("-", " ");
        _pyPlugins.CallPluginEvent("on_client_received", AppendLog, opcode,data_hex);
    }
    private void OnServerPacketReceive(Packet packet)
    {
        byte[] data = packet.GetBytes();
        string opcode = packet.HexCode;
        string data_hex = BitConverter.ToString(data).Replace("-", " ");
        _pyPlugins.CallPluginEvent("on_server_received", AppendLog, opcode,data_hex);
    }

    private void btnOpenFolder_Click(object sender, EventArgs e)
    {
        if (SpawnManager.TryGetEntities<SpawnedPlayer>(out var players))
        {
            foreach (var p in players)
            {
                AppendLog($"Name: {p.Name}, x: {p.Position.X}, x: {p.Position.Y}, UID: {p.UniqueId}");
                foreach (var entry in p.Inventory)
                {
                    var key = entry.Key;       // RefObjItem
                    var value = entry.Value;   // byte (plus value)

                    AppendLog($"{key.CodeName} +{value}");
                }
            }    
                
        }



    }
}