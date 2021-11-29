#!/bin/sh

helmFolder="$(dirname "$0")"
cd $helmFolder
rootFolder="$(dirname $(dirname $PWD))"

eval $(minikube docker-env)
echo "DOCKER_HOST=$DOCKER_HOST"

# docker image build MapTalkie.Services.Posts -t maptalkie/posts-service:0.1.0
cd $rootFolder
docker build -f MapTalkie.Services.Posts/Dockerfile -t maptalkie/posts-service:0.1.0 .

cd $helmFolder
helm upgrade --install maptalkie-release . \
  --namespace=default \
  --set maptalkie-posts-service.image.tag="0.1.0"
  #--debug 