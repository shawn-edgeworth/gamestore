function mountHeader(current) {
    const header = document.getElementById("site-header");
    if (!header) {
        return;
    }

    header.style.position = "relative";
    header.style.zIndex = "1000";

    const extraLinks = [];

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
                <li><a href="/" data-nav="home"><strong>Главная</strong></a></li>
                <li><a href="/games.html" data-nav="games">Все игры</a></li>
            </ul>
            <ul>
                <li>
                    <form class="nav-search" action="/search.html" method="get">
                        <fieldset role="group">
                            <input type="search" name="search" placeholder="Поиск по имени" required>
                            <button type="submit">Найти</button>
                        </fieldset>
                    </form>
                </li>
                ${extraLinks.join("")}
                <li id="nav-cart"></li>
                <li id="nav-wallet"></li>
                <li id="nav-account"></li>
            </ul>
        </nav>
    `;

    markCurrentNav(current);
    return renderNav(current);
}
