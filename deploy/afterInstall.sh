#!/bin/bash
mkdir -p /app/log
	
sudo rsync --delete-before --verbose --archive --exclude ".*" --exclude "log" /app/serverTemp/ /app/server/ > /app/log/deploy.log