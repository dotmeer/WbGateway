# Персональный гейтвей-заплатка для WirenBoard

## Публикация и развертывание

публикация докер-образа:   
`docker build -t dotmeer/wbgateway:{tag} .`   
`docker login`   
`docker push dotmeer/wbgateway:{tag}`

перекачиваем образ в NAS, там docker-compose

## Roadmap проекта

не по порядку и важности, а по желанию

- [ ] переезд на net8
- [ ] метрики через OpenTelemetry вместо prometheus-net, но отдавать в prometheus
- [ ] minimal API (возможно)
- [ ] интеграция с Алисой