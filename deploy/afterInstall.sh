#!/bin/bash
mkdir -p /app/log
	
sudo rsync --delete-before --verbose --archive --exclude ".*" /app/serverTemp/ /app/server/ > /app/log/deploy.log