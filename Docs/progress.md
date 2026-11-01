# GameStore — прогресс обучения

> Живой файл. Агент обновляет после закрытого шага. Последнее обновление: **2026-11-01**

Карта файлов: [`structure.md`](./structure.md). Roadmap: [`plan.md`](./plan.md).

---

## Цель

Подготовиться к стажировке **«Разработчик .NET» (Т-Банк / Тинькофф)**: pet-проект на ASP.NET Core + закрепление C#, LINQ, SQL, Git, EF Core.

Ожидания курса (с сайта): C# / алгоритмы / базовый Git / коллекции + LINQ (`Where`/`Sum`). Плюс — свой проект. На стажировке дальше: AspNet Core, EF Core, GitLab, Docker, брокеры и т.д.

**Фокус сейчас:** Postgres в Docker Compose работает, API по-прежнему `dotnet run` на машине. В README ещё нет инструкции про Compose. Алгоритмы — отдельно, не в этом репо. Тесты не углублять.

---

## Снимок проекта (факт)

| Параметр | Значение |
| --- | --- |
| Стек | .NET 10, Minimal APIs, EF Core + Npgsql, PostgreSQL, FluentValidation, JWT, xUnit |
| Решение | `GameStore.Api` + `GameStore.Api.Tests` |
| Сущности | `Game`, `Category`, `User`, `Order`, `OrderItem`, `Cart`, `CartItem` |
| Auth | регистрация, логин, JWT (HS256, **7 дней**). Запись игр и категорий — роль `Admin`. Заказы, кошелёк, корзина, возврат — любой JWT. Просроченный токен сбрасывается в шапке, не рисует «вход» |
| Orders | `POST /orders`, `GET /orders`, `GET /orders/{id}` (свой → 200, чужой → 404). Списание `User.Balance` в том же `SaveChanges`. Чек: снимок `OrderItem.GameName` + `UnitPrice`. Статус: enum `OrderStatus` (`Paid = 0`, `Refunded = 1`). `POST /orders/{id}/refund`: полный возврат, сумма со снимка, один `SaveChanges` (баланс + статус), повтор → 400, чужой → 404. Витрина: кнопка на `orders.html` |
| Wallet | `User.Balance` `numeric(12,2)` (`MoneyHelper` 12,2). `GET /users/me`, `POST /users/me/balance` (только пополнение) |
| Cart | одна корзина на пользователя (уникальный `UserId`). Позиции: уникальные `CartId`+`GameId`. Игра удалена → Cascade строк корзины. Заказ↔игра Restrict |
| README | есть: запуск, user-secrets, миграции, сценарии Swagger; **блока Compose ещё нет** |
| Frontend | статика `wwwroot` (каталог, поиск, логин, заказ, админка, кошелёк, корзина, **возврат на заказах**) |
| Docker | `docker-compose.yml`: только Postgres 16, named volume `gameStoreData`, порт 5432. API в контейнер не клали. Для GameStore база = Compose; служба Windows на 5432 выключена |
| Git | `origin/master`; фичи ветками `feature/frontend`, `feature/wallet`, `feature/cart`, `feature/refunds`, сейчас **`feature/docker`** |
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
| GET | `/users/me` | JWT | ✅ профиль и баланс |
| POST | `/users/me/balance` | JWT | ✅ пополнение |
| POST | `/orders` | JWT | ✅ списание баланса, снимок имени и цены |
| GET | `/orders` | JWT | ✅ свои заказы, `GameName` из колонки, есть `id` и `orderStatus` |
| GET | `/orders/{id}` | JWT | ✅ свой → 200, чужой → 404, без токена → 401, в теле `orderStatus` |
| POST | `/orders/{id}/refund` | JWT | ✅ полный возврат, снимок чека, повтор → 400, чужой → 404, без токена → 401 |
| GET | `/cart` | JWT | ✅ нет строки → 200 и пустой `cartItems`, без INSERT |
| POST | `/cart/items` | JWT | ✅ get-or-create, та же игра → `Quantity +=` |
| DELETE | `/cart/items/{gameId}` | JWT | ✅ нет корзины/позиции → 404; шапка `"Carts"` остаётся |
| PATCH | `/cart/items/{gameId}` | JWT | ✅ ставит `Quantity`, не прибавляет; нет позиции → 404 |
| POST | `/cart/checkout` | JWT | ✅ заказ + списание + очистка `CartItem` в одном `SaveChanges`; пустая корзина → 400 |

