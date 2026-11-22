const mobileMenu = document.querySelector(".mobile-menu");
const navigation = document.getElementById("prototype-navigation");
const compactNavigation = window.matchMedia("(max-width: 660px), (max-height: 550px)");

function setNavigation(open) {
    navigation.hidden = !open;
    mobileMenu.setAttribute("aria-expanded", String(open));
}

function updateNavigation() {
    mobileMenu.hidden = !compactNavigation.matches;
    setNavigation(!compactNavigation.matches);
}

mobileMenu.addEventListener("click", () => setNavigation(navigation.hidden));
compactNavigation.addEventListener("change", updateNavigation);
updateNavigation();

const notificationMenu = document.querySelector("[data-notification-menu]");
const preferencesForm = document.getElementById("notification-preferences");
const preferenceKey = "bajkuj-notifications-" + notificationMenu.dataset.role;
const preferences = { renewal: true, ended: true, reminderDays: 7 };
const reminderDayOptions = [1, 3, 7, 14];

try {
    const saved = JSON.parse(localStorage.getItem(preferenceKey));
    for (const category of ["renewal", "ended"]) {
        if (typeof saved?.[category] === "boolean") {
            preferences[category] = saved[category];
        }
    }
    if (reminderDayOptions.includes(saved?.reminderDays)) {
        preferences.reminderDays = saved.reminderDays;
    }
} catch {
    // Use the defaults when local storage is unavailable.
}

function updateNotifications() {
    const items = [...notificationMenu.querySelectorAll("[data-notification]")];
    items.forEach((item) => {
        const outsideReminderWindow = item.dataset.notification === "renewal"
            && Number(item.dataset.daysUntilEnd) > preferences.reminderDays;
        item.hidden = !preferences[item.dataset.notification] || outsideReminderWindow;
    });
    const hasNotifications = items.some((item) => !item.hidden);
    notificationMenu.querySelector(".notification-dot").hidden = !hasNotifications;
    notificationMenu.querySelector("[data-notification-empty]").hidden = hasNotifications;
}

if (preferencesForm) {
    const reminderDays = document.getElementById("reminder-days");
    reminderDays.value = String(preferences.reminderDays);
    const options = [...preferencesForm.querySelectorAll("input[type=checkbox]")];
    options.forEach((option) => {
        option.checked = preferences[option.name];
    });
    preferencesForm.addEventListener("submit", (event) => {
        event.preventDefault();
        options.forEach((option) => {
            preferences[option.name] = option.checked;
        });
        preferences.reminderDays = Number(reminderDays.value);
        const status = document.querySelector("[data-preferences-status]");
        try {
            localStorage.setItem(preferenceKey, JSON.stringify(preferences));
            status.textContent = status.dataset.saved;
        } catch {
            status.textContent = status.dataset.unsaved;
        }
        updateNotifications();
    });
}
updateNotifications();

const profileKey = "bajkuj-profile-" + notificationMenu.dataset.role;
const profileForm = document.getElementById("profile-preferences");
const profileName = document.getElementById("profile-name");
const profileEmail = document.getElementById("profile-email");

function showProfileName(name) {
    document.querySelectorAll("[data-profile-name]").forEach((element) => {
        element.textContent = name;
    });
}

try {
    const saved = JSON.parse(localStorage.getItem(profileKey));
    if (typeof saved?.displayName === "string" && saved.displayName.trim() && saved.displayName.length <= 80) {
        showProfileName(saved.displayName);
        if (profileForm) {
            profileName.value = saved.displayName;
        }
    }
    if (profileForm && typeof saved?.contactEmail === "string" && saved.contactEmail.length <= 254) {
        profileEmail.value = saved.contactEmail;
    }
} catch {
    // Keep the preview profile when local storage is unavailable.
}

if (profileForm) {
    profileForm.addEventListener("submit", (event) => {
        event.preventDefault();
        profileName.value = profileName.value.trim();
        if (!profileForm.reportValidity()) {
            return;
        }
        const profile = { displayName: profileName.value, contactEmail: profileEmail.value.trim() };
        showProfileName(profile.displayName);
        const status = document.querySelector("[data-profile-status]");
        try {
            localStorage.setItem(profileKey, JSON.stringify(profile));
            status.textContent = status.dataset.saved;
        } catch {
            status.textContent = status.dataset.unsaved;
        }
    });
}

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
    const confirmation = document.getElementById("feedback-confirmation");
    feedbackForm.addEventListener("submit", (event) => {
        event.preventDefault();

        const comment = document.getElementById("feedback-comment");
        if (!comment.value.trim()) {
            comment.focus();
            confirmation.textContent = confirmation.dataset.required;
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
        confirmation.textContent = confirmation.dataset.saved;
        feedbackForm.reset();
    });
}

const rentalForm = document.querySelector("[data-rental-form]");
if (rentalForm) {
    const term = document.getElementById("rental-term");
    const returnLabel = rentalForm.querySelector("[data-rental-return]");
    const totalLabel = rentalForm.querySelector("[data-rental-total]");
    term.addEventListener("change", () => {
        const plan = term.selectedOptions[0];
        returnLabel.textContent = plan.dataset.returnDate;
        totalLabel.textContent = plan.dataset.price;
    });
}
