const tokenKey = "gamestore.token";
const emailKey = "gamestore.email";
const roleKey = "gamestore.role";

const placeholderImage =
    "data:image/svg+xml," +
    encodeURIComponent(
        '<svg xmlns="http://www.w3.org/2000/svg" width="160" height="110">' +
        '<rect width="100%" height="100%" fill="#d9d9d9"/>' +
        '<text x="50%" y="50%" dominant-baseline="middle" text-anchor="middle"' +
        ' font-family="sans-serif" font-size="14" fill="#666">Нет фото</text></svg>'
    );

const cartIconSvg =
    '<svg xmlns="http://www.w3.org/2000/svg" width="22" height="22" fill="none" ' +
    'stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" ' +
    'viewBox="0 0 24 24" aria-hidden="true">' +
    '<circle cx="9" cy="21" r="1"/><circle cx="20" cy="21" r="1"/>' +
    '<path d="M1 1h4l2.68 13.39a2 2 0 0 0 2 1.61h9.72a2 2 0 0 0 2-1.61L23 6H6"/>' +
    "</svg>";

function getToken() {
    return localStorage.getItem(tokenKey);
}

function getEmail() {
    return localStorage.getItem(emailKey);
}

function getRole() {
    return localStorage.getItem(roleKey);
}

function isAdmin() {
    return getRole() === "Admin";
}

function readTokenExpiry(token) {
    try {
        const payload = token.split(".")[1];
        const json = JSON.parse(atob(payload.replace(/-/g, "+").replace(/_/g, "/")));
        return typeof json.exp === "number" ? json.exp * 1000 : 0;
    } catch {
        return 0;
    }
}

function isLoggedIn() {
    const token = getToken();
    if (!token) {
        return false;
    }

    const expiry = readTokenExpiry(token);
    if (!expiry || Date.now() >= expiry) {
        clearSession();
        return false;
    }

    return true;
}

function saveSession(token, email, role) {
    localStorage.setItem(tokenKey, token);
    localStorage.setItem(emailKey, email);
    if (role) {
        localStorage.setItem(roleKey, role);
    } else {
        localStorage.removeItem(roleKey);
    }
}

function clearSession() {
    localStorage.removeItem(tokenKey);
    localStorage.removeItem(emailKey);
    localStorage.removeItem(roleKey);
}

function currentPath() {
    return window.location.pathname + window.location.search;
}

function redirectToLogin() {
    window.location.href = "/login.html?next=" + encodeURIComponent(safeNextPath(currentPath()));
}

function authHeaders() {
    const headers = { "Content-Type": "application/json" };
    const token = getToken();
    if (token) {
        headers.Authorization = "Bearer " + token;
    }

    return headers;
}

function formatMoney(value) {
    const amount = Number(value);
    if (Number.isNaN(amount)) {
        return "$0";
    }

    return "$" + amount.toFixed(2);
}

function createGameImage(url, alt) {
    const img = document.createElement("img");
    img.alt = alt ?? "";
    img.src = url || placeholderImage;
    img.addEventListener("error", () => {
        img.src = placeholderImage;
    });
    return img;
}

function isCatalogListPage() {
    const path = window.location.pathname.replace(/\\/g, "/");
    return /\/(games|search)\.html$/i.test(path);
}

function renderGames(root, games, layout) {
    const mode = layout === "list" || root.dataset.layout === "list" || isCatalogListPage()
        ? "list"
        : (layout || "grid");
    root.replaceChildren();
    root.classList.remove("game-grid", "game-list");
    root.classList.add(mode === "list" ? "game-list" : "game-grid");

    for (const game of games) {
        const article = document.createElement("article");
        article.className = "game-card";

        const link = document.createElement("a");
        link.href = "/game.html?id=" + game.id;
        link.className = "game-card-link";

        const title = document.createElement("h2");
        title.textContent = game.name;

        const meta = document.createElement("p");
        const category = game.categoryName ?? "Без категории";
        meta.textContent = category + " · " + formatMoney(game.price);

        const text = document.createElement("div");
        text.append(title, meta);

        link.append(createGameImage(game.imageUrl, game.name), text);
        article.append(link);
        root.append(article);
    }
}

async function fetchGames(params) {
    const query = new URLSearchParams();

    if (params.page) {
        query.set("page", String(params.page));
    }

    if (params.pageSize) {
        query.set("pageSize", String(params.pageSize));
    }

    if (params.search) {
        query.set("search", params.search);
    }

    const suffix = query.toString();
    const url = suffix ? "/games?" + suffix : "/games";
    const response = await fetch(url);

    if (!response.ok) {
        throw new Error("Не удалось загрузить игры: HTTP " + response.status);
    }

    const data = await response.json();
    return data.games ?? [];
}

async function fetchCategories() {
    const response = await fetch("/categories");
    if (!response.ok) {
        throw new Error("Не удалось загрузить категории: HTTP " + response.status);
    }

    const data = await response.json();
    return data.categories ?? [];
}

async function fetchCurrentUser() {
    const response = await fetch("/users/me", { headers: authHeaders() });
    if (response.status === 401) {
        clearSession();
        return null;
    }

    if (!response.ok) {
        throw new Error(await readApiError(response));
    }

    return await response.json();
}

async function fetchCart() {
    const response = await fetch("/cart", { headers: authHeaders() });
    if (response.status === 401) {
        clearSession();
        return null;
    }

    if (!response.ok) {
        throw new Error(await readApiError(response));
    }

    return await response.json();
}

