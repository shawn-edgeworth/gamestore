# GameStore — прогресс обучения

> Живой файл. Обновляем после каждого учебного блока или заметного изменения в коде.
> Последнее обновление: **2026-10-28**

---

## Цель

Подготовиться к стажировке **«Разработчик .NET» (Т-Банк / Тинькофф)**: pet-проект на ASP.NET Core + закрепление C#, LINQ, SQL, Git, EF Core.

Ожидания курса (с сайта): C# / алгоритмы / базовый Git / коллекции + LINQ (`Where`/`Sum`). Плюс — свой проект. На стажировке дальше: AspNet Core, EF Core, GitLab, Docker, брокеры и т.д.

**Фокус сейчас:** backend закрыт по MVP. Дальше алгоритмы и git-ветки; по желанию одна статическая страница из `wwwroot` (не React/MVC). Тесты не углублять. SQL поддерживать короткими запросами, не новым курсом.

---

## Снимок проекта (факт)

| Параметр | Значение |
| --- | --- |
| Стек | .NET 10, Minimal APIs, EF Core + Npgsql, PostgreSQL, FluentValidation, JWT, xUnit |
| Решение | `GameStore.Api` + `GameStore.Api.Tests` |
| Сущности | `Game`, `Category`, `User`, `Order`, `OrderItem` |
| Auth | регистрация, логин, JWT (HS256). Запись игр и категорий — роль `Admin`. Заказы — любой JWT |
| Orders | `POST /orders`, `GET /orders`, `GET /orders/{id}` (свой → 200, чужой → 404). Один `SaveChanges` = одна транзакция |
| README | есть (`README.md` в корне): запуск, user-secrets, миграции, сценарии Swagger |
| Frontend / Docker | нет |
| Git | репозиторий, `.gitignore`, коммиты, remote `origin` на `master`; ветки `feature/...` ещё слабо |
| Архитектура | Endpoint → Service → Repository → DbContext. Один тип — один файл. Неймспейсы: Models, Services, Data, Mappers, Validation |

Эндпоинты (префикса `/api` нет):

| Метод | Путь | Auth | Статус |
| --- | --- | --- | --- |
| GET | `/games` | нет | ✅ фильтры, поиск, пагинация |
| GET | `/games/{id}` | нет | ✅ |
| POST | `/games` | Admin | ✅ |
| PUT | `/games/{id}` | Admin | ✅ |
| DELETE | `/games/{id}` | Admin | ✅ |
| PATCH | `/games/{id}/price` | Admin | ✅ |
| PATCH | `/games/{id}/category` | Admin | ✅ |
| GET | `/categories` | нет | ✅ |
| GET | `/categories/{id}` | нет | ✅ |
| POST | `/categories` | Admin | ✅ |
| PUT | `/categories/{id}` | Admin | ✅ |
| DELETE | `/categories/{id}` | Admin | ✅ Restrict, если есть игры |
| POST | `/register` | нет | ✅ |
| POST | `/login` | нет | ✅ JWT, поле `Token` |
| POST | `/orders` | JWT | ✅ |
| GET | `/orders` | JWT | ✅ свои заказы, `GameName` |
| GET | `/orders/{id}` | JWT | ✅ свой → 200, чужой → 404, без токена → 401 |

---

## Матрица навыков (самооценка → цель)

Легенда: `—` слабо · `~` база · `+` уверенно · `✓` для стажировки достаточно на старте

| Тема | Сейчас | Цель к экзамену | Где закрепляем |
| --- | --- | --- | --- |
| C# основы | + | ✓ | весь проект |
| Коллекции / LINQ | + | ✓ | фильтры игр, мапперы, `Where`/`Skip`/`Take`/`ILike` |
| async/await | ~ | ✓ | сервисы, EF `*Async` |
| DI / lifetimes | + | ✓ | `Program.cs`, Scoped сервисы и DbContext |
| ASP.NET Core (pipeline, Minimal API) | + | ✓ | эндпоинты, JWT, 401/403, Swagger |
| EF Core / миграции | + | ✓ | Include/ThenInclude, FK, Restrict/Cascade, один SaveChanges |
| SQL | + | ~ / + | DBeaver до CTE; индексы отдельно, если понадобятся |
| DTO / Entity / маппинг | + | ✓ | Models + Mappers |
| Валидация | + | ✓ | FluentValidation + `OperationResult` |
| Тесты (unit + integration) | ~ | ✓ | каркас есть; не приоритет |
| Git (checkout/commit/push/pull) | + | ✓ | коммиты и remote есть; ветки ещё слабо |
| Алгоритмы | ~ | ✓ | отдельно, 3–5 задач/неделя |
| Auth (JWT) | + | ~ / ✓ | login/register, Admin, свои заказы |
| Docker | — | ~ | ближе к финалу |

---

## Чеклист проекта

### Сделано

- [x] Каркас ASP.NET Core + PostgreSQL + connection string
- [x] EF Core: `GameDbContext`, сущности каталога, пользователей и заказов
- [x] Миграции до Users, Orders и Restrict/Cascade FK
- [x] CRUD игр + PATCH цены и категории
- [x] Category: list/get/create/update/delete
- [x] Id и Category в response DTO, `Include(Category)`
- [x] Проверка существования категории при создании/смене категории
- [x] FluentValidation + `OperationResult` + `[MemberNotNullWhen]`
- [x] Фильтры `GET /games`: categoryId, цена, search (`ILike`), page/pageSize
- [x] OpenAPI + Swagger UI; кнопка Authorize (Swashbuckle bearer)
- [x] `.gitignore`, `Jwt:Key` в user-secrets
- [x] Git-коммиты и remote
- [x] Users: регистрация, хеш пароля, логин, JWT
- [x] Роль Admin на все записи игр и категорий; GET каталога публичный
- [x] Orders: create/list/get by id (владелец / 404); один `SaveChangesAsync`
- [x] FK Restrict/Cascade, один тип на файл, неймспейсы по слоям
- [x] README в корне (запуск, секреты, миграции, сценарии Swagger)
- [x] SQL-практика: SELECT…LIMIT, JOIN, LEFT JOIN, GROUP BY/HAVING, подзапросы, EXISTS, CTE

