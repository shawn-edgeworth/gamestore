# GameStore — прогресс обучения

> Живой файл. Обновляем после каждого учебного блока или заметного изменения в коде.
> Последнее обновление: **2026-10-13**

---

## Цель

Подготовиться к стажировке **«Разработчик .NET» (Т-Банк / Тинькофф)**: pet-проект на ASP.NET Core + закрепление C#, LINQ, SQL, Git, EF Core.

Ожидания курса (с сайта): C# / алгоритмы / базовый Git / коллекции + LINQ (`Where`/`Sum`). Плюс — свой проект. На стажировке дальше: AspNet Core, EF Core, GitLab, Docker, брокеры и т.д.

**Фокус сейчас:** backend ~80%, простой фронт позже ~20%. Не уходить в React/TS до стабильного API.

---



## Снимок проекта (факт)


| Параметр                     | Значение                                                      |
| ---------------------------- | ------------------------------------------------------------- |
| Стек                         | .NET 10, Minimal APIs, EF Core + Npgsql, PostgreSQL, xUnit    |
| Решение                      | `GameStore.Api` + `GameStore.Api.Tests`                       |
| Сущности                     | `Game`, `Category` (1→N, Category обязателен)                 |
| Auth / Users / Orders / Cart | нет                                                           |
| Frontend / Docker / README   | нет                                                           |
| Git                          | репозиторий есть, **коммитов ещё нет**, `.gitignore` нет      |
| Архитектура                  | Endpoint → Service → Repository → DbContext (без интерфейсов) |


Эндпоинты (префикса `/api` нет):


| Метод      | Путь                   | Статус |
| ---------- | ---------------------- | ------ |
| GET        | `/games`               | ✅      |
| GET        | `/games/{id}`          | ✅      |
| POST       | `/games`               | ✅      |
| PUT        | `/games/{id}`          | ✅      |
| DELETE     | `/games/{id}`          | ✅      |
| PATCH      | `/games/{id}/price`    | ✅      |
| PATCH      | `/games/{id}/category` | ✅      |
| GET        | `/categories`          | ✅      |
| GET        | `/categories/{id}`     | ✅      |
| POST       | `/categories`          | ✅      |
| PUT/DELETE | `/categories/...`      | ❌      |


---



## Матрица навыков (самооценка → цель)

Легенда: `—` слабо · `~` база · `+` уверенно · `✓` для стажировки достаточно на старте


| Тема                                 | Сейчас | Цель к экзамену | Где закрепляем                           |
| ------------------------------------ | ------ | --------------- | ---------------------------------------- |
| C# основы                            | +      | ✓               | весь проект                              |
| Коллекции / LINQ                     | ~      | ✓               | репозитории, SQL↔LINQ, алгоритмы         |
| async/await                          | ~      | ✓               | сервисы, EF `*Async`                     |
| DI / lifetimes                       | ~      | ✓               | `Program.cs`, Scoped DbContext           |
| ASP.NET Core (pipeline, Minimal API) | ~      | ✓               | эндпоинты, статусы, OpenAPI              |
| EF Core / миграции                   | ~      | ✓               | модели, миграции, Include                |
| SQL                                  | —      | ~ / +           | DBeaver на своей БД, ежедневно           |
| DTO / Entity / маппинг               | ~      | ✓               | Models + Mappers                         |
| Валидация                            | ~      | ✓               | свои валидаторы → потом FluentValidation |
| Тесты (unit + integration)           | ~      | ✓               | починить и расширить                     |
| Git (checkout/commit/push/pull)      | ~      | ✓               | первые коммиты + ветки                   |
| Алгоритмы                            | ~      | ✓               | отдельно, 3–5 задач/неделя               |
| Auth (JWT / Identity)                | —      | ~               | этап после стабильного CRUD              |
| Docker                               | —      | ~               | ближе к финалу проекта                   |


---



## Чеклист проекта



### Сделано