---

## Матрица навыков (самооценка → цель)

Легенда: `—` слабо · `~` база · `+` уверенно · `✓` для стажировки достаточно на старте

| Тема | Сейчас | Цель к экзамену | Где закрепляем |
| --- | --- | --- | --- |
| C# основы | + | ✓ | весь проект |
| Коллекции / LINQ | + | ✓ | фильтры игр, мапперы, `Where`/`Skip`/`Take`/`ILike`, поиск позиции корзины |
| async/await | ~ | ✓ | сервисы, EF `*Async` |
| DI / lifetimes | + | ✓ | `Program.cs`, Scoped сервисы и DbContext |
| ASP.NET Core (pipeline, Minimal API) | + | ✓ | эндпоинты, JWT, 401/403, Swagger, статика |
| EF Core / миграции | + | ✓ | Include, FK Restrict/Cascade, уникальные индексы, identity `Id == 0`, сырой `Sql` в миграции |
| SQL | + | ~ / + | DBeaver до CTE; уникальный индекс на корзине уже в модели |
| DTO / Entity / маппинг | + | ✓ | снимок чека vs живой каталог в корзине |
| Валидация | + | ✓ | FluentValidation + `OperationResult` |
| Тесты (unit + integration) | ~ | ✓ | каркас есть; не приоритет |
| Git (checkout/commit/push/pull) | + | ✓ | ветки `feature/frontend`, `feature/wallet`, `feature/cart`, `feature/refunds`, `feature/docker` |
| Алгоритмы | ~ | ✓ | отдельно, 3–5 задач/неделя |
| Auth (JWT) | + | ~ / ✓ | login/register, Admin, свои заказы/корзина/кошелёк/возврат |
| Docker | ~ | ~ | Compose: только Postgres; API с хоста |

---

## Чеклист проекта

### Сделано

- [x] Каркас ASP.NET Core + PostgreSQL + connection string
- [x] EF Core: `GameDbContext`, сущности каталога, пользователей, заказов, корзины
- [x] Миграции до Users, Orders, Balance/precision, Cart + `OrderItem.GameName` (backfill SQL), `OrderStatus` (`defaultValue: 0`)
- [x] CRUD игр + PATCH цены и категории
- [x] Category: list/get/create/update/delete
- [x] Id и Category в response DTO, `Include(Category)`
- [x] Проверка существования категории при создании/смене категории
- [x] FluentValidation + `OperationResult` + `[MemberNotNullWhen]`
- [x] Фильтры `GET /games`: categoryId, цена, search (`ILike`), page/pageSize
- [x] OpenAPI + Swagger UI; кнопка Authorize (Swashbuckle bearer)
- [x] `.gitignore`, `Jwt:Key` в user-secrets
- [x] Git-коммиты, remote, ветки `feature/...`
- [x] Users: регистрация, хеш пароля, логин, JWT
- [x] Роль Admin на все записи игр и категорий; GET каталога публичный
- [x] Orders: create/list/get by id (владелец / 404); один `SaveChangesAsync`; списание баланса
- [x] Снимок `GameName` на `OrderItem`; маппер читает колонку, не `Game.Name`
- [x] Кошелёк: пополнение `POST /users/me/balance`, `GET /users/me`, `numeric(12,2)`
- [x] Корзина в БД: уникальный `UserId`, уникальные `CartId`+`GameId`, Cascade при удалении игры
- [x] `GET /cart`, `POST /cart/items` (get-or-create, без ручного `Id = -1`)
- [x] `DELETE /cart/items/{gameId}` (404 если нет корзины или позиции; каталог не трогаем)
- [x] `PATCH /cart/items/{gameId}` (присвоение количества, не сумма)
- [x] `POST /cart/checkout`: один `SaveChanges` (заказ, баланс, `cart.Items.Clear()`), пустая корзина → 400
- [x] Витрина корзины: `wwwroot/cart.html`, ссылка в шапке, «В корзину» на карточке игры
- [x] FK Restrict/Cascade, один тип на файл, неймспейсы по слоям
- [x] README в корне
- [x] Статическая витрина `wwwroot` (каталог, поиск, логин, заказы, админка, кошелёк, корзина)
- [x] SQL-практика: SELECT…LIMIT, JOIN, LEFT JOIN, GROUP BY/HAVING, подзапросы, EXISTS, CTE
- [x] `OrderStatus` на сущности и в DTO (`Paid` / `Refunded`); `OrderDetails.Id`; CreateOrder и Checkout ставят `Paid`
- [x] `POST /orders/{id}/refund`: один `SaveChanges`, не `AddMoney`; `RequireAuthorization`
- [x] Витрина возврата: `orders.html` — `Paid` → кнопка, `Refunded` → текст, после успеха обновляются список и баланс в шапке
- [x] `docker-compose.yml`: Postgres 16, `POSTGRES_DB=GameStore`, пароль `postgres` как в `appsettings.json`, named volume на `/var/lib/postgresql/data`
- [x] Строка `ConnectionStrings:GameStore` убрана из user-secrets; остался `Jwt:Key`. Иначе секрет перекрывал пароль контейнера (`28P01`)
- [x] `OnTokenValidated`: нет строки в `"Users"` → `401`, эндпоинт не зовётся. Витрина чистит сессию по `401`

