#!/bin/bash

docker build -t boascontainers/store-discount-message-scheduler:latest -f ./MessageScheduler/Dockerfile .
docker image push boascontainers/store-discount-message-scheduler:latest
