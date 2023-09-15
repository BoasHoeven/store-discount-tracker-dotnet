#!/bin/bash

docker build -t boascontainers/store-discount-telegram-bot:latest -f ./TelegramAPI/Dockerfile .
docker image push boascontainers/store-discount-telegram-bot:latest
