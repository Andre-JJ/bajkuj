const mobileMenu = document.querySelector(".mobile-menu");
const navigation = document.getElementById("prototype-navigation");
const mobileLayout = window.matchMedia("(max-width: 660px)");

function setNavigation(open) {
    navigation.hidden = !open;
    mobileMenu.setAttribute("aria-expanded", String(open));
}

function updateNavigation() {
    mobileMenu.hidden = !mobileLayout.matches;
    setNavigation(!mobileLayout.matches);
}

mobileMenu.addEventListener("click", () => setNavigation(navigation.hidden));
mobileLayout.addEventListener("change", updateNavigation);
updateNavigation();

const notificationMenu = document.querySelector("[data-notification-menu]");
const preferencesForm = document.getElementById("notification-preferences");
const preferenceKey = "bajkuj-notifications-" + notificationMenu.dataset.role;
const preferences = { renewal: true, ended: true };

try {
    const saved = JSON.parse(localStorage.getItem(preferenceKey));
    for (const category of Object.keys(preferences)) {
        if (typeof saved?.[category] === "boolean") {
            preferences[category] = saved[category];
        }
    }
} catch {
    // Use the defaults when local storage is unavailable.
}

function updateNotifications() {
    const items = [...notificationMenu.querySelectorAll("[data-notification]")];
    items.forEach((item) => {
        item.hidden = !preferences[item.dataset.notification];
    });
    const hasNotifications = items.some((item) => !item.hidden);
    notificationMenu.querySelector(".notification-dot").hidden = !hasNotifications;
    notificationMenu.querySelector("[data-notification-empty]").hidden = hasNotifications;
}

if (preferencesForm) {
    const options = [...preferencesForm.querySelectorAll("input[type=checkbox]")];
    options.forEach((option) => {
        option.checked = preferences[option.name];
    });
    preferencesForm.addEventListener("submit", (event) => {
        event.preventDefault();
        options.forEach((option) => {
            preferences[option.name] = option.checked;
        });
        const status = document.querySelector("[data-preferences-status]");
        try {
            localStorage.setItem(preferenceKey, JSON.stringify(preferences));
            status.textContent = "Predvoľby boli uložené.";
        } catch {
            status.textContent = "Predvoľby platia na tejto stránke. Prehliadač ich nemohol uložiť.";
        }
        updateNotifications();
    });
}
updateNotifications();

document.addEventListener("click", (event) => {
    if (!notificationMenu.contains(event.target)) {
        notificationMenu.open = false;
    }
});
notificationMenu.addEventListener("keydown", (event) => {
    if (event.key === "Escape" && notificationMenu.open) {
        notificationMenu.open = false;
        notificationMenu.querySelector("summary").focus();
    }
});

function openLinkedAgreement() {
    const agreement = document.getElementById(window.location.hash.slice(1));
    if (agreement instanceof HTMLDetailsElement) {
        agreement.open = true;
        agreement.scrollIntoView({ block: "nearest" });
    }
}
window.addEventListener("hashchange", openLinkedAgreement);
openLinkedAgreement();

document.querySelectorAll("select[data-filter]").forEach((select) => {
    const container = document.getElementById(select.dataset.filter);
    const empty = select.dataset.empty && document.getElementById(select.dataset.empty);
    const rows = [...container.querySelectorAll("[data-filter-value]")];

    function updateFilter() {
        rows.forEach((row) => {
            row.hidden = Boolean(select.value) && row.dataset.filterValue !== select.value;
        });

        if (empty) {
            empty.hidden = rows.some((row) => !row.hidden);
        }
    }

    select.addEventListener("change", updateFilter);
    updateFilter();
});

const feedbackForm = document.getElementById("feedback-form");

if (feedbackForm) {
    feedbackForm.addEventListener("submit", (event) => {
        event.preventDefault();

        const comment = document.getElementById("feedback-comment");
        if (!comment.value.trim()) {
            comment.focus();
            document.getElementById("feedback-confirmation").textContent = "Zadajte komentár.";
            return;
        }

        const topic = document.getElementById("feedback-topic").value;
        const rating = document.getElementById("feedback-rating").value.split(" ")[0];
        const item = document.createElement("li");
        const heading = document.createElement("strong");
        const body = document.createElement("p");

        heading.textContent = topic + " · " + rating + " / 5";
        body.textContent = comment.value.trim();
        item.append(heading, body);
        document.getElementById("feedback-history").prepend(item);
        document.getElementById("feedback-confirmation").textContent = "Hodnotenie bolo pridané do náhľadu.";
        feedbackForm.reset();
    });
}
