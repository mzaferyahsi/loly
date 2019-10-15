export APP_NAME=loly-agent
export APP_VERSION=$(date '+%Y%m%d%H%M%S')

cd ..

docker build -f Docker/Agent.Dockerfile -t ${APP_NAME}:${APP_VERSION} . && \
  docker tag ${APP_NAME}:${APP_VERSION} ${APP_NAME}:latest && \
  docker tag ${APP_NAME}:${APP_VERSION} ${REPO}/${DOCKER_USERNAME}/${APP_NAME}:${APP_VERSION} && \
  docker tag ${APP_NAME}:${APP_VERSION} ${REPO}/${DOCKER_USERNAME}/${APP_NAME}:${APP_VERSION} && \
  docker tag ${APP_NAME}:${APP_VERSION} ${REPO}/${DOCKER_USERNAME}/${APP_NAME}:latest && \
  docker push ${REPO}/${DOCKER_USERNAME}/${APP_NAME}:${APP_VERSION}  && \
  docker push ${REPO}/${DOCKER_USERNAME}/${APP_NAME}:latest

cd Docker