- [x] Каркас ASP.NET Core + PostgreSQL + connection string
- [x] EF Core: `GameDbContext`, сущности `Game` / `Category`
- [x] Миграции: Initial → детали игры → Categories → Category required
- [x] CRUD игр + PATCH цены и категории
- [x] Частичный API категорий (list/get/create)
- [x] Слой Service + Repository
- [x] Request/Response DTO + мапперы
- [x] Кастомная валидация + `OperationResult`
- [x] OpenAPI + Swagger UI (Development)
- [x] Тестовый проект: unit (`GameValidator`) + integration (`WebApplicationFactory` + InMemory)



### Известный техдолг (не забывать)

- [x] В response DTO нет `Id` — ломает клиент и, вероятно, integration-тесты
- [x] При чтении игр нет `Include(Category)` → `CategoryName` часто null
- [x] При создании/обновлении игры нет явной проверки существования `CategoryId`
- [ ] Category: нет update/delete
- [x] FluentValidation в пакетах, но не используется
- [ ] `GameStore.Api.http` — устаревший weatherforecast
- [x] Нет `.gitignore`, пароль БД в `appsettings.json`
- [x] Нет ни одного git-коммита
- [ ] Нет тестов на Categories
- [ ] Integration-тесты, похоже, устарели относительно DTO



### Дальше по проекту (приоритет)

1. **Стабилизация** — Id в DTO, Include, проверка CategoryId, починить тесты
2. **Git-гигиена** — `.gitignore`, первый коммит, привычка commit/branch
3. **SQL-практика** на своей БД (JOIN Games↔Categories)
4. **Users + Auth (JWT)** — минимум регистрация/логин
5. **Orders / OrderItems** (+ транзакции)
6. Простой HTML/JS фронт на `fetch`
7. Docker Compose (API + Postgres)
8. README под портфолио

Полный roadmap: `[plan.md](./plan.md)`.

---



## Учебные треки (параллельно проекту)



### A. GameStore (основной)

Идём по этапам в `plan.md`. После каждого этапа обновляем чеклист выше.

### B. SQL (ежедневно 20–40 мин)

На базе GameStore в DBeaver:


| Уровень | Темы                                             | Статус |
| ------- | ------------------------------------------------ | ------ |
| 1       | SELECT, WHERE, ORDER BY, LIMIT                   | ☐      |
| 2       | INNER/LEFT JOIN (Games + Categories)             | ☐      |
| 3       | GROUP BY, HAVING, COUNT, SUM, AVG                | ☐      |
| 4       | NULL, COALESCE, CASE                             | ☐      |
| 5       | IN, EXISTS, подзапросы                           | ☐      |
| 6       | CTE, индексы, транзакции (когда появятся Orders) | ☐      |




### C. Алгоритмы (для отбора)

3–5 задач в неделю (LeetCode Easy/Medium или ULearn/Яндекс). Фокус: массивы, строки, словари, два указателя, сортировка.


| Неделя | Тема | Сделано |
| ------ | ---- | ------- |
| …      | …    | ☐       |




### D. Git


| Навык                          | Статус                  |
| ------------------------------ | ----------------------- |
| status / add / commit          | ☐ практика на этом репо |
| branch / checkout / merge      | ☐                       |
| push / pull / remote           | ☐                       |
| осмысленные сообщения коммитов | ☐                       |


---



## Лог сессий

Краткие записи: дата → что сделали → что поняли → что дальше.

### 2026-10-13 — стартовый аудит

- Зафиксировано фактическое состояние репо vs старый план из ChatGPT.
- Созданы `progress.md` + обновлённый `plan.md`.
- Следующий шаг: стабилизация API (Id, Include, CategoryId) + `.gitignore` и первый коммит.



### YYYY-MM-DD — кратко

- Сделали:
- Закрепили (концепции):
- Открытые вопросы:
- Следующий шаг:
-->

---



## Как обновлять этот файл

1. После кода — отметить чеклист «Сделано» / техдолг.
2. После учебы без кода — строка в «Лог сессий» + матрица навыков.
3. Не раздувать: факты и следующий шаг важнее длинных конспектов.
4. Детальный план этапов держим в `plan.md`; здесь — **статус**.