### Известный техдолг (не забывать)

- [ ] В `GET /games` нет `totalCount` в ответе
- [ ] `MoneyHelper` без неймспейса `GameStore.Api.*`
- [ ] `GameValidatorTests` устарели; integration-тесты не доведены — не чинить в приоритете
- [ ] CS8604 в `JwtTokenService`: `Jwt:Key` теоретически null (в `Program.cs` уже есть throw)
- [ ] CS8604 в `OnTokenValidated`: `context.Principal` в типе библиотеки `ClaimsPrincipal?`
- [ ] README не описывает `docker compose up -d` и что команды `ef` / `run` набирают на Windows, не в контейнере
- [ ] SQL-индексы как тема разбора (на корзине уникальные уже есть)

### Дальше по проекту (приоритет)

1. **README:** запуск через Compose (остаток этапа H)
2. Алгоритмы 3–5 задач в неделю — в отдельном чате, не в этом репо
3. Мелочи API (`totalCount`) — по пути. Контейнер с самим API — не сейчас

Полный roadmap: [`plan.md`](./plan.md).

Этапы A, C, D, E закрыты. SQL (B) по запросам закрыт. G (витрина) сделана как статика, включая корзину. H: Compose с Postgres есть, текст в README ещё нет. I (алгоритмы) — параллельно.

---

## Учебные треки (параллельно проекту)

### A. GameStore (основной)

Идём по этапам в `plan.md`. После каждого этапа агент обновляет чеклист выше.

### B. SQL (поддерживать, не курс заново)

| Уровень | Темы | Статус |
| --- | --- | --- |
| 1 | SELECT, WHERE, ORDER BY, LIMIT | ✅ |
| 2 | INNER JOIN + LEFT JOIN | ✅ |
| 3 | GROUP BY, SUM, HAVING, AVG | ✅ |
| 4 | NULL / IS NULL, COALESCE, CASE | ✅ |
| 5 | IN, EXISTS / NOT EXISTS, скалярный подзапрос | ✅ |
| 6 | CTE | ✅ |
| 6b | Индексы | ~ уникальные на корзине в модели; разбор «зачем индекс» ещё можно |
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
| branch / checkout / merge | ✅ ветки `feature/frontend`, `feature/wallet`, `feature/cart`, `feature/refunds`, `feature/docker` |
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

### 2026-10-28…29 — витрина и кошелёк

- Статика `wwwroot`: каталог, поиск, логин, карточка игры, заказы, админка, кошелёк.
- `User.Balance`, пополнение, `numeric(12,2)`, списание при `POST /orders`.
- Ветки `feature/frontend`, `feature/wallet`.

### 2026-10-29 — корзина

- `Cart` + `CartItem`, уникальные индексы, Cascade на игру в корзине, Restrict на заказ.
- `OrderItem.GameName` + `UPDATE` в миграции `AddCartAndOrderName`.
- `GET /cart` (пустая → 200), `POST /cart/items` (get-or-create).
- `Id = -1` ломает identity: EF вставляет явный ключ, второй POST бьёт в `PK_Carts`. Новая шапка только с `UserId`, `Add` при `Id == 0`.
- Карта репо: `Docs/structure.md`. Прогресс дальше ведёт агент.

