function mountHeader(current) {
    const header = document.getElementById("site-header");
    if (!header) {
        return;
    }

    const extraLinks = [];

    if (isLoggedIn() && !isAdmin()) {
        extraLinks.push('<li><a href="/orders.html" data-nav="orders">Заказы</a></li>');
    }

    if (isAdmin()) {
        extraLinks.push(`
            <li>
                <details class="dropdown">
                    <summary>Редактировать</summary>
                    <ul>
                        <li><a href="/admin-games.html" data-nav="admin-games">Игры</a></li>
                        <li><a href="/admin-categories.html" data-nav="admin-categories">Категории</a></li>
                    </ul>
                </details>
            </li>
        `);
    }

    header.innerHTML = `
        <nav>
            <ul>
                <li><a href="/" data-nav="home"><strong>GameStore</strong></a></li>
            </ul>
            <ul>
                <li><a href="/games.html" data-nav="games">Все игры</a></li>
                <li>
                    <form action="/search.html" method="get">
                        <fieldset role="group">
                            <input type="search" name="search" placeholder="Поиск по имени" required>
                            <button type="submit">Найти</button>
                        </fieldset>
                    </form>
                </li>
                ${extraLinks.join("")}
                <li id="nav-auth"></li>
            </ul>
        </nav>
    `;

    markCurrentNav(current);
    return renderNav(current);
}
