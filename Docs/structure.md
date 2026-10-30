# GameStore — карта репозитория

Навигация для ментора и для себя. Статус фич — в [`progress.md`](./progress.md). Roadmap — в [`plan.md`](./plan.md). Правило ментора — `.ai/base-rule.md` и `.cursor/rules/mentor-workflow.mdc`.

Префикса `/api` нет. Неймспейс задаёт **слой**, не папка: `Models/Cart/Cart.cs` всё равно `GameStore.Api.Models`.

---

## Корень

| Путь | Зачем |
| --- | --- |
| `GameStore.sln` | Решение: API + тесты |
| `GameStore.Api/` | Весь backend и статика `wwwroot` |
| `GameStore.Api.Tests/` | xUnit + `WebApplicationFactory`; не приоритет |
| `Docs/` | `plan.md`, `progress.md`, этот файл |
| `README.md` | Запуск, user-secrets, миграции, Swagger |
| `.ai/base-rule.md` | Полное правило ментора |

---

## Слои `GameStore.Api`

Поток: `Program.cs` (endpoint) → `Services` → `Data` (репозиторий + `GameDbContext`) → PostgreSQL. В JSON уходит DTO из `Mappers`, не сущность.

| Папка | Неймспейс | Роль |
| --- | --- | --- |
| `Program.cs` | (top-level) | DI, JWT, pipeline, все `MapGet`/`MapPost`. `TryGetUserId`, `ToHttpError` |
| `Models/{Game,Category,User,Order,Cart}/` | `GameStore.Api.Models` | Сущности и DTO, **один тип — один файл** |
| `Services/` | `GameStore.Api.Services` | Валидация, правила, `OperationResult<T>` |
| `Data/` | `GameStore.Api.Data` | Репозитории, `GameDbContext` |
| `Mappers/` | `GameStore.Api.Mappers` | Entity → response DTO |
| `Validation/` | `GameStore.Api.Validation` | FluentValidation; подхватывается `AddValidatorsFromAssemblyContaining` |
| `Helpers/MoneyHelper.cs` | (глобальный) | `Precision = 12`, `Scale = 2` для денег |
| `Migrations/` | `GameStore.Api.Migrations` | Только EF; руками править `Sql` backfill, не снимок |
| `wwwroot/` | — | Статика: `UseDefaultFiles` + `UseStaticFiles` |

---

## Где искать фичу

| Тема | Файлы |
| --- | --- |
| Каталог игр | `Models/Game/*`, `GameService`, `GameRepository`, `GameMapper`, фильтр `GameQuery` |
| Категории | `Models/Category/*`, `CategoryService`, `CategoryRepository` |
| Пользователи, JWT, кошелёк | `Models/User/*`, `UserService`, `UserRepository`, `JwtTokenService`. Баланс: `GET /users/me`, `POST /users/me/balance`, списание в `OrderRepository.CreateOrder` |
| Заказы | `Models/Order/*` (`OrderStatus` Paid/Refunded), `OrderService`, `OrderRepository`, `OrderMapper`. Чек: снимок `OrderItem.GameName` + `UnitPrice`, не живой `Game.Name`. JSON-поле статуса: `orderStatus` |
| Корзина | `Models/Cart/*`, `CartService`, `CartRepository`, `CartMapper`. Черновик: имя/цена с `Game`. Одна корзина на `UserId` (уникальный индекс) |
| Деньги в БД | `GameDbContext` `HasPrecision`, миграции `AddUserBalance` / `*MoneyPrecision` |
| Связи FK | `GameDbContext.OnModelCreating`: заказ↔игра **Restrict**, корзина↔игра **Cascade**, заказ→позиции **Cascade** |
| Ошибки HTTP | `Services/GameOperationResult.cs` (`OperationResult<T>`), `OperationErrorType`, `ToHttpError` в `Program.cs` |
| Витрина | `wwwroot/*.html` (`cart.html`, `game.html` «В корзину», `orders.html` «Вернуть заказ»), `wwwroot/js/{site,layout,admin}.js`, `wwwroot/css/site.css` |

---

## Инварианты (не ломать)

- JWT: issuer `GameStore`, audience `GameStore.Api`, claim пользователя — `ClaimTypes.NameIdentifier`.
- Каталог GET публичный; запись игр/категорий — роль `Admin`; заказы, кошелёк, корзина, возврат — любой валидный JWT.
- Чужой заказ → **404**, не 403.
- Новый заказ: `OrderStatus = Paid`. Колонка `"OrderStatus"` — int, старые строки после миграции `0`.
- Возврат: полный, сумма со снимка `OrderItem`, повтор → **400**, не второй плюс к балансу. Заказ не удалять, корзину не заполнять.
- Один `SaveChangesAsync` на одну бизнес-операцию (заказ, пополнение, правка корзины, возврат).
- `GET /cart` не делает `INSERT`. Нет строки `"Carts"` → 200 и пустой список позиций.
- Новая корзина: `new Cart { UserId = userId }`, **`Id` не задавать** (0 = identity). `Carts.Add` только если `cart.Id == 0`.
- Повтор той же игры в корзине: `Quantity +=`, не вторая строка (`CartId` + `GameId` уникальны).
- `GET /cart` пустую корзину не путать с записью: для `AddItem` звать `cartRepository.GetCart`, не сервисный `GetCart`.

---

## Эндпоинты → метод

Все маршруты объявлены в `Program.cs`.

| Метод | Путь | Сервис |
| --- | --- | --- |
| CRUD + PATCH | `/games`, `/categories` | `GameService`, `CategoryService` |
| POST | `/register`, `/login` | `UserService` |
| GET/POST | `/users/me`, `/users/me/balance` | `UserService` |
| POST/GET | `/orders`, `/orders/{id}` | `OrderService`. Список и деталка отдают `id` и `orderStatus` |
| POST | `/orders/{id}/refund` | `OrderService.Refund` → `OrderRepository.Refund` |
| GET | `/cart` | `CartService.GetCart` |
| POST | `/cart/items` | `CartService.AddItem` |
| DELETE | `/cart/items/{gameId}` | `CartService.RemoveItem` |
| PATCH | `/cart/items/{gameId}` | `CartService.UpdateItem` |
| POST | `/cart/checkout` | `OrderService.Checkout` → `OrderRepository.Checkout` |

POST `/cart/items` прибавляет количество, PATCH ставит его. Checkout не вызывает `CreateOrder`: один `SaveChanges` на заказ, баланс и `cart.Items.Clear()`. `POST /orders` с телом живёт отдельно.

---

## Таблицы (имена EF)

| Таблица | Заметки |
| --- | --- |
| `"Games"`, `"Categories"`, `"Users"`, `"Orders"` | plural |
| `"OrderItem"`, `"CartItem"` | singular |
| `"Carts"` | plural; уникальный `"UserId"` |

Миграция корзины и снимка имени: `Migrations/20261029161634_AddCartAndOrderName.cs` (`UPDATE` `"OrderItem"."GameName"` из `"Games"`).
