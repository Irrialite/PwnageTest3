#!/bin/bash
cd /app/server/src/ServerTest2/
sudo dotnet restore
path=$(sudo dotnet publish --configuration Release | grep -Pio "(?<=published to )(.+)")
cd $path
sudo dotnet ServerTest2.dll &