### 2026-10-30 — удаление и PATCH корзины

- `DELETE /cart/items/{gameId}`: `Items.Remove` + `UpdateCart`, шапка остаётся. Нет корзины или позиции → 404, без запроса в каталог.
- `PATCH /cart/items/{gameId}`: `Quantity =` из тела, не `+=`. Валидатор `> 0`. Нет позиции → 404, новую строку не создаёт.
- `POST /cart/checkout`: общий Scoped `DbContext`, один `SaveChanges`. Пустая корзина → 400, не 404.
- Витрина: `cart.html` (GET/PATCH/DELETE/checkout), ссылка в шапке, на карточке игры «В корзину» → `POST /cart/items`.
- UI 2026-10-30: заказы `OrderByDescending(CreatedAt)`; JWT 7 дней + сброс просроченной сессии; «Все игры» вертикальный список, пейджер сверху и снизу; qty +/− на игре и в корзине; шапка «Главная» + «Все игры», корзина иконкой, заказы/выход в меню почты.

### 2026-10-30 — статус заказа

- Ветка `feature/refunds` (корзину в этот коммит не мешали).
- Enum `OrderStatus`: `Paid = 0`, `Refunded = 1`. Свойство на `Order`, колонка `"OrderStatus"`, миграция `defaultValue: 0` для старых строк.
- CreateOrder и Checkout явно ставят `Paid`. В `OrderDetails` появился `Id`; в списке и в `CreateOrderResponse` есть `orderStatus`.
- Эндпоинта возврата ещё нет: статус есть, деньги назад не ходят.

### 2026-10-30 — POST refund

- `OrderService.Refund`: владелец как в `GetOrder` (чужой → 404), уже `Refunded` → 400, сумма `UnitPrice * Quantity`, корзину не трогает.
- `OrderRepository.Refund`: `Balance +=` и `OrderStatus = Refunded` в одном `SaveChanges`, без `AddMoney`.
- `POST /orders/{id}/refund` → 200 и `ToResponse()`. `.RequireAuthorization()` как у соседних заказов.
- Кнопки на витрине ещё нет.

### 2026-10-30 — витрина возврата

- `orders.html`: загрузка вынесена в `loadOrders`. `orderStatus === 0` → «Вернуть заказ», `=== 1` → «Возвращён» (ноль в JS ложный, голое `if` нельзя).
- `POST /orders/{id}/refund` с JWT. 401 → логин. Ошибка → `readApiError` и перерисовка. Успех → список + `renderNav("orders")` (баланс в шапке). Кнопка `disabled` на время запроса.

### 2026-11-01 — Docker Compose (Postgres)

- Ветка `feature/docker`. Файл `docker-compose.yml`: образ `postgres:16`, named volume, без контейнера API.
- Локальную службу Postgres на 5432 выключили: иначе Compose и `ef` попадали не в ту базу.
- `28P01`: user-secrets держали старый `ConnectionStrings:GameStore` и перекрывали `appsettings.json`. Ключ убрали, `Jwt:Key` оставили. Для GameStore база = контейнер, секрет строки больше не дёргаем.
- `dotnet ef` и `dotnet run` — в PowerShell из корня репозитория, не в контейнере. Миграции накатились, `GET /games` → 200.
- README про Compose ещё не дописан.

### 2026-11-01 — JWT: пользователь должен быть в БД

- `OnTokenValidated` после подписи берёт `UserRepository` из `RequestServices`, `GetUserById`. Нет строки — `context.Fail`, `POST /games` → 401.
- `TokenValidatedContext.Principal` в API библиотеки nullable; `GetRequiredService` null не возвращает.

---

## Как обновлять этот файл

1. Агент после закрытого шага правит чеклист, снимок, таблицу эндпоинтов, техдолг и лог.
2. Ученик поправляет, только если факт неверный.
3. Не раздувать: факты и следующий шаг важнее длинных конспектов.
4. Детальный план этапов — в `plan.md`; карта файлов — в `structure.md`; здесь — **статус**.
