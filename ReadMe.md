# Персональный гейтвей-заплатка для WirenBoard

## Публикация и развертывание

публикация докер-образа:
docker build -t dotmeer/wbgateway:{tag} .
docker login
docker push dotmeer/wbgateway:{tag}

перекачиваем образ в NAS, там docker-compose