using System;
using System.Collections.Generic;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FiveSpnLoggerClientLibrary;
using FiveSpnLoggerClientLibrary.Classes;
using FiveSpnLoggerClientLibrary.Enums;

namespace WeatherSyncClient
{
public class WeatherSyncClient : BaseScript
    {
        private Weather _wxCurrent = Weather.Clear;
        private Weather _wxNew = Weather.Clear;
        private string _wxLocation = "NONE";
        private bool _wxIsChanging;

        /// <summary>
        /// Requests an updated weather from the server when a player spawns.
        /// </summary>
        /// <param name="a"></param>
        private static void OnPlayerSpawned([FromSource]Vector3 a)
        {
            TriggerServerEvent("WeatherSync:RequestUpdate");
        }

        /// <summary>
        /// Syncs the players weather if it is needed. 
        /// </summary>
        private void SyncPlayerWeather()
        {
            if (World.Weather != _wxNew && !_wxIsChanging)
            {
                Debug.WriteLine("SPNWeather: Changing weather to " + _wxNew.ToString() + " from " + World.Weather.ToString() + " which is synced from " + _wxLocation);
                _wxIsChanging = true;
                _wxCurrent = _wxNew;
                World.TransitionToWeather(_wxCurrent, 45.0f);
            }
            else if (World.Weather == _wxCurrent && _wxIsChanging)
            {
                _wxIsChanging = false;
            }
        }

        /// <summary>
        /// Receive the weather from the server.
        /// </summary>
        /// <param name="newWx"></param>
        /// <param name="location"></param>
        private void ReceiveWeather(string newWx, string location)
        {
            _wxLocation = location;
            switch (newWx)
            {
                case "THUNDER":
                    _wxNew = Weather.ThunderStorm;
                    break;
                case "RAIN":
                    _wxNew = Weather.Raining;
                    break;
                case "SNOWLIGHT":
                    _wxNew = Weather.Snowlight;
                    break;
                case "SNOW":
                    _wxNew = Weather.Snowing;
                    break;
                case "BLIZZARD":
                    _wxNew = Weather.Blizzard;
                    break;
                case "FOGGY":
                    _wxNew = Weather.Foggy;
                    break;
                case "EXTRASUNNY":
                    _wxNew = Weather.ExtraSunny;
                    break;
                case "CLOUDS":
                    _wxNew = Weather.Clouds;
                    break;
                case "OVERCAST":
                    _wxNew = Weather.Overcast;
                    break;
                case "SMOG":
                    _wxNew = Weather.Smog;
                    break; 
                default:
                    _wxNew = Weather.Clear;
                    break;
            }
            SyncPlayerWeather();
        }

        /// <summary>
        /// Sends a chat message with all /Weather options
        /// </summary>
        private static void SendWeatherOptions()
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
/Weather CITY 'NAME' - Set the location to a city name for the real world information. Visit OpenWeatherMap.org for information on locations.
/Weather ID ##### - Set the location to a city id number for the real world information. Visit OpenWeatherMap.org for information on locations.
/Weather ZIP ##### - Set the location to a zip code for the real world information. Visit OpenWeatherMap.org for information on locations.
", new int[] { 0, 100, 255 });
        }

        /// <summary>
        /// Register all the weather commands based on if a member has admin permissions or not.
        /// </summary>
        /// <param name="isAdmin">Does the player have permission to set the server variables.</param>
        private static void RegisterWxCommands(bool isAdmin)
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
                                TriggerServerEvent("WeatherSync:SetOverride", true, args[0]);
                                SendChatMessage($"Attempting to override weather sync to {args[0]}");
                                break;
                            default:
                                SendChatMessage("Wrong Command! Type /Weather to see all commands!", new int[] { 255, 0, 0 });
                                break;
                        }
                    }
                    else if (args.Count == 2)
                    {
                        switch (args[0])
                        {
                            case "INTERVAL":
                            case "interval":
                                if (int.TryParse(args[1].ToString(), out int result))
                                {
                                    TriggerServerEvent("WeatherSync:SetRefresh", result);
                                    SendChatMessage("Attempting to set interval to: " + result.ToString() + " minutes.");
                                } else SendChatMessage("Wrong Command!", new int[] { 255, 0, 0 });
                                break;
                            case "CITYID":
                            case "CityID":
                            case "cityid":
                                TriggerServerEvent("WeatherSync:SetLocationId", args[1]);
                                SendChatMessage("Attempting to set weather location id to: " + args[1].ToString() + ".");
                                break;
                            case "CITY":
                            case "City":
                            case "city":
                                TriggerServerEvent("WeatherSync:SetLocationCity", args[1]);
                                SendChatMessage("Attempting to set weather location city to: " + args[1].ToString() + ".");
                                break;
                            case "ZIP":
                            case "Zip":
                            case "zip":
                                TriggerServerEvent("WeatherSync:SetLocationZip", args[1]);
                                SendChatMessage("Attempting to set weather location zip to: " + args[1].ToString() + ".");
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
        private static void ReceiveReply(string reply)
        {
            SendChatMessage(reply);
        }

        public WeatherSyncClient()
        {
            ClientLogger.SendClientLogMessage(new LogMessage("WeatherSync", LogMessageSeverity.Verbose,"Initializing"));
            TriggerServerEvent("WeatherSync:CheckPerms");
            EventHandlers.Add("playerSpawned", new Action<Vector3>(OnPlayerSpawned));
            EventHandlers["WeatherSync:UpdateClientWx"] += new Action<string, string>(ReceiveWeather);
            EventHandlers["WeatherSync:ApplyClientPerms"] += new Action<bool>(RegisterWxCommands);
            EventHandlers["WeatherSync:ReplyToClient"] += new Action<string>(ReceiveReply);
        }

        private static void SendChatMessage(string message, int[] c = null)
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