# FiveSPN---Weather

![FiveSPN-Weather Banner](https://cdn.discordapp.com/attachments/793012996690804766/1039349079358050365/fspnweather.png)

FiveSPN-Weather is a FiveM resource that syncs the game server weather between all players to up to four real world locations or a global override set by a server admin from within the game.

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

### Step Three: Add the source location to the locations.json file.

Using either a zip code, city id, or city name, add your source location to the locations.json file.

Default locations are Seattle, WA for Paleto, Phoenix for Blaine County, and Los Angeles for Los Santos.  

Using City Name
```javascript
{
    "Cities": [
        ["Seattle", -165.17, 6052.23, 80.03],
        ["Phoenix", 1398.82, 3977.76, 324.74],
        ["Los Angeles", -353.02, 523.95, 523.95]
    ],
    "Zips": [
        
    ],
    "Ids": [
        
    ]
}
```

Using Zip
```javascript
{
    "Cities": [

    ],
    "Zips": [
        [98101, -165.17, 6052.23, 80.03],
        [85323, 1398.82, 3977.76, 324.74],
        [90001, -353.02, 523.95, 523.95]
    ],
    "Ids": [

    ]
}
```

Using City Id (Find a cities id [here](https://openweathermap.org/find) by searching, then going to the city page and looking at url)
```javascript
{
    "Cities": [
        
    ],
    "Zips": [
        
    ],
    "Ids": [
        [5809844, -165.17, 6052.23, 80.03],
        [5308655, 1398.82, 3977.76, 324.74],
        [5368361, -353.02, 523.95, 523.95]
    ]
}
```

## Admin Server Commands

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
```

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## Discord
FiveM development can be even more amazing if we work together to grow the open source community! 

Lets Collab! Join the project discord at [itsthenom.com!](http://itsthenom.com/)
## Licensing

    Copyright ©2022 Owen Dolberg (SourPatchNom)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.

In the hopes that the greater community may benefit, you may use this code under the [GNU Affero General Public License v3.0](LICENSE).

This resource distribution utilizes the [Newtonsoft.JSON Library](https://github.com/JamesNK/Newtonsoft.Json) under the [MIT License](https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md).

This software references the CitizenFX.Core.Server and CitizenFX.Core.Client nuget packages (c) 2017-2020 the CitizenFX Collective used under [license](https://github.com/citizenfx/fivem/blob/master/code/LICENSE) and under the [FiveM Service Agreement](https://fivem.net/terms)

Never heard of FiveM? Learn more about the CitizenFX FiveM project [here](https://fivem.net/)

## Credits
* <b>Sloosecannon</b> for inspiration and rubber ducky assistance during the initial conception of all this in 2020.
* <b>AGHMatti</b> I think... for reference on the http helper, really wish I could locate the source repo now.
