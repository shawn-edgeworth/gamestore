# GameStore — прогресс обучения

> Живой файл. Обновляем после каждого учебного блока или заметного изменения в коде.
> Последнее обновление: **2026-10-27**

---

## Цель

Подготовиться к стажировке **«Разработчик .NET» (Т-Банк / Тинькофф)**: pet-проект на ASP.NET Core + закрепление C#, LINQ, SQL, Git, EF Core.

Ожидания курса (с сайта): C# / алгоритмы / базовый Git / коллекции + LINQ (`Where`/`Sum`). Плюс — свой проект. На стажировке дальше: AspNet Core, EF Core, GitLab, Docker, брокеры и т.д.

**Фокус сейчас:** backend ~80%, SQL ежедневно, простой фронт позже. Не уходить в React/TS. Тесты не углублять, пока SQL и портфолио-дыры не закрыты.

---

## Снимок проекта (факт)

| Параметр | Значение |
| --- | --- |
| Стек | .NET 10, Minimal APIs, EF Core + Npgsql, PostgreSQL, FluentValidation, JWT, xUnit |
| Решение | `GameStore.Api` + `GameStore.Api.Tests` |
| Сущности | `Game`, `Category`, `User`, `Order`, `OrderItem` |
| Auth | регистрация, логин, JWT (HS256), роль `Admin` на `POST /games` |
| Orders | `POST /orders`, `GET /orders`, `GET /orders/{id}` (свой → 200, чужой → 404) |
| Frontend / Docker / README | нет |
| Git | репозиторий, `.gitignore`, коммиты, remote `origin` на `master` |
| Архитектура | Endpoint → Service → Repository → DbContext. Один тип — один файл. Неймспейсы: Models, Services, Data, Mappers, Validation |

Эндпоинты (префикса `/api` нет):

| Метод | Путь | Auth | Статус |
| --- | --- | --- | --- |
| GET | `/games` | нет | ✅ фильтры, поиск, пагинация |
| GET | `/games/{id}` | нет | ✅ |
| POST | `/games` | Admin | ✅ |
| PUT | `/games/{id}` | нет | ✅ (роль ещё не повешена) |
| DELETE | `/games/{id}` | нет | ✅ (роль ещё не повешена) |
| PATCH | `/games/{id}/price` | нет | ✅ (роль ещё не повешена) |
| PATCH | `/games/{id}/category` | нет | ✅ (роль ещё не повешена) |
| GET | `/categories` | нет | ✅ |
| GET | `/categories/{id}` | нет | ✅ |
| POST | `/categories` | нет | ✅ |
| PUT | `/categories/{id}` | нет | ✅ |
| DELETE | `/categories/{id}` | нет | ✅ Restrict, если есть игры |
| POST | `/register` | нет | ✅ |
| POST | `/login` | нет | ✅ JWT |
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
| ASP.NET Core (pipeline, Minimal API) | + | ✓ | эндпоинты, JWT middleware, статусы, Swagger |
| EF Core / миграции | + | ✓ | Include/ThenInclude, FK, Restrict/Cascade |
| SQL | + | ~ / + | DBeaver: JOIN, GROUP BY/HAVING, подзапросы, CTE; транзакции/индексы ещё впереди |
| DTO / Entity / маппинг | + | ✓ | Models + Mappers, без циклов JSON |
| Валидация | + | ✓ | FluentValidation + `OperationResult` |
| Тесты (unit + integration) | ~ | ✓ | есть каркас; `GameValidatorTests` устарели; фокус не сейчас |
| Git (checkout/commit/push/pull) | + | ✓ | коммиты есть; ветки `feature/...` ещё слабо |
| Алгоритмы | ~ | ✓ | отдельно, 3–5 задач/неделя |
| Auth (JWT) | + | ~ / ✓ | register/login, claims, Admin, свои заказы |
| Docker | — | ~ | ближе к финалу проекта |

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
- [x] FluentValidation вместо самописных `GameValidator`
- [x] Фильтры `GET /games`: categoryId, цена, search (`ILike`), page/pageSize
- [x] Слой Service + Repository, DTO + мапперы, `OperationResult`
- [x] `[MemberNotNullWhen]` на `IsSuccess` — в эндпоинтах не дублируем `Value is null`
- [x] OpenAPI + Swagger UI; кнопка Authorize (Swashbuckle bearer)
- [x] `.gitignore`, секреты не в git (`user-secrets` для `Jwt:Key`)
- [x] Git-коммиты и remote
- [x] Users: регистрация, хеш пароля, логин, JWT
- [x] Роль Admin на создание игры
- [x] Orders: `POST /orders`, `GET /orders`, `GET /orders/{id}` (владелец / 404)
- [x] FK: Game→Category Restrict, Order→User Restrict, OrderItem→Game Restrict, OrderItem→Order Cascade
- [x] Правило «один тип — один файл» + неймспейсы по слоям

