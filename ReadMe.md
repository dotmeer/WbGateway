# Персональный гейтвей-заплатка для WirenBoard

## Публикация и развертывание

публикация докер-образа:   
`docker build -t dotmeer/wbgateway:{tag} . -f {running assembly file path}`   
`docker login`   
`docker push dotmeer/wbgateway:{tag}`

перекачиваем образ в NAS, там docker-compose

## Roadmap проекта

не по порядку и важности, а по желанию

* переезд на net8
* метрики через OpenTelemetry вместо prometheus-net, но отдавать в prometheus, подумать о трейсах. после окончательного релиза
* интеграция с Алисой