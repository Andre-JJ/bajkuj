async function loadCardCheckout(form) {
    const submit = form.querySelector("button");
    const message = form.querySelector("[data-payment-message]");
    const options = form.dataset;
    try {
        const stripe = Stripe(options.publishableKey, { locale: document.documentElement.lang });
        const checkout = stripe.initCheckoutElementsSdk({
            clientSecret: options.clientSecret,
            defaultValues: { email: options.email }
        });
        const payment = checkout.createPaymentElement({ layout: "tabs" });
        payment.on("ready", () => {
            message.textContent = "";
            submit.disabled = false;
        });
        payment.on("loaderror", () => {
            submit.disabled = true;
            message.textContent = options.error;
        });
        const result = await checkout.loadActions();
        if (result.type !== "success") {
            message.textContent = result.error.message;
            return;
        }
        form.addEventListener("submit", async (event) => {
            event.preventDefault();
            submit.disabled = true;
            message.textContent = "";
            try {
                const confirmation = await result.actions.confirm({ redirect: "if_required" });
                if (confirmation.type === "success") {
                    window.location.assign(options.resultUrl);
                    return;
                }
                message.textContent = confirmation.error.message;
            } catch {
                message.textContent = options.error;
            }
            submit.disabled = false;
        });
        payment.mount(form.querySelector("[data-payment-element]"));
    } catch {
        message.textContent = options.error;
    }
}

const cardCheckout = document.querySelector("[data-card-checkout]");
if (cardCheckout) {
    loadCardCheckout(cardCheckout);
}
