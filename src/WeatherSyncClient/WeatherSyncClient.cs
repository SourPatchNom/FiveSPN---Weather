using System;
using System.Collections.Generic;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using WeatherSyncClient.Services;

namespace WeatherSyncClient
{
public sealed class WeatherSyncClient : BaseScript
    {
        public WeatherSyncClient()
        {
            TriggerEvent("FiveSPN-LogToClient", API.GetCurrentResourceName(),3,"Initializing");
            EventHandlers.Add("playerSpawned", new Action<Vector3>(OnPlayerSpawned));
            EventHandlers["FiveSPN-UpdateClientWx"] += new Action<string>(ReceiveWeather);
            EventHandlers["FiveSPN-ReplyToClientWx"] += new Action<string>(ReceiveReply);
            Tick += ClientWeatherService.Instance.WeatherTick;
            RegisterWxCommands();
        }

        /// <summary>
        /// Requests an updated weather from the server when a player spawns.
        /// </summary>
        /// <param name="a"></param>
        private static void OnPlayerSpawned([FromSource]Vector3 a)
        {
            TriggerServerEvent("FiveSPN-WX-RequestUpdate");
        }

        /// <summary>
        /// Receives updates from the server for world weather.
        /// </summary>
        /// <param name="jsonString"></param>
        private static void ReceiveWeather(string jsonString)
        {
            WorldWeatherService.Instance.UpdateWorldWeatherStates(jsonString);
        }

        /// <summary>
        /// Sends a chat message with a reply from the server.
        /// </summary>
        /// <param name="reply"></param>
        private static void ReceiveReply(string reply)
        {
            SendDefaultChatMessage(reply);
        }
        
        /// <summary>
        /// Sends a chat message using the default chat system.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="c"></param>
        private static void SendDefaultChatMessage(string message, int[] c = null)
        {
            TriggerEvent("chat:addMessage", new
            {
                color = c ?? new[] { 0, 100, 255 },
                multiline = true,
                args = new[] { "SPNWeather: ", message }
            });
        }
        
        /// <summary>
        /// Sends a chat message with all /Weather options
        /// </summary>
        private static void SendWeatherOptions()
        {
            SendDefaultChatMessage(@"
/Weather REFRESH - Forces a server wide refresh of weather.
/Weather RESET - Resumes using real world sync.
/Weather CLEAR - Sets weather to clear.
/Weather THUNDER - Sets weather to thunderstorms.
/Weather RAIN - Sets weather to rain.
/Weather SNOWLIGHT - Sets weather to light snow.
/Weather SNOW - Sets weather to snow.
/Weather BLIZZARD - Sets weather to blizzard.
/Weather FOGGY - Sets weather to foggy.
/Weather EXTRASUNNY - Sets weather to extra sunny.
/Weather CLOUDS - Sets weather to clouds.
/Weather OVERCAST - Sets weather to overcast.
/Weather SMOG - Sets weather to smog.
/Weather HALLOWEEN - Sets weather to halloween.
", new int[] { 0, 100, 255 });
        }

         /// <summary>
         /// Register all the weather commands based on if a member has admin permissions or not.
         /// </summary>
         private static void RegisterWxCommands()
         {
             API.RegisterCommand("RefreshWeather", new Action<int, List<object>, string>((source, args, raw) =>
             {
                 TriggerServerEvent("FiveSPN-WX-RequestUpdate");
                 ClientWeatherService.Instance.ForceUpdate();
             }), false);
             
             API.RegisterCommand("Weather", new Action<int, List<object>, string>((source, args, raw) =>
             {
                 switch (args.Count)
                 {
                     case 0:
                         SendWeatherOptions();
                         break;
                     case 1:
                         switch (args[0])
                         {
                             case "REFRESH":
                                 TriggerServerEvent("FiveSPN-WX-RequestUpdate");
                                 SendDefaultChatMessage($"Attempting to refresh weather.");
                                 break;
                             case "RESET":
                                 TriggerServerEvent("FiveSPN-WX-ClearOverride");
                                 break;
                             case "CLEAR":
                             case "THUNDER":
                             case "RAIN":
                             case "SNOWLIGHT":
                             case "SNOW":
                             case "BLIZZARD":
                             case "FOGGY":
                             case "EXTRASUNNY":
                             case "CLOUDS":
                             case "OVERCAST":
                             case "SMOG":
                                 TriggerServerEvent("FiveSPN-WX-SetOverride", args[0]);
                                 SendDefaultChatMessage($"Attempting to override weather sync to {args[0]}");
                                 break;
                             default:
                                 SendDefaultChatMessage("Wrong Command! Type /Weather to see all commands!", new int[] { 255, 0, 0 });
                                 break;
                         }

                         break;
                 }
             }), false);
         }
    }
}