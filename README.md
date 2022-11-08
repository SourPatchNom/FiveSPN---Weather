# FiveSPN---Weather

![FiveSPN-Weather Banner](https://cdn.discordapp.com/attachments/793012996690804766/1039349079358050365/fspnweather.png)

FiveSPN-Weather is a FiveM resource that syncs the game server weather between all players to a real world location or overriden to an admin defined type using in game commands.

## Prerequisites

For best performance and monitoring, make sure the FiveSPN--Logger resource is utilized properly. This resource is available in the [Full Suite](https://github.com/SourPatchNom/FiveSPN---Suite) or from the [Source](https://github.com/SourPatchNom/FiveSPN---Logger)

## Installation

### Step One: Add the resource to your server.
Copy the folder inside of the FiveMResource folder into your server's resources folder and add it to the server.cfg with the following line. 
```
start FiveSPN-WeatherSync
```

### Step Two: Get an Open Weather Map API key for your server.

1. Goto https://openweathermap.org/
2. Register
3. Navigate to the API key area and generate a key for each server you are using.
4. Add the key to the FiveSPN-Weather resource manifest.

### Step Three: Add the location to the resource manifest.

Using either a zip code, or a city name, add it to the variable in the resource manifest.

## Server Commands

```
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
/Weather INTERVAL # - Set the minutes between updates. 
/Weather CITY 'NAME' - Set the location to a city name for the real world information. Visit OpenWeatherMap.org for information on locations.
/Weather ID ##### - Set the location to a city id number for the real world information. Visit OpenWeatherMap.org for information on locations.
/Weather ZIP ##### - Set the location to a zip code for the real world information. Visit OpenWeatherMap.org for information on locations.
```

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## Discord
FiveM development can be even more amazing if we work together to grow the open source community! Lets Collab! Join the project discord at [itsthenom.com!](http://itsthenom.com/)

## Licenses

In the hopes that the greater community may benefit, you may use this code under the [GNU Affero General Public License v3.0](LICENSE). 

This resource distribution utilizes the [Newtonsoft.JSON Library](https://github.com/JamesNK/Newtonsoft.Json) under the [MIT License](https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md).

This software references the CitizenFX.Core.Server and CitizenFX.Core.Client nuget packages (c) 2017-2020 the CitizenFX Collective used under [license](https://github.com/citizenfx/fivem/blob/master/code/LICENSE) and under the [FiveM Service Agreement](https://fivem.net/terms)

Never heard of FiveM? Learn more about the CitizenFX FiveM project [here](https://fivem.net/)

## Credits
* <b>Sloosecannon</b> for inspiration and rubber ducky assistance during the initial conception of all this in 2020.
* <b>AGHMatti</b> I think... for reference on the http helper, really wish I could locate the source repo now.
