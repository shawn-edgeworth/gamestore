function parseSteamAppId(value) {
    const trimmed = value.trim();
    if (/^\d+$/.test(trimmed)) {
        return trimmed;
    }

    const match = trimmed.match(/\/app\/(\d+)/i);
    return match ? match[1] : null;
}

function stripHtmlText(value) {
    if (!value) {
        return "";
    }

    const box = document.createElement("div");
    box.innerHTML = value;
    return (box.textContent || "").trim();
}

function steamPriceValue(data) {
    if (data.is_free) {
        return 0;
    }

    const cents = data.price_overview && data.price_overview.final;
    if (typeof cents !== "number") {
        return 0;
    }

    return cents / 100;
}

function steamReleaseDateValue(releaseDate) {
    if (!releaseDate || !releaseDate.date) {
        return "";
    }

    const parsed = new Date(releaseDate.date);
    if (Number.isNaN(parsed.getTime())) {
        return "";
    }

    const year = parsed.getFullYear();
    const month = String(parsed.getMonth() + 1).padStart(2, "0");
    const day = String(parsed.getDate()).padStart(2, "0");
    return year + "-" + month + "-" + day;
}

function pickCategoryId(categories, genres) {
    if (!categories.length) {
        return "";
    }

    const genreNames = (genres ?? [])
        .map((genre) => String(genre.description ?? "").trim().toLowerCase())
        .filter(Boolean);

    const matched = categories.find((category) =>
        genreNames.includes(category.name.trim().toLowerCase())
    );

    if (matched) {
        return String(matched.id);
    }

    const index = Math.floor(Math.random() * categories.length);
    return String(categories[index].id);
}

function applySteamGameToForm(form, data, categories) {
    form.gameId.value = "";
    form.name.value = data.name ?? "";
    form.price.value = steamPriceValue(data).toFixed(2);
    form.description.value = stripHtmlText(data.short_description || data.detailed_description || "");
    form.releaseDate.value = steamReleaseDateValue(data.release_date);
    form.imageUrl.value = data.header_image ?? "";
    form.categoryId.value = pickCategoryId(categories, data.genres);
}

function fillCategorySelect(select, categories, selectedId) {
    select.replaceChildren();

    const empty = document.createElement("option");
    empty.value = "";
    empty.textContent = "Категория";
    select.append(empty);

    for (const category of categories) {
        const option = document.createElement("option");
        option.value = String(category.id);
        option.textContent = category.name;
        if (selectedId && Number(selectedId) === category.id) {
            option.selected = true;
        }

        select.append(option);
    }
}

function formToGame(form) {
    const releaseDate = form.releaseDate.value;
    return {
        name: form.name.value.trim(),
        price: Number(form.price.value),
        description: form.description.value.trim() || null,
        releaseDate: releaseDate || null,
        imageUrl: form.imageUrl.value.trim() || null,
        categoryId: Number(form.categoryId.value)
    };
}

