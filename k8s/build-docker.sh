BASEDIR=$(dirname $(dirname "$0"))
cd $BASEDIR

docker build . -f MapTalkie/Dockerfile -t maratbr/maptalkie-api:dev 