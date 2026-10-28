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

async function loadAdmin() {
    if (!requireAdmin()) {
        return;
    }

    const status = document.getElementById("admin-status");
    const gameForm = document.getElementById("game-form");
    const categoryForm = document.getElementById("category-form");
    const gamesBody = document.getElementById("games-body");
    const categoriesBody = document.getElementById("categories-body");
    const categorySelect = gameForm.categoryId;

    async function reload() {
        status.textContent = "";
        const [games, categories] = await Promise.all([fetchGames({}), fetchCategories()]);
        fillCategorySelect(categorySelect, categories, gameForm.categoryId.value);
        renderGameRows(games, categories);
        renderCategoryRows(categories);
    }

    function renderGameRows(games) {
        gamesBody.replaceChildren();

        for (const game of games) {
            const row = document.createElement("tr");

            const picture = document.createElement("td");
            picture.append(createGameImage(game.imageUrl, game.name));

            const name = document.createElement("td");
            name.textContent = game.name;

            const price = document.createElement("td");
            price.textContent = String(game.price);

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

    function renderCategoryRows(categories) {
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

loadAdmin();