### Известный техдолг (не забывать)

- [ ] Admin на `PUT`/`DELETE`/`PATCH` игр (сейчас только `POST /games`)
- [ ] В `GET /games` нет `totalCount` в ответе (в плане этапа C была обёртка)
- [ ] В `OrderDetails` нет `Id`
- [ ] `GameValidatorTests` ссылаются на удалённый `GameValidator` — тесты красные, не чинить в приоритете
- [ ] Integration-тесты не доведены под текущий контракт
- [ ] CS8604 в `JwtTokenService`: `Jwt:Key` теоретически null (в `Program.cs` уже есть throw)
- [ ] Нет README, Docker Compose, фронта
- [ ] SQL: индексы, явные транзакции (блок SELECT/JOIN/CTE закрыт)

### Дальше по проекту (приоритет)

1. **Транзакция на заказе** — сколько `SaveChanges` в `CreateOrder`, что атомарно
2. **Git-ветки** + короткий README под портфолио
3. По желанию: Admin на остальные записи каталога
4. Алгоритмы 3–5 задач/неделя (параллельно)
5. Docker Compose + простой HTML/JS — после README
6. SQL поддерживать: раз в день короткий запрос по своей БД, не новый курс

Полный roadmap: [`plan.md`](./plan.md).

Этапы плана: A, C, D (минимум), E (обязательное) закрыты. SQL (B) по запросам закрыт; индексы/транзакции — хвост. F/G/H ещё впереди.

---

## Учебные треки (параллельно проекту)

### A. GameStore (основной)

Идём по этапам в `plan.md`. После каждого этапа обновляем чеклист выше.

### B. SQL (ежедневно 20–40 мин)

На базе GameStore в DBeaver:

| Уровень | Темы | Статус |
| --- | --- | --- |
| 1 | SELECT, WHERE, ORDER BY, LIMIT | ✅ |
| 2 | INNER JOIN + LEFT JOIN (игры без покупок; продажи с нулями) | ✅ |
| 3 | GROUP BY, SUM, HAVING, AVG | ✅ |
| 4 | NULL / IS NULL, COALESCE, CASE | ✅ |
| 5 | IN, EXISTS / NOT EXISTS, скалярный подзапрос | ✅ |
| 6 | CTE (`WITH`); индексы; транзакции | ~ CTE есть; индексы и транзакции нет |

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
- Тесты сознательно отошли на второй план.

### 2026-10-23…25 — Users, JWT, Admin

- Users, хеш пароля, логин, JWT, Swagger bearer.
- `POST /games` только с ролью Admin.

### 2026-10-25…26 — Orders, FK, неймспейсы, ошибки

- Заказы, Restrict/Cascade, свои заказы, `GET /orders/{id}` (чужой → 404).
- `MemberNotNullWhen` на `OperationResult`: эндпоинт не дублирует `Value is null`.
- 401 = нет/битый токен; 404 = нет ресурса или не твой.

### 2026-10-26…27 — SQL с нуля (DBeaver)

- SELECT, WHERE, ORDER BY, LIMIT на `"Games"` (8 игр).
- JOIN игр с категориями; цепочка заказы → пользователи → позиции → игры (4 строки чека).
- GROUP BY + SUM: в SELECT только ключ группы или агрегат. HAVING — фильтр корзин (сумма > 100).
- LEFT JOIN + IS NULL — игры без покупок; SUM + COALESCE — штуки с нулями.
- CASE sold / not sold. AVG: без GROUP BY — одна цифра на таблицу.
- Подзапрос: цена > среднего (внутренний SELECT без внешнего псевдонима `g`).
- IN список id категорий; EXISTS / NOT EXISTS как LINQ `Any` / `!Any`.
- CTE: это таблица, не переменная; `FROM` должен включать то, из чего берёшь колонки. Для среднего — `(SELECT average FROM a)`. Для одного фильтра сумм проще HAVING, чем WITH.
- CROSS JOIN слышали (одна строка × все игры), отдельно не тренировали.
- Дальше: транзакция в `CreateOrder` (`SaveChanges`).

---

## Как обновлять этот файл

1. После кода — отметить чеклист «Сделано» / техдолг.
2. После учебы без кода — строка в «Лог сессий» + матрица навыков.
3. Не раздувать: факты и следующий шаг важнее длинных конспектов.
4. Детальный план этапов держим в `plan.md`; здесь — **статус**.
