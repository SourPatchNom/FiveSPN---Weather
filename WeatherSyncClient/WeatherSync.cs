using System;
using System.Collections.Generic;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace WeatherSyncClient
{
public class WeatherSync : BaseScript
    {
        private Weather wxCurrent = Weather.Clear;
        private Weather wxNew = Weather.Clear;
        private string wxLocation = "NONE";
        private bool wxIsChanging = false;

        /// <summary>
        /// Requests an updated weather from the server when a player spawns.
        /// </summary>
        /// <param name="a"></param>
        public void OnPlayerSpawned([FromSource]Vector3 a)
        {
            TriggerServerEvent("SPNWeatherRequestUpdate");
        }

        /// <summary>
        /// Syncs the players weather if it is needed. 
        /// </summary>
        private void SyncPlayerWeather()
        {
            if (World.Weather != wxNew && !wxIsChanging)
            {
                Debug.WriteLine("SPNWeather: Changing weather to " + wxNew.ToString() + " from " + World.Weather.ToString() + " which is synced from " + wxLocation);
                wxIsChanging = true;
                wxCurrent = wxNew;
                World.TransitionToWeather(wxCurrent, 45.0f);
            }
            else if (World.Weather == wxCurrent && wxIsChanging)
            {
                wxIsChanging = false;
            }
        }

        /// <summary>
        /// Recieve the weather from the server.
        /// </summary>
        /// <param name="newWX"></param>
        /// <param name="location"></param>
        public void RecieveWeather(string newWX, string location)
        {
            wxLocation = location;
            switch (newWX)
            {
                case "THUNDER":
                    wxNew = Weather.ThunderStorm;
                    break;
                case "RAIN":
                    wxNew = Weather.Raining;
                    break;
                case "SNOWLIGHT":
                    wxNew = Weather.Snowlight;
                    break;
                case "SNOW":
                    wxNew = Weather.Snowing;
                    break;
                case "BLIZZARD":
                    wxNew = Weather.Blizzard;
                    break;
                case "FOGGY":
                    wxNew = Weather.Foggy;
                    break;
                case "EXTRASUNNY":
                    wxNew = Weather.ExtraSunny;
                    break;
                case "CLOUDS":
                    wxNew = Weather.Clouds;
                    break;
                case "OVERCAST":
                    wxNew = Weather.Overcast;
                    break;
                case "SMOG":
                    wxNew = Weather.Smog;
                    break; 
                default:
                    wxNew = Weather.Clear;
                    break;
            }
            SyncPlayerWeather();
        }

        /// <summary>
        /// Sends a chat message with all /Weather options
        /// </summary>
        private void SendWeatherOptions()
        {
            SendChatMessage(@"
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
/Weather INTERVAL # - Set the minutes between updates. 
/Weather LOCATION DALLAS - Set the location for the real world information. Visit OpenWeatheMap.org for information on locations.
", new int[] { 0, 100, 255 });
        }

        /// <summary>
        /// Register all the weather commands based on if a member has admin permissions or not.
        /// </summary>
        /// <param name="isAdmin">Does the player have permission to set the server variables.</param>
        public void RegisterWxCommands(bool isAdmin)
        {
            API.RegisterCommand("RefreshWeather", new Action<int, List<object>, string>((source, args, raw) =>
            {
                TriggerServerEvent("SPNWeatherRequestUpdate");
            }), false);

            if (isAdmin)
            {
                API.RegisterCommand("Weather", new Action<int, List<object>, string>((source, args, raw) =>
                {
                    if (args.Count == 0)
                    {
                        SendWeatherOptions();
                    }
                    else if (args.Count == 1)
                    {
                        switch (args[0])
                        {
                            case "RESET":
                                TriggerServerEvent("SPNWeatherSetOveride", false, "NONE");
                                SendChatMessage("Attempting to reset weather sync!");
                                break;
                            case "CLEAR":
                                TriggerServerEvent("SPNWeatherSetOveride", true, "CLEAR");
                                SendChatMessage("Attempting to overide weather sync to CLEAR!");
                                break;
                            case "THUNDER":
                                TriggerServerEvent("SPNWeatherSetOveride", true, "THUNDER");
                                SendChatMessage("Attempting to overide weather sync to THUNDER!");
                                break;
                            case "RAIN":
                                TriggerServerEvent("SPNWeatherSetOveride", true, "RAIN");
                                SendChatMessage("Attempting to overide weather sync to RAIN!");
                                break;
                            case "SNOWLIGHT":
                                TriggerServerEvent("SPNWeatherSetOveride", true, "SNOWLIGHT");
                                SendChatMessage("Attempting to overide weather sync to SNOWLIGHT!");
                                break;
                            case "SNOW":
                                TriggerServerEvent("SPNWeatherSetOveride", true, "SNOW");
                                SendChatMessage("Attempting to overide weather sync to SNOW!");
                                break;
                            case "BLIZZARD":
                                TriggerServerEvent("SPNWeatherSetOveride", true, "BLIZZARD");
                                SendChatMessage("Attempting to overide weather sync to BLIZZARD!");
                                break;
                            case "FOGGY":
                                TriggerServerEvent("SPNWeatherSetOveride", true, "FOGGY");
                                SendChatMessage("Attempting to overide weather sync to FOGGY!");
                                break;
                            case "EXTRASUNNY":
                                TriggerServerEvent("SPNWeatherSetOveride", true, "EXTRASUNNY");
                                SendChatMessage("Attempting to overide weather sync to EXTRASUNNY!");
                                break;
                            case "CLOUDS":
                                TriggerServerEvent("SPNWeatherSetOveride", true, "CLOUDS");
                                SendChatMessage("Attempting to overide weather sync to CLOUDS!");
                                break;
                            case "OVERCAST":
                                TriggerServerEvent("SPNWeatherSetOveride", true, "OVERCAST");
                                SendChatMessage("Attempting to overide weather sync to OVERCAST!");
                                break;
                            case "SMOG":
                                TriggerServerEvent("SPNWeatherSetOveride", true, "SMOG");
                                SendChatMessage("Attempting to overide weather sync to SMOG!");
                                break;
                            default:
                                SendChatMessage("Wrong Command!Type /Weather to see all commands!", new int[] { 255, 0, 0 });
                                break;
                        }
                    }
                    else if (args.Count == 2)
                    {
                        switch (args[0])
                        {
                            case "INTERVAL":
                                if (int.TryParse(args[1].ToString(), out int result))
                                {
                                    TriggerServerEvent("SPNWeatherSetRefresh", result);
                                    SendChatMessage("Attempting to set interval to: " + result.ToString() + " minutes");
                                } else SendChatMessage("Wrong Command!", new int[] { 255, 0, 0 });
                                break;
                            case "LOCATION":
                                TriggerServerEvent("SPNWeatherSetRefresh", args[1]);
                                SendChatMessage("Attempting to set locations to: " + args[2].ToString() + " minutes");
                                break;
                            default:
                                SendChatMessage("Wrong Command! Type /Weather to see all commands!", new int[] { 255, 0, 0 });
                                break;
                        }
                    }
                }), false);
            }
        }

        /// <summary>
        /// Sends a chat message with a reply from the server.
        /// </summary>
        /// <param name="reply"></param>
        public void RecieveReply(string reply)
        {
            SendChatMessage(reply);
        }

        public WeatherSync()
        {
            Debug.WriteLine("SPNWeather: Initializing");
            TriggerServerEvent("SPNWeatherCheckPerms");
            EventHandlers.Add("playerSpawned", new Action<Vector3>(OnPlayerSpawned));
            
            EventHandlers["WeatherSync:ClientUpdate"] += new Action<string, string>(RecieveWeather);
            
            EventHandlers["SPNWeatherClientPerms"] += new Action<bool>(RegisterWxCommands);
            EventHandlers["SPNWeatherClientReply"] += new Action<string>(RecieveReply);
        }

        private void SendChatMessage(string message, int[] c = null)
        {
            TriggerEvent("chat:addMessage", new
            {
                color = c ?? new[] { 0, 100, 255 },
                multiline = true,
                args = new[] { "SPNWeather: ", message }
            });
        }
    }
}