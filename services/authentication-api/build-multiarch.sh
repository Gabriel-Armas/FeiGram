#!/bin/bash

IMAGE_NAME="authentication-api"
TAG="latest"

PLATFORMS="linux/amd64,linux/arm64"

docker buildx create --use --name mybuilder || docker buildx use mybuilder

docker buildx build \
  --platform $PLATFORMS \
  -t $IMAGE_NAME:$TAG \
  --load \
  .

echo "Imagen multiarch $IMAGE_NAME:$TAG construida para plataformas: $PLATFORMS"
