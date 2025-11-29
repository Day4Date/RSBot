using RSBot.Python.Components.API.Interface;
using RSBot.Python.Views;
using System;
using System.Linq;
using Python.Runtime;
using RSBot.Core;

namespace RSBot.Python.Components.API.Core
{
    /// <summary>
    /// Dieses Modul stellt Kernfunktionen für Python zur Verfügung,
    /// z.B. Logging, Infos, Charakterdaten und Start/Stop.
    /// </summary>
    public class CoreAPI : IPythonPlugin
    {
        // Referenz auf die Haupt-main (main1),
        // damit wir z.B. ins Log schreiben können.
        private Main _main;

        /// <summary>
        /// Der eindeutige Name dieses Plugins.
        /// Diesen Namen können wir später z.B. im ModuleLoader oder in Python nutzen.
        /// </summary>
        public string ModuleName => "core";

        /// <summary>
        /// Diese Methode wird beim Start einmal aufgerufen,
        /// damit das Plugin die main (main1) bekommt.
        /// </summary>
        /// <param name="main">Die Hauptmain der Anwendung.</param>
        public void Init(Main main)
        {
            _main = main;
        }

        private bool StartBot()
        {
            try
            {
                if (Kernel.Proxy == null)
                    return false;

                if (!Kernel.Proxy.IsConnectedToAgentserver)
                    return false;

                if (Kernel.Bot == null)
                {
                    return false;
                }

                if (Game.Player == null)
                {
                    return false;
                }

                if (!Kernel.Bot.Running)
                {
                    Kernel.Bot.Start();
                    return true;
                }
                return false;

            }
            catch (Exception ex)
            {
                Log.Error($"Error while trying to start the bot from Plugin: {ex}" );
                return false;
            }
        }
        private bool StopBot()
        {
            try
            {
                if (Kernel.Proxy == null)
                    return false;

                if (!Kernel.Proxy.IsConnectedToAgentserver)
                    return false;

                if (Kernel.Bot == null)
                {
                    return false;
                }

                if (Game.Player == null)
                {
                    return false;
                }

                if (Kernel.Bot.Running)
                {
                    Kernel.Bot.Stop();

                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"Error while trying to stop the bot from Plugin: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Schreibt eine Nachricht ins Log-Fenster.
        /// </summary>
        public void log(params object[] args)
        {
            if (args == null || args.Length == 0)
            {
                _main?.AppendLog(string.Empty);
                return;
            }

            string msg = string.Join(" ", args.Select(a =>
            {
                if (a == null) return "None";

                if (a is PyObject pyObj)
                {
                    using (Py.GIL())
                    {
                        return pyObj.ToString();
                    }
                }
                return a.ToString();
            }));

            _main?.AppendLog(msg);
        }
        /// <summary>
        /// Beispiel-Start-Funktion.
        /// </summary>
        public bool start_bot()
        {
            return StartBot();
        }

        /// <summary>
        /// Beispiel-Stop-Funktion.
        /// </summary>
        public bool stop_bot()
        {
            return StopBot();
        }
    }
}
