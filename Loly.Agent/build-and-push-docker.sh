export APP_NAME=loly
export APP_VERSION=$(date '+%Y%m%d%H%M%S')

docker build -t ${APP_NAME}:${APP_VERSION} .
docker tag ${APP_NAME}:${APP_VERSION} ${DOCKER_USERNAME}/${APP_NAME}:${APP_VERSION}
docker tag ${APP_NAME}:${APP_VERSION} ${DOCKER_USERNAME}/${APP_NAME}:latest

docker push ${DOCKER_USERNAME}/${APP_NAME}:${APP_VERSION}
docker push ${DOCKER_USERNAME}/${APP_NAME}:latest