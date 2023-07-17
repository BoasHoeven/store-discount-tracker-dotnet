#!/bin/bash

docker build -t boascontainers/store-discount-telegram-bot:latest -f ./TelegramBot/Dockerfile .
docker image push boascontainers/store-discount-telegram-bot:latest
