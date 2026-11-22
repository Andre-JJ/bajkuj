const bikePicker = document.getElementById("landing-bike");
const bikePreview = document.querySelector("[data-bike-preview]");
if (bikePicker && bikePreview) {
    const link = bikePreview.querySelector("[data-bike-photo]");
    const image = link.querySelector("img");
    const placeholder = bikePreview.querySelector("[data-bike-placeholder]");
    const stock = document.querySelector("[data-bike-stock]");
    const title = document.querySelector("[data-bike-model]");
    const monthPrice = document.querySelector("[data-bike-month-price]");
    const yearPrice = document.querySelector("[data-bike-year-price]");
    const modelButtons = document.querySelectorAll("[data-select-bike]");
    function showSelectedBike() {
        const { photo, model, stock: units, monthPrice: monthly, yearPrice: yearly } = bikePicker.selectedOptions[0].dataset;
        link.hidden = !photo;
        placeholder.hidden = Boolean(photo);
        stock.textContent = units;
        title.textContent = model;
        monthPrice.textContent = monthly;
        yearPrice.textContent = yearly;
        for (const button of modelButtons) {
            button.setAttribute("aria-pressed", String(button.dataset.selectBike === bikePicker.value));
        }
        if (photo) {
            link.href = photo;
            image.src = photo;
            image.alt = model;
        }
    }
    bikePicker.addEventListener("change", showSelectedBike);
    for (const button of modelButtons) {
        button.disabled = false;
        button.addEventListener("click", () => {
            bikePicker.value = button.dataset.selectBike;
            showSelectedBike();
        });
    }
    showSelectedBike();
    document.querySelector("[data-bike-picker-fallback]").hidden = true;
    bikePreview.hidden = false;
    document.querySelector("[data-bike-details]").hidden = false;
}

const bikePhotoDialog = document.getElementById("bike-photo-dialog");
if (bikePhotoDialog?.showModal) {
    document.addEventListener("click", (event) => {
        const link = event.target.closest("a[data-bike-photo][href]");
        if (!link || event.defaultPrevented || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) {
            return;
        }
        const image = bikePhotoDialog.querySelector("img");
        image.src = link.href;
        image.alt = link.querySelector("img").alt;
        event.preventDefault();
        bikePhotoDialog.showModal();
    });
}
