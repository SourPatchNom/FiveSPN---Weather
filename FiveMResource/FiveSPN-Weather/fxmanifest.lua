fx_version 'bodacious'

name 'FiveSPN-WeatherSync'
description 'Manages weather for the server.'
author 'SourPatchNom'
version 'v1.0'
url 'https://itsthenom.com'

weather_api_key '' -- Put your API key here!
refresh_rate '5' -- Time in minutes between weather updates.
verbose_logs 'false'

games { 'gta5' }

server_script {
	"FiveSpn.Weather.Server.net.dll",
}

client_script {
	"FiveSpn.Weather.Client.net.dll",
}

files {
	"FiveSpn.Weather.Library.dll",
	"Newtonsoft.Json.dll"
}

dependancy 'FiveSPN-Logger'