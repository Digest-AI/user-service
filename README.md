# User Service

## Описание

Сервис управления пользователями и аутентификации для Digest.AI.
---

## Список задач

---

## Как запустить локально

### 1. Открыть проект в Visual Studio

### 2. Установить User Secrets (PowerShell)

В корне проекта:

```powershell
# JWT ключ (минимум 32 символа)
dotnet user-secrets set "Jwt:Key" "testkey(real one should be 32 chars minimum)"

# Connection String SQL Server
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "insert connection string here"
```

### 3. Обновить базу данных

```powershell
dotnet ef database update
```

### 4. Запустить сервис

```powershell
dotnet run
```

---

### Внешняя ссылка 