async function addCartItem(gameId, quantity) {
    return fetch("/cart/items", {
        method: "POST",
        headers: authHeaders(),
        body: JSON.stringify({ gameId, quantity })
    });
}

async function setCartItemQuantity(gameId, quantity) {
    return fetch("/cart/items/" + gameId, {
        method: "PATCH",
        headers: authHeaders(),
        body: JSON.stringify({ quantity })
    });
}

async function deleteCartItem(gameId) {
    return fetch("/cart/items/" + gameId, {
        method: "DELETE",
        headers: authHeaders()
    });
}

function createQtyStepper(quantity, handlers) {
    const wrap = document.createElement("div");
    wrap.className = "qty-stepper";

    const minus = document.createElement("button");
    minus.type = "button";
    minus.className = "outline qty-btn";
    minus.textContent = "−";
    minus.setAttribute("aria-label", "Уменьшить");
    minus.addEventListener("click", () => handlers.onDelta(-1));

    const input = document.createElement("input");
    input.type = "number";
    input.min = "1";
    input.step = "1";
    input.value = String(quantity);
    input.setAttribute("aria-label", "Количество");
    input.addEventListener("keydown", (event) => {
        if (event.key === "Enter") {
            event.preventDefault();
            handlers.onSet(Number(input.value));
        }
    });
    input.addEventListener("change", () => {
        handlers.onSet(Number(input.value));
    });

    const plus = document.createElement("button");
    plus.type = "button";
    plus.className = "outline qty-btn";
    plus.textContent = "+";
    plus.setAttribute("aria-label", "Увеличить");
    plus.addEventListener("click", () => handlers.onDelta(1));

    wrap.append(minus, input, plus);
    return wrap;
}

async function readApiError(response) {
    const raw = await response.text();

    if (!raw) {
        return "Ошибка HTTP " + response.status;
    }

    try {
        const parsed = JSON.parse(raw);
        if (typeof parsed === "string") {
            return parsed;
        }
    } catch {
        return raw;
    }

    return raw;
}

function createCartLink(current) {
    const link = document.createElement("a");
    link.href = "/cart.html";
    link.className = "nav-cart";
    link.dataset.nav = "cart";
    link.setAttribute("aria-label", "Корзина");
    link.innerHTML = cartIconSvg;
    if (current === "cart") {
        link.setAttribute("aria-current", "page");
    }

    return link;
}

async function renderNav(current) {
    const cartSlot = document.getElementById("nav-cart");
    const walletSlot = document.getElementById("nav-wallet");
    const accountSlot = document.getElementById("nav-account");
    if (!accountSlot) {
        return;
    }

    cartSlot?.replaceChildren();
    walletSlot?.replaceChildren();
    accountSlot.replaceChildren();

    if (!isLoggedIn()) {
        const login = document.createElement("a");
        login.href = "/login.html";
        login.textContent = "Вход";
        if (current === "login") {
            login.setAttribute("aria-current", "page");
        }

        accountSlot.append(login);
        return;
    }

    if (cartSlot) {
        cartSlot.append(createCartLink(current));
    }

    const wallet = document.createElement("a");
    wallet.href = "/wallet.html";
    wallet.className = "nav-wallet";
    wallet.textContent = formatMoney(0);
    if (current === "wallet") {
        wallet.setAttribute("aria-current", "page");
    }

    if (walletSlot) {
        walletSlot.append(wallet);
    }

    try {
        const me = await fetchCurrentUser();
        if (!me) {
            await renderNav(current);
            return;
        }

        wallet.textContent = formatMoney(me.balance);
    } catch {
        wallet.textContent = "Кошелёк";
    }

    const menu = document.createElement("details");
    menu.className = "dropdown";

    const summary = document.createElement("summary");
    summary.textContent = getEmail() ?? "Аккаунт";

    const dropdown = document.createElement("ul");

    const ordersItem = document.createElement("li");
    const ordersLink = document.createElement("a");
    ordersLink.href = "/orders.html";
    ordersLink.dataset.nav = "orders";
    ordersLink.textContent = "Заказы";
    if (current === "orders") {
        ordersLink.setAttribute("aria-current", "page");
    }

    ordersItem.append(ordersLink);

    const logoutItem = document.createElement("li");
    const logout = document.createElement("a");
    logout.href = "#";
    logout.textContent = "Выйти";
    logout.addEventListener("click", (event) => {
        event.preventDefault();
        clearSession();
        window.location.href = "/";
    });
    logoutItem.append(logout);

    dropdown.append(ordersItem, logoutItem);
    menu.append(summary, dropdown);
    accountSlot.append(menu);
}

function markCurrentNav(current) {
    const links = document.querySelectorAll("[data-nav]");
    for (const link of links) {
        if (link.getAttribute("data-nav") === current) {
            link.setAttribute("aria-current", "page");
        }
    }
}

function requireLogin() {
    if (!isLoggedIn()) {
        redirectToLogin();
        return false;
    }

    return true;
}

function safeNextPath(value) {
    if (!value || !value.startsWith("/") || value.startsWith("//")) {
        return "/";
    }

    return value;
}

function requireAdmin() {
    if (!requireLogin()) {
        return false;
    }

    if (!isAdmin()) {
        window.location.href = "/";
        return false;
    }

    return true;
}
