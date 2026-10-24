# GuardLab

GuardLab -  учебная платформа на C# для практики веб-безопасности: сравнивайте уязвимые и защищённые API-сценарии на фиктивных данных. ASP.NET Core, React, PostgreSQL.

## Запуск

1. Скопировать `.env.example` в `.env` и заменить на свои пароли 
2. Собрать и запустить сервисы: `docker compose up --build -d`
3. Открыть <http://localhost:8080>. Swagger API доступен через <http://localhost:8080/swagger>
4. Остановка: `docker compose down`. Чтобы удалить локальные данные базы - `docker compose down -v`

## Запуск в minikube
### 1. Запустить кластер и подготовить секреты

```
minikube start -p minikube
kubectl config use-context minikube
```

### 2. Собрать и загрузить образы
```
docker build -t guardlab-api:1.0 -f src/GuardLab.Api/Dockerfile .
minikube image load guardlab-api:1.0

docker build -t guardlab-web:1.0 -f src/GuardLab.Web/Dockerfile .
minikube image load guardlab-web:1.0

docker build -t guardlab-db-migrator:1.0 -f src/GuardLab.DbMigrator/Dockerfile .
minikube image load guardlab-db-migrator:1.0
```

### 3. Применить манифесты
```
kubectl apply -f infra/manifests/1-namespace.yaml
kubectl apply -f infra/manifests/2-secrets.yaml
kubectl apply -f infra/manifests/3-pg-configmap.yaml
kubectl apply -f infra/manifests/5-pg-service.yaml
kubectl apply -f infra/manifests/4-pg-statefulset.yaml
kubectl rollout status statefulset/postgres -n guardlab --timeout=180s

kubectl apply -f infra/manifests/6-migrator-job.yaml
kubectl wait --for=condition=complete job/db-migrator -n guardlab --timeout=300s

kubectl apply -f infra/manifests/7-api-deployment.yaml
kubectl apply -f infra/manifests/8-api-service.yaml
kubectl apply -f infra/manifests/9-front-deployment.yaml
kubectl apply -f infra/manifests/10-front-service.yaml

kubectl rollout status deployment/guardlab-api -n guardlab --timeout=180s
kubectl rollout status deployment/guardlab-web -n guardlab --timeout=180s
kubectl get pods,services,pvc -n guardlab
```

### 4. Victoria Logs и OpenTelemetry коллектор

Victoria Logs хранит данные в PVC
Collector запускается как DaemonSet
```
kubectl apply -f infra/manifests/11-victoria-logs.yaml
kubectl rollout status deployment/victoria-logs -n guardlab --timeout=180s

kubectl apply -f infra/manifests/12-opentelemetry-collector.yaml
kubectl rollout status daemonset/otel-collector -n guardlab --timeout=180s
```

```
kubectl port-forward -n guardlab svc/victoria-logs 9428:9428
```

 http://localhost:9428/select/vmui/

```
minikube service guardlab-web -n guardlab --url
```


Остановить кластер, сохранив его данные:

```powershell
minikube stop -p minikube
```
### Первый запуск пустой базы

Перед первым запуском надо задать в `.env` JWT-ключ длиной не менее 32 байт. `docker compose up --build` автоматически запускает PostgreSQL,  db-init, миграции, учебные seed-скрипты и seed администратора

```
docker compose up --build
```

db-init запускается при каждом docker compose up

Для запуска API из IDE или PowerShell нужно запустить PostgreSQL и init-контейнер (`docker compose up --build -d postgres db-init`), затем настроить локальные секреты:

```
dotnet user-secrets init --project src\GuardLab.Api
dotnet user-secrets set "ConnectionStrings:Platform" "Host=localhost;Port=15432;Database=guardlab;Username=platform_app;Password=..." --project src\GuardLab.Api
dotnet user-secrets set "ConnectionStrings:Sandbox" "Host=localhost;Port=15432;Database=guardlab;Username=lab_app;Password=..." --project src\GuardLab.Api
dotnet run --project src\GuardLab.Api
```

`launchSettings.json` задаёт для локального запуска `ASPNETCORE_ENVIRONMENT=Development`; Compose запускает контейнер API в `Production`. В Development приложение подключает User Secrets, 
а контейнер получает connection strings из переменных окружения Compose

## Локальные пароли и User Secrets

```env
POSTGRES_PASSWORD=пароль-пользователя
DB_OWNER_PASSWORD=пароль-db_owner
PLATFORM_DB_PASSWORD=пароль-platform_app
SANDBOX_DB_PASSWORD=пароль-lab_app
```

```
docker compose up -d postgres
```

`infra/postgres/init-roles.sql` создаёт роли `db_owner`, `platform_app` и `lab_app` с соответствующими паролями из `.env`

Для локального запуска API надо сохранить connection strings в User Secrets

```
dotnet user-secrets init --project src\GuardLab.Api

dotnet user-secrets set "ConnectionStrings:Platform" `
  "Host=localhost;Port=15432;Database=guardlab;Username=platform_app;Password=пароль-platform_app" `
  --project src\GuardLab.Api

dotnet user-secrets set "ConnectionStrings:Sandbox" `
  "Host=localhost;Port=15432;Database=guardlab;Username=lab_app;Password=пароль-lab_app" `
  --project src\GuardLab.Api
```

Проверить сохранённые настройки можно так:

```
dotnet user-secrets list --project src\GuardLab.Api
```


```
docker compose down -v
docker compose up -d postgres
```

## Локальный frontend

Из `src\GuardLab.Web`:

```
npm.cmd install
npm.cmd run build
npm.cmd run dev -- --host 127.0.0.1 --port 5180
```

## Логирование

API использует `ILogger<T>` с JSON Console Provider. Логи выводятся в stdout/stderr и доступны через `docker compose logs -f api`. 
Middleware записывает метод, путь, статус и длительность запроса. Заголовок `X-Trace-Id` связывает запрос с его логами.
Пароли, JWT и тела запросов не записываются.

## Структура

- `src/GuardLab.Api` - HTTP Controllers и запуск приложения.
- `src/GuardLab.Application` - переиспользуемые сценарии (всякие сервисы и тд)
- `src/GuardLab.Core` - доменные модели
- `src/GuardLab.Infrastructure` - инфраструктурные реализации (по типу репозиториев для postgres)
- `src/GuardLab.Web` - React/TypeScript frontend и Vite
- `infra/postgres` - роли, таблицы схем

## Алерт по логам API через vmalert

Алерт проверяет логи Warning из запроса течение последних пяти минут
Условие должно сохраняться одну минуту, vmalert выполняет правило раз в минуту
Alertmanager показывает состояние алерта в UI

```
kubectl apply -f infra/manifests/13-alertmanager.yaml
kubectl rollout status deployment/alertmanager -n guardlab --timeout=180s
kubectl apply -f infra/manifests/14-vmalert.yaml
kubectl rollout status deployment/vmalert -n guardlab --timeout=180s
```


```
kubectl port-forward -n guardlab svc/vmalert 8880:8880
```

```
kubectl port-forward -n guardlab svc/alertmanager 9093:9093
```

Интерфейс vmalert:http://localhost:8880/ 
Полученные алерты Alertmanager: http://localhost:9093/#/alerts
Алёрты тут: http://localhost:8880/vmalert/alerts?search=#group-4708710682925592581


# Метрики
См. infra/helm/README.md