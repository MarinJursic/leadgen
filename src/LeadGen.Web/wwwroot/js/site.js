if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", initPageChrome);
} else {
  initPageChrome();
}

function initPageChrome() {
  initSubmitProgress();
  initToasts();
}

function initSubmitProgress() {
  document.querySelectorAll("form[data-submit-progress]").forEach((form) => {
    if (form.dataset.submitProgressReady === "true") {
      return;
    }

    form.dataset.submitProgressReady = "true";

    form.addEventListener("submit", (event) => {
      if (form.dataset.submitProgressRelease === "true") {
        form.dataset.submitProgressRelease = "false";
        return;
      }

      if (form.dataset.submitting === "true") {
        event.preventDefault();
        return;
      }

      if (!formIsValid(form)) {
        event.preventDefault();
        return;
      }

      form.dataset.submitting = "true";
      form.setAttribute("aria-busy", "true");

      const button = form.querySelector("[data-submit-button]");
      if (button instanceof HTMLButtonElement) {
        button.disabled = true;
        toggleLoadingLabel(button, true);
      }

      const targetSelector = form.getAttribute("data-submit-progress-target");
      const progress = targetSelector ? document.querySelector(targetSelector) : null;
      if (progress) {
        progress.classList.remove("d-none");
      }

      const submitWasAlreadyHandled = event.defaultPrevented;
      const minVisibleMs = getSubmitProgressMinMs(form);
      if (!submitWasAlreadyHandled && minVisibleMs > 0) {
        event.preventDefault();
        submitFormWithProgress(form, minVisibleMs);
      }
    });
  });
}

async function submitFormWithProgress(form, minVisibleMs) {
  const minimumDelay = delay(minVisibleMs);

  try {
    const response = await fetch(form.action, {
      body: new FormData(form),
      credentials: "same-origin",
      headers: {
        Accept: "application/json",
        "X-Requested-With": "XMLHttpRequest"
      },
      method: form.method || "post"
    });

    await minimumDelay;
    await handleProgressSubmitResponse(form, response);
  } catch {
    await minimumDelay;
    form.dataset.submitProgressRelease = "true";
    HTMLFormElement.prototype.submit.call(form);
  }
}

async function handleProgressSubmitResponse(form, response) {
  const contentType = response.headers.get("content-type") ?? "";
  if (contentType.includes("application/json")) {
    const payload = await response.json();
    if (payload.redirectUrl) {
      window.location.assign(payload.redirectUrl);
      return;
    }
  }

  if (response.redirected && response.url) {
    window.location.assign(response.url);
    return;
  }

  const html = await response.text();
  document.open();
  document.write(html);
  document.close();
}

function delay(ms) {
  return new Promise((resolve) => window.setTimeout(resolve, ms));
}

function getSubmitProgressMinMs(form) {
  const rawValue = form.getAttribute("data-submit-progress-min-ms");
  const parsed = Number.parseInt(rawValue ?? "0", 10);
  if (Number.isNaN(parsed)) {
    return 0;
  }

  return Math.max(0, Math.min(parsed, 2000));
}

function formIsValid(form) {
  if (window.jQuery) {
    const $form = window.jQuery(form);
    if ($form.data("validator") && !$form.valid()) {
      return false;
    }
  }

  return typeof form.checkValidity !== "function" || form.checkValidity();
}

function toggleLoadingLabel(button, isLoading) {
  const ready = button.querySelector("[data-submit-ready]");
  const loading = button.querySelector("[data-submit-loading]");
  ready?.classList.toggle("d-none", isLoading);
  loading?.classList.toggle("d-none", !isLoading);
  loading?.classList.toggle("d-inline-flex", isLoading);
}

function initToasts() {
  document.querySelectorAll(".toast").forEach((toastEl) => {
    if (window.bootstrap?.Toast) {
      window.bootstrap.Toast.getOrCreateInstance(toastEl).show();
      return;
    }

    toastEl.classList.add("show");
  });
}
