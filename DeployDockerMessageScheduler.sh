#!/bin/bash

docker build -t boascontainers/store-discount-message-scheduler:latest -f ./DiscountAnnouncementScheduler/Dockerfile .
docker image push boascontainers/store-discount-message-scheduler:latest
