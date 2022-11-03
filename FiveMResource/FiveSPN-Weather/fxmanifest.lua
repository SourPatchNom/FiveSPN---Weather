fx_version 'bodacious'

name 'FiveSPN-WeatherSync'
description 'Manages weather for the server.'
author 'SourPatchNom'
version 'v1.0'
url 'https://itsthenom.com'

weather_api_key '' -- Put your API key here!
--Uncomment and use only one of the following. Find a cities id with https://openweathermap.org/find
--weather_city 'Hollywood' -- Use this to use a city name.
--weather_id '5357527' -- Use this to use a city id number.
weather_zip '90210' -- Use this to use a zip code.
refresh_rate '5' -- Time in minutes between weather updates.

games { 'gta5' }

server_script {
	"WeatherSyncServer.net.dll",
}

client_script {
	"WeatherSyncClient.net.dll",
}

dependancy 'FiveSPN-Logger'