# GuardLab

GuardLab -  учебная платформа на C# для практики веб-безопасности: сравнивайте уязвимые и защищённые API-сценарии на фиктивных данных. ASP.NET Core, React, PostgreSQL.

## Запуск

1. Скопировать `.env.example` в `.env` и заменить на свои пароли 
2. Собрать и запустить сервисы: `docker compose up --build -d`
3. Открыть <http://localhost:8080>. Swagger API доступен через <http://localhost:8080/swagger>
4. Остановка: `docker compose down`. Чтобы удалить локальные данные базы - `docker compose down -v`

Для запуска API из IDE или PowerShell нужно оставить PostgreSQL в Docker (`docker compose up -d postgres`) и настроить локальные секреты:

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