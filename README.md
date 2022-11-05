# FiveSPN---WeatherSync

Sync your game server weather between all players and to a real world location! Can also be manually overriden by an admin from in the server for special needs.

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

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

## Licenses

In the hopes that the greater community may benefit, you may use this code under the [GNU Affero General Public License v3.0](LICENSE). 

This resource distribution utilizes the [Newtonsoft.JSON Library](https://github.com/JamesNK/Newtonsoft.Json) under the [MIT License](https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md).

## Credits
* <b>Sloosecannon</b> for inspiration and rubber ducky assistance during the initial conception of all this in 2020.
* <b>AGHMatti</b> I think... for reference on the http helper, really wish I could locate the source repo now.
