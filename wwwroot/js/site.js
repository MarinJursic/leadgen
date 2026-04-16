(() => {
  const initVaultSheets = () => {
    const openVault = (id) => {
      const panel = document.getElementById(id);
      const overlay = document.querySelector(`[data-vault-close="${id}"].vault-overlay`);
      if (!panel || !overlay) {
        return;
      }

      panel.classList.add("is-open");
      overlay.classList.add("is-open");
      panel.setAttribute("aria-hidden", "false");
    };

    const closeVault = (id) => {
      const panel = document.getElementById(id);
      const overlay = document.querySelector(`[data-vault-close="${id}"].vault-overlay`);
      if (!panel || !overlay) {
        return;
      }

      panel.classList.remove("is-open");
      overlay.classList.remove("is-open");
      panel.setAttribute("aria-hidden", "true");
    };

    document.querySelectorAll("[data-vault-open]").forEach((button) => {
      button.addEventListener("click", () => openVault(button.dataset.vaultOpen));
    });

    document.querySelectorAll("[data-vault-close]").forEach((button) => {
      button.addEventListener("click", () => closeVault(button.dataset.vaultClose));
    });

    document.addEventListener("keydown", (event) => {
      if (event.key !== "Escape") {
        return;
      }

      document.querySelectorAll(".vault-sheet.is-open").forEach((panel) => {
        closeVault(panel.id);
      });
    });
  };

  const initMissionCanvas = () => {
    const canvasPage = document.querySelector("[data-mission-canvas]");
    if (!canvasPage) {
      return;
    }

    const progressValue = canvasPage.querySelector("[data-progress-value]");
    const progressBar = canvasPage.querySelector("[data-progress-bar]");
    const progressPhase = canvasPage.querySelector("[data-progress-phase]");
    const viewport = canvasPage.querySelector("[data-mission-viewport]");
    const nodes = new Map(
      Array.from(canvasPage.querySelectorAll("[data-node-role]")).map((node) => [
        node.dataset.nodeRole,
        node
      ])
    );

    const stages = [
      { role: "Strategist", start: 0, complete: 18, phase: "Mission routing" },
      { role: "Scout", start: 12, complete: 38, phase: "Deep research" },
      { role: "Sentinel", start: 22, complete: 52, phase: "Signal verification" },
      { role: "Anchor", start: 40, complete: 74, phase: "Identity resolution" },
      { role: "Soul", start: 56, complete: 86, phase: "Intent mining" },
      { role: "Stitcher", start: 72, complete: 96, phase: "Archive stitching" },
      { role: "Sniper", start: 84, complete: 100, phase: "Final verification" }
    ];

    let progress = Number(canvasPage.dataset.startingProgress || "14");
    let scale = 1;

    const setPhase = (value) => {
      let activePhase = "Archive ready";
      for (const stage of stages) {
        if (value < stage.complete) {
          activePhase = stage.phase;
          break;
        }
      }
      progressPhase.textContent = activePhase;
    };

    const updateNodes = (value) => {
      nodes.forEach((node) => {
        node.classList.remove("is-searching", "is-complete");
      });

      stages.forEach((stage, index) => {
        const node = nodes.get(stage.role);
        if (!node) {
          return;
        }

        const badge = node.querySelector(".mission-node__badge");
        if (badge) {
          badge.textContent = stage.role === "Strategist" ? "1" : String(index + 1);
        }

        if (value >= stage.complete) {
          node.classList.add("is-complete");
        } else if (value >= stage.start) {
          node.classList.add("is-searching");
        }
      });
    };

    const updateProgress = (value) => {
      const rounded = Math.max(0, Math.min(100, Math.floor(value)));
      progressValue.textContent = `${rounded}%`;
      progressBar.style.width = `${rounded}%`;
      setPhase(rounded);
      updateNodes(rounded);
    };

    const applyZoom = () => {
      viewport.style.setProperty("--mission-scale", scale.toFixed(2));
    };

    canvasPage.querySelectorAll("[data-zoom]").forEach((button) => {
      button.addEventListener("click", () => {
        const action = button.dataset.zoom;

        if (action === "in") {
          scale = Math.min(1.45, scale + 0.12);
        } else if (action === "out") {
          scale = Math.max(0.8, scale - 0.12);
        } else {
          scale = 1;
        }

        applyZoom();
      });
    });

    updateProgress(progress);
    applyZoom();

    const tick = () => {
      progress = Math.min(100, progress + 0.45);
      updateProgress(progress);

      if (progress < 100) {
        window.setTimeout(tick, 70);
      }
    };

    window.setTimeout(tick, 320);
  };

  initVaultSheets();
  initMissionCanvas();
})();