### Известный техдолг (не забывать)

- [ ] В `GET /games` нет `totalCount` в ответе
- [ ] В `OrderDetails` нет `Id`
- [ ] `GameValidatorTests` устарели; integration-тесты не доведены — не чинить в приоритете
- [ ] CS8604 в `JwtTokenService`: `Jwt:Key` теоретически null (в `Program.cs` уже есть throw)
- [ ] Нет Docker Compose и нет витрины (`wwwroot`)
- [ ] SQL-индексы не разбирали (явный `BeginTransaction` не нужен)

### Дальше по проекту (приоритет)

1. **Алгоритмы** 3–5 задач в неделю (массивы, строки, словари, два указателя)
2. **Git-ветки** `feature/...` на следующую маленькую фичу
3. По желанию: одна статическая страница в `wwwroot` + `fetch` + CSS с CDN (не React, не MVC)
4. Docker Compose (PostgreSQL, опционально API)
5. SQL: короткий запрос по своей БД время от времени
6. Мелочи API (`totalCount`, `OrderDetails.Id`) — только если останется время

Полный roadmap: [`plan.md`](./plan.md).

Этапы A, C, D, E (обязательное) и README закрыты. SQL (B) по запросам закрыт. G (витрина) и H (Docker) по желанию. I (алгоритмы, ветки, рассказ о проекте) — основной фокус к отбору.

---

## Учебные треки (параллельно проекту)

### A. GameStore (основной)

Идём по этапам в `plan.md`. После каждого этапа обновляем чеклист выше.

### B. SQL (поддерживать, не курс заново)

| Уровень | Темы | Статус |
| --- | --- | --- |
| 1 | SELECT, WHERE, ORDER BY, LIMIT | ✅ |
| 2 | INNER JOIN + LEFT JOIN | ✅ |
| 3 | GROUP BY, SUM, HAVING, AVG | ✅ |
| 4 | NULL / IS NULL, COALESCE, CASE | ✅ |
| 5 | IN, EXISTS / NOT EXISTS, скалярный подзапрос | ✅ |
| 6 | CTE | ✅ |
| 6b | Индексы | ☐ |
| 6c | Транзакция заказа через один `SaveChanges` | ✅ (явный `BeginTransaction` не делали) |

### C. Алгоритмы (для отбора)

3–5 задач в неделю (LeetCode Easy/Medium или ULearn/Яндекс). Фокус: массивы, строки, словари, два указателя, сортировка.

| Неделя | Тема | Сделано |
| --- | --- | --- |
| … | … | ☐ |

### D. Git

| Навык | Статус |
| --- | --- |
| status / add / commit | ✅ на этом репо |
| branch / checkout / merge | ☐ практика на следующей фиче |
| push / pull / remote | ✅ `origin/master` |
| осмысленные сообщения коммитов | ~ есть, держать короткие «зачем» |

---

## Лог сессий

Краткие записи: дата → что сделали → что поняли → что дальше.

### 2026-10-13 — стартовый аудит

- Зафиксировано фактическое состояние репо vs старый план из ChatGPT.
- Созданы `progress.md` + обновлённый `plan.md`.

### 2026-10 — каталог, валидация, фильтры

- Id в DTO, `Include(Category)`, проверка `CategoryId`, Category PUT/DELETE.
- FluentValidation, фильтры `GET /games`, `.gitignore`, первые коммиты.

### 2026-10-23…25 — Users, JWT, Admin

- Users, хеш пароля, логин, JWT, Swagger bearer.
- Сначала Admin только на `POST /games`.

### 2026-10-25…26 — Orders, FK, ошибки

- Заказы, Restrict/Cascade, свои заказы, `GET /orders/{id}` (чужой → 404).
- `MemberNotNullWhen` на `OperationResult`.

### 2026-10-26…27 — SQL с нуля (DBeaver)

- SELECT…LIMIT, JOIN, GROUP BY/HAVING, LEFT JOIN, CASE, AVG, IN, EXISTS, CTE.
- Для одного фильтра сумм проще HAVING, чем WITH.

### 2026-10-27…28 — Admin на запись, README, термины auth

- Один `SaveChangesAsync` в `CreateOrder`: `Add` только в памяти, сохранение — одна транзакция.
- Admin на PUT/DELETE/PATCH игр и на запись категорий; GET каталога публичный.
- Аутентификация = кто ты (401); авторизация = можно ли (403 или 404 на чужой заказ).
- README: запуск, user-secrets, миграции, сценарии Swagger без curl как основного пути.
- Дальше: алгоритмы; витрина только как статика из того же API, если захочется демо.

---

## Как обновлять этот файл

1. После кода — отметить чеклист «Сделано» / техдолг.
2. После учебы без кода — строка в «Лог сессий» + матрица навыков.
3. Не раздувать: факты и следующий шаг важнее длинных конспектов.
4. Детальный план этапов держим в `plan.md`; здесь — **статус**.