async function loadGamesAdmin() {
    const gameForm = document.getElementById("game-form");
    if (!gameForm || !requireAdmin()) {
        return;
    }

    const status = document.getElementById("admin-status");
    const gamesBody = document.getElementById("games-body");
    const categorySelect = gameForm.categoryId;
    const steamUrl = document.getElementById("steam-url");
    const steamParse = document.getElementById("steam-parse");
    let categories = [];

    async function reload() {
        status.textContent = "";
        const loaded = await Promise.all([fetchGames({}), fetchCategories()]);
        const games = loaded[0];
        categories = loaded[1];
        fillCategorySelect(categorySelect, categories, gameForm.categoryId.value);
        gamesBody.replaceChildren();

        for (const game of games) {
            const row = document.createElement("tr");

            const picture = document.createElement("td");
            picture.append(createGameImage(game.imageUrl, game.name));

            const name = document.createElement("td");
            name.textContent = game.name;

            const price = document.createElement("td");
            price.textContent = formatMoney(game.price);

            const category = document.createElement("td");
            category.textContent = game.categoryName ?? "";

            const actions = document.createElement("td");
            const edit = document.createElement("button");
            edit.type = "button";
            edit.textContent = "Изменить";
            edit.addEventListener("click", () => {
                gameForm.gameId.value = String(game.id);
                gameForm.name.value = game.name;
                gameForm.price.value = String(game.price);
                gameForm.description.value = game.description ?? "";
                gameForm.releaseDate.value = game.releaseDate ?? "";
                gameForm.imageUrl.value = game.imageUrl ?? "";
                gameForm.categoryId.value = String(game.categoryId ?? "");
                gameForm.scrollIntoView({ behavior: "smooth" });
            });

            const remove = document.createElement("button");
            remove.type = "button";
            remove.className = "secondary";
            remove.textContent = "Удалить";
            remove.addEventListener("click", async () => {
                if (!window.confirm("Удалить игру «" + game.name + "»?")) {
                    return;
                }

                const response = await fetch("/games/" + game.id, {
                    method: "DELETE",
                    headers: authHeaders()
                });

                if (!response.ok) {
                    status.textContent = await readApiError(response);
                    return;
                }

                gameForm.reset();
                gameForm.gameId.value = "";
                await reload();
            });

            actions.append(edit, remove);
            row.append(picture, name, price, category, actions);
            gamesBody.append(row);
        }
    }

    gameForm.addEventListener("submit", async (event) => {
        event.preventDefault();
        const body = formToGame(gameForm);
        const id = gameForm.gameId.value;
        const url = id ? "/games/" + id : "/games";
        const method = id ? "PUT" : "POST";

        const response = await fetch(url, {
            method,
            headers: authHeaders(),
            body: JSON.stringify(body)
        });

        if (!response.ok) {
            status.textContent = await readApiError(response);
            return;
        }

        gameForm.reset();
        gameForm.gameId.value = "";
        await reload();
    });

    document.getElementById("game-reset").addEventListener("click", () => {
        gameForm.reset();
        gameForm.gameId.value = "";
    });

    async function fillFromSteam() {
        const appId = parseSteamAppId(steamUrl.value);
        if (!appId) {
            status.textContent = "Вставьте ссылку вида https://store.steampowered.com/app/2429450/Woodcutter/";
            return;
        }

        steamParse.disabled = true;
        status.textContent = "Загружаю данные Steam…";

        try {
            const response = await fetch("/steam/appdetails?appids=" + encodeURIComponent(appId), {
                headers: authHeaders()
            });

            if (!response.ok) {
                status.textContent = await readApiError(response);
                return;
            }

            const payload = await response.json();
            const entry = payload[appId];
            if (!entry || !entry.success || !entry.data) {
                status.textContent = "Steam не вернул игру для app id " + appId;
                return;
            }

            applySteamGameToForm(gameForm, entry.data, categories);
            status.textContent = "Поля заполнены из Steam. Проверьте и нажмите «Сохранить игру».";
        } catch (error) {
            status.textContent = error.message;
        } finally {
            steamParse.disabled = false;
        }
    }

    if (steamParse && steamUrl) {
        steamParse.addEventListener("click", fillFromSteam);
        steamUrl.addEventListener("keydown", (event) => {
            if (event.key === "Enter") {
                event.preventDefault();
                fillFromSteam();
            }
        });
    }

    try {
        await reload();
    } catch (error) {
        status.textContent = error.message;
    }
}

async function loadCategoriesAdmin() {
    const categoryForm = document.getElementById("category-form");
    if (!categoryForm || !requireAdmin()) {
        return;
    }

    const status = document.getElementById("admin-status");
    const categoriesBody = document.getElementById("categories-body");

    async function reload() {
        status.textContent = "";
        const categories = await fetchCategories();
        categoriesBody.replaceChildren();

        for (const category of categories) {
            const row = document.createElement("tr");

            const name = document.createElement("td");
            name.textContent = category.name;

            const actions = document.createElement("td");
            const edit = document.createElement("button");
            edit.type = "button";
            edit.textContent = "Изменить";
            edit.addEventListener("click", () => {
                categoryForm.categoryId.value = String(category.id);
                categoryForm.name.value = category.name;
                categoryForm.scrollIntoView({ behavior: "smooth" });
            });

            const remove = document.createElement("button");
            remove.type = "button";
            remove.className = "secondary";
            remove.textContent = "Удалить";
            remove.addEventListener("click", async () => {
                if (!window.confirm("Удалить категорию «" + category.name + "»?")) {
                    return;
                }

                const response = await fetch("/categories/" + category.id, {
                    method: "DELETE",
                    headers: authHeaders()
                });

                if (!response.ok) {
                    status.textContent = await readApiError(response);
                    return;
                }

                categoryForm.reset();
                categoryForm.categoryId.value = "";
                await reload();
            });

            actions.append(edit, remove);
            row.append(name, actions);
            categoriesBody.append(row);
        }
    }

    categoryForm.addEventListener("submit", async (event) => {
        event.preventDefault();
        const body = { name: categoryForm.name.value.trim() };
        const id = categoryForm.categoryId.value;
        const url = id ? "/categories/" + id : "/categories";
        const method = id ? "PUT" : "POST";

        const response = await fetch(url, {
            method,
            headers: authHeaders(),
            body: JSON.stringify(body)
        });

        if (!response.ok) {
            status.textContent = await readApiError(response);
            return;
        }

        categoryForm.reset();
        categoryForm.categoryId.value = "";
        await reload();
    });

    document.getElementById("category-reset").addEventListener("click", () => {
        categoryForm.reset();
        categoryForm.categoryId.value = "";
    });

    try {
        await reload();
    } catch (error) {
        status.textContent = error.message;
    }
}

loadGamesAdmin();
loadCategoriesAdmin();
