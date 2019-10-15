export APP_NAME=loly-agent
export APP_VERSION=$(date '+%Y%m%d%H%M%S')

cd ..

docker build -f Docker/Agent.Dockerfile -t ${APP_NAME}:${APP_VERSION} . && \
  docker tag ${APP_NAME}:${APP_VERSION} ${APP_NAME}:latest && \
  docker tag ${APP_NAME}:${APP_VERSION} ${DOCKER_USERNAME}/${APP_NAME}:${APP_VERSION} && \
  docker tag ${APP_NAME}:${APP_VERSION} ${DOCKER_USERNAME}/${APP_NAME}:${APP_VERSION} && \
  docker tag ${APP_NAME}:${APP_VERSION} ${DOCKER_USERNAME}/${APP_NAME}:latest && \
  docker push ${DOCKER_USERNAME}/${APP_NAME}:${APP_VERSION}  && \
  docker push ${DOCKER_USERNAME}/${APP_NAME}:latest

cd Docker