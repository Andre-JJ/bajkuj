const deleteBikeDialog = document.getElementById("inventory-delete-dialog");
if (deleteBikeDialog) {
    const form = document.getElementById("inventory-delete-form");
    const name = deleteBikeDialog.querySelector("[data-delete-bike-name]");
    for (const button of document.querySelectorAll("[data-delete-bike]")) {
        button.addEventListener("click", () => {
            form.action = button.dataset.deleteUrl;
            name.textContent = button.dataset.bikeName;
            deleteBikeDialog.showModal();
        });
    }
    deleteBikeDialog.querySelector("[data-cancel-delete]").addEventListener("click", () => deleteBikeDialog.close());
}
