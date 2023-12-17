using System;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using WindowsGSM.Functions;
using WindowsGSM.GameServer.Engine;
using WindowsGSM.GameServer.Query;
using System.IO;

namespace WindowsGSM.Plugins
{
    public class Wreckfest : SteamCMDAgent
    {
        // - Plugin Details
        public Plugin Plugin = new Plugin
        {
            name = "WindowsGSM.Wreckfest",
            author = "Soul",
            description = "\U0001f9e9 WindowsGSM plugin that provides Wreckfest Dedicated server support!",
            version = "1.0",
            url = "https://github.com/Soulflare3/WindowsGSM.Wreckfest",
            color = "#7a0101"
        };

        // - Standard Constructor and properties
        public Wreckfest(ServerConfig serverData) : base(serverData) => base.serverData = _serverData = serverData;

        private readonly ServerConfig _serverData;

        // - Settings properties for SteamCMD installer
        public override bool loginAnonymous => true;
        public override string AppId => "361580";
        
        // - Game server Fixed variables
        public override string StartPath => @"Wreckfest_x64.exe";
        public string FullName = "Wreckfest Dedicated Server";
        public bool AllowsEmbedConsole = false;
        public int PortIncrements = 1;
        public object QueryMethod = new A2S();

        // - Game server default values
        public string Port = "33540";
        public string ServerName = "Wreckfest_Dedicated_Server";
        public string QueryPort = "27016";
        public string Defaultmap = "speedway2_demolition_arena";
        public string Maxplayers = "24";
        public string Additional = "";

        // - Create a default cfg for the game server after installation
        public async void CreateServerCFG()
        {
            if (!File.Exists(ServerPath.GetServersServerFiles(_serverData.ServerID, "server_config.cfg")))
            {
                File.Copy(ServerPath.GetServersServerFiles(_serverData.ServerID, "initial_server_config.cfg"), ServerPath.GetServersServerFiles(_serverData.ServerID, "server_config.cfg"));
            }
        }

        // - Start server function, return its Process to WindowsGSM
        public async Task<Process> Start()
        {
            string exePath = Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, StartPath);
            if (!File.Exists(exePath))
            {
                Error = $"{Path.GetFileName(exePath)} not found ({exePath})";
                return null;
            }

            string param = $"-s server_config=server_config.cfg -server_set server_name={_serverData.ServerName} max_players={_serverData.ServerMaxPlayer} game_port={_serverData.ServerPort} query_port={_serverData.ServerQueryPort} track={_serverData.ServerMap} {_serverData.ServerParam}";

            // Prepare Process
            var p = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = ServerPath.GetServersServerFiles(_serverData.ServerID),
                    FileName = exePath,
                    Arguments = param.ToString(),
                    WindowStyle = ProcessWindowStyle.Normal,
                    UseShellExecute = false
                },
                EnableRaisingEvents = true
            };

            // Start Process
            try
            {
                p.Start();
                if (AllowsEmbedConsole)
                {
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                }

                return p;
            }
            catch (Exception e)
            {
                Error = e.Message;
                return null; // return null if fail to start
            }

        }

        // - Stop server function
        public async Task Stop(Process p)
        {

            await Task.Run(() =>
            {
                if (p.StartInfo.RedirectStandardInput)
                {
                    p.StandardInput.WriteLine("exit");
                }
                else
                {
                    ServerConsole.SendMessageToMainWindow(p.MainWindowHandle, "exit");
                }
             });
            await Task.Delay(5000);
        }
    }
}