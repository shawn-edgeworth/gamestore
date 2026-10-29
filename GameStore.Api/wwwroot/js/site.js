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

function isLoggedIn() {
    return Boolean(getToken());
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

function renderGames(root, games) {
    root.replaceChildren();
    root.classList.add("game-grid");

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

        link.append(createGameImage(game.imageUrl, game.name), title, meta);
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
    if (!response.ok) {
        throw new Error(await readApiError(response));
    }

    return await response.json();
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

async function renderNav(current) {
    const host = document.getElementById("nav-auth");
    if (!host) {
        return;
    }

    host.replaceChildren();

    const email = getEmail();
    const token = getToken();

    if (token && email) {
        host.className = "nav-auth";

        const name = document.createElement("span");
        name.textContent = email;

        const wallet = document.createElement("a");
        wallet.href = "/wallet.html";
        wallet.textContent = formatMoney(0);
        if (current === "wallet") {
            wallet.setAttribute("aria-current", "page");
        }

        try {
            const me = await fetchCurrentUser();
            wallet.textContent = formatMoney(me.balance);
        } catch {
            wallet.textContent = "Кошелёк";
        }

        const logout = document.createElement("button");
        logout.type = "button";
        logout.textContent = "Выйти";
        logout.addEventListener("click", () => {
            clearSession();
            window.location.href = "/";
        });

        host.append(name, wallet, logout);
        return;
    }

    const login = document.createElement("a");
    login.href = "/login.html";
    login.textContent = "Вход";
    if (current === "login") {
        login.setAttribute("aria-current", "page");
    }

    host.append(login);
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
        window.location.href = "/login.html";
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
