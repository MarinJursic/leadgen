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

  const debounce = (callback, delay = 220) => {
    let handle;
    return (...args) => {
      window.clearTimeout(handle);
      handle = window.setTimeout(() => callback(...args), delay);
    };
  };

  const initBlurValidation = () => {
    if (!window.jQuery || !window.jQuery.validator) {
      return;
    }

    window.jQuery("form").each(function () {
      window.jQuery.validator.unobtrusive.parse(this);
    });

    document.addEventListener("blur", (event) => {
      const field = event.target;
      if (!(field instanceof HTMLInputElement || field instanceof HTMLTextAreaElement || field instanceof HTMLSelectElement)) {
        return;
      }

      const form = field.closest("form");
      if (!form || !window.jQuery(form).data("validator")) {
        return;
      }

      window.jQuery(field).valid();
    }, true);
  };

  const initAjaxSearch = () => {
    document.querySelectorAll("[data-ajax-search]").forEach((form) => {
      const input = form.querySelector("[data-search-input]");
      const status = form.querySelector("[data-search-status]");
      const entity = form.dataset.searchEntity;
      const panel = form.closest(".surface-panel") || document;
      const tableBody = panel.querySelector("[data-search-results]");
      const cardGrid = document.querySelector("[data-card-results]");

      if (!input || !entity) {
        return;
      }

      const renderActions = (actions) => {
        const cell = document.createElement("td");
        cell.className = "table-actions";
        actions.forEach((action) => {
          const link = document.createElement("a");
          link.href = action.url;
          link.textContent = action.label;
          link.className = `button-link${action.style === "danger" ? " button-link--danger" : ""}`;
          cell.appendChild(link);
        });
        return cell;
      };

      const renderCell = (cellData) => {
        const cell = document.createElement("td");
        const strong = document.createElement("strong");
        strong.textContent = cellData.primary || "";
        cell.appendChild(strong);

        if (cellData.secondary) {
          cell.appendChild(document.createElement("br"));
          const secondary = document.createElement("span");
          secondary.textContent = cellData.secondary;
          cell.appendChild(secondary);
        }

        return cell;
      };

      const renderRows = (result) => {
        if (!tableBody) {
          return;
        }

        const columnCount = tableBody.closest("table")?.querySelectorAll("thead th").length || 1;
        tableBody.innerHTML = "";

        if (!result.rows || result.rows.length === 0) {
          const row = document.createElement("tr");
          const cell = document.createElement("td");
          cell.colSpan = columnCount;
          cell.className = "empty-state";
          cell.textContent = "No matching records.";
          row.appendChild(cell);
          tableBody.appendChild(row);
          return;
        }

        result.rows.forEach((rowData) => {
          const row = document.createElement("tr");
          row.className = "search-row-enter";
          rowData.cells.forEach((cellData) => row.appendChild(renderCell(cellData)));
          row.appendChild(renderActions(rowData.actions || []));
          tableBody.appendChild(row);
        });
      };

      const renderCards = (result) => {
        if (!cardGrid || entity !== "queue") {
          return;
        }

        cardGrid.innerHTML = "";
        if (!result.cards || result.cards.length === 0) {
          const empty = document.createElement("div");
          empty.className = "empty-state surface-panel";
          empty.textContent = "No matching outreach items.";
          cardGrid.appendChild(empty);
          return;
        }

        result.cards.forEach((card) => {
          const article = document.createElement("article");
          article.className = "detail-panel queue-card search-row-enter";
          article.innerHTML = `
            <div class="lead-card__meta">
              <span class="score-pill score-pill--high"></span>
              <span class="mission-chip"></span>
            </div>
            <h2 class="detail-panel__title"></h2>
            <p class="detail-panel__summary"></p>
            <p class="detail-panel__summary"></p>
            <div class="queue-card__footer">
              <span class="signal-item__meta"></span>
              <a class="button-link">Open dossier</a>
            </div>`;
          article.querySelector(".score-pill").textContent = card.score;
          article.querySelector(".mission-chip").textContent = card.subtitle;
          article.querySelector(".detail-panel__title").textContent = card.title;
          article.querySelectorAll(".detail-panel__summary")[0].textContent = card.subtitle;
          article.querySelectorAll(".detail-panel__summary")[1].textContent = card.summary;
          article.querySelector(".signal-item__meta").textContent = card.meta;
          article.querySelector(".button-link").href = card.detailUrl;
          cardGrid.appendChild(article);
        });
      };

      const executeSearch = debounce(async () => {
        const url = `/search/${encodeURIComponent(entity)}?q=${encodeURIComponent(input.value)}`;
        const target = tableBody || (entity === "queue" ? cardGrid : null);
        target?.classList.add("is-refreshing");
        status.textContent = "Searching...";

        try {
          const response = await fetch(url, { headers: { "Accept": "application/json" } });
          const result = await response.json();
          renderRows(result);
          renderCards(result);
          status.textContent = `${result.totalCount} matching record${result.totalCount === 1 ? "" : "s"}`;
        } catch {
          status.textContent = "Search failed";
        } finally {
          target?.classList.remove("is-refreshing");
        }
      });

      input.addEventListener("input", executeSearch);
    });
  };

  const initAutocomplete = () => {
    document.querySelectorAll("[data-autocomplete]").forEach((field) => {
      const input = field.querySelector("[data-autocomplete-input]");
      const hidden = field.querySelector("[data-autocomplete-value]");
      const menu = field.querySelector("[data-autocomplete-menu]");
      const message = field.querySelector("[data-valmsg-for]");
      const url = field.dataset.autocompleteUrl;

      if (!input || !hidden || !menu || !url) {
        return;
      }

      const setOpen = (open) => {
        menu.classList.toggle("is-open", open);
      };

      const render = (items) => {
        menu.innerHTML = "";
        if (items.length === 0) {
          const empty = document.createElement("div");
          empty.className = "autocomplete-menu__empty";
          empty.textContent = "No results";
          menu.appendChild(empty);
          setOpen(true);
          return;
        }

        items.forEach((item) => {
          const button = document.createElement("button");
          button.type = "button";
          button.className = "autocomplete-menu__item";
          button.innerHTML = "<strong></strong><span></span>";
          button.querySelector("strong").textContent = item.text;
          button.querySelector("span").textContent = item.description || "";
          button.addEventListener("click", () => {
            input.value = item.text;
            hidden.value = item.id;
            input.classList.remove("input-validation-error");
            if (message) {
              message.textContent = "";
            }
            hidden.dispatchEvent(new Event("change", { bubbles: true }));
            setOpen(false);
            field.classList.add("is-selected");
          });
          menu.appendChild(button);
        });
        setOpen(true);
      };

      const search = debounce(async () => {
        const query = input.value.trim();
        if (query.length < 1) {
          hidden.value = "";
          setOpen(false);
          return;
        }

        const response = await fetch(`${url}?q=${encodeURIComponent(query)}`, {
          headers: { "Accept": "application/json" }
        });
        render(await response.json());
      }, 180);

      input.addEventListener("input", () => {
        hidden.value = "";
        field.classList.remove("is-selected");
        search();
      });

      input.addEventListener("focus", () => {
        if (input.value.trim()) {
          search();
        }
      });

      document.addEventListener("click", (event) => {
        if (!field.contains(event.target)) {
          setOpen(false);
        }
      });

      input.addEventListener("blur", () => {
        window.setTimeout(() => {
          if (field.contains(document.activeElement)) {
            return;
          }
          if (input.required && !hidden.value) {
            input.classList.add("input-validation-error");
            if (message) {
              message.textContent = "Select a result from the list.";
            }
          }
        }, 120);
      });
    });
  };

  const initDateControls = () => {
    const locale = navigator.language?.toLowerCase().startsWith("hr") ? "hr" : "en";
    const monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    const pad = (value) => String(value).padStart(2, "0");
    const toIso = (date) => `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}:00`;
    const formatDisplay = (date, includeTime) => {
      const datePart = locale === "hr"
        ? `${pad(date.getDate())}.${pad(date.getMonth() + 1)}.${date.getFullYear()}`
        : `${pad(date.getMonth() + 1)}/${pad(date.getDate())}/${date.getFullYear()}`;
      return includeTime ? `${datePart} ${pad(date.getHours())}:${pad(date.getMinutes())}` : datePart;
    };

    const parseDisplay = (value) => {
      const trimmed = value.trim();
      if (!trimmed) {
        return null;
      }

      const iso = trimmed.match(/^(\d{4})-(\d{2})-(\d{2})(?:[T\s](\d{2}):(\d{2}))?/);
      if (iso) {
        return new Date(Number(iso[1]), Number(iso[2]) - 1, Number(iso[3]), Number(iso[4] || 0), Number(iso[5] || 0));
      }

      const hr = trimmed.match(/^(\d{1,2})\.(\d{1,2})\.(\d{4})(?:\s+(\d{1,2}):(\d{2}))?/);
      if (hr) {
        return new Date(Number(hr[3]), Number(hr[2]) - 1, Number(hr[1]), Number(hr[4] || 0), Number(hr[5] || 0));
      }

      const en = trimmed.match(/^(\d{1,2})\/(\d{1,2})\/(\d{4})(?:\s+(\d{1,2}):(\d{2}))?/);
      if (en) {
        return new Date(Number(en[3]), Number(en[1]) - 1, Number(en[2]), Number(en[4] || 0), Number(en[5] || 0));
      }

      return null;
    };

    document.querySelectorAll("[data-date-control]").forEach((control) => {
      const display = control.querySelector("[data-date-display]");
      const hidden = control.querySelector("[data-date-value]");
      const picker = control.querySelector("[data-date-picker]");
      const title = control.querySelector("[data-date-title]");
      const grid = control.querySelector("[data-date-grid]");
      const message = control.querySelector("[data-valmsg-for]");
      const hour = control.querySelector("[data-date-hour]");
      const minute = control.querySelector("[data-date-minute]");
      const includeTime = control.dataset.includeTime === "true";

      const hasInitialValue = Boolean(hidden.value);
      let selected = parseDisplay(hidden.value) || new Date();
      let viewing = new Date(selected.getFullYear(), selected.getMonth(), 1);

      const updateFields = () => {
        hidden.value = toIso(selected);
        display.value = formatDisplay(selected, includeTime);
        if (hour) {
          hour.value = String(selected.getHours());
        }
        if (minute) {
          minute.value = pad(selected.getMinutes());
        }
        hidden.dispatchEvent(new Event("change", { bubbles: true }));
      };

      const renderCalendar = () => {
        title.textContent = `${monthNames[viewing.getMonth()]} ${viewing.getFullYear()}`;
        grid.innerHTML = "";
        const firstDay = new Date(viewing.getFullYear(), viewing.getMonth(), 1);
        const offset = (firstDay.getDay() + 6) % 7;
        const daysInMonth = new Date(viewing.getFullYear(), viewing.getMonth() + 1, 0).getDate();

        for (let i = 0; i < offset; i += 1) {
          grid.appendChild(document.createElement("span"));
        }

        for (let day = 1; day <= daysInMonth; day += 1) {
          const button = document.createElement("button");
          button.type = "button";
          button.textContent = String(day);
          button.className = "date-picker__day";
          if (selected.getFullYear() === viewing.getFullYear()
            && selected.getMonth() === viewing.getMonth()
            && selected.getDate() === day) {
            button.classList.add("is-selected");
          }
          button.addEventListener("click", () => {
            selected = new Date(viewing.getFullYear(), viewing.getMonth(), day, selected.getHours(), selected.getMinutes());
            renderCalendar();
            updateFields();
          });
          grid.appendChild(button);
        }
      };

      control.querySelector("[data-date-prev]")?.addEventListener("click", () => {
        viewing = new Date(viewing.getFullYear(), viewing.getMonth() - 1, 1);
        renderCalendar();
      });

      control.querySelector("[data-date-next]")?.addEventListener("click", () => {
        viewing = new Date(viewing.getFullYear(), viewing.getMonth() + 1, 1);
        renderCalendar();
      });

      control.querySelector("[data-date-today]")?.addEventListener("click", () => {
        selected = new Date();
        viewing = new Date(selected.getFullYear(), selected.getMonth(), 1);
        renderCalendar();
        updateFields();
      });

      control.querySelector("[data-date-apply]")?.addEventListener("click", () => {
        if (hour) {
          selected.setHours(Math.max(0, Math.min(23, Number(hour.value || 0))));
        }
        if (minute) {
          selected.setMinutes(Math.max(0, Math.min(59, Number(minute.value || 0))));
        }
        updateFields();
        picker.classList.remove("is-open");
      });

      display.addEventListener("focus", () => {
        picker.classList.add("is-open");
      });

      display.addEventListener("blur", () => {
        window.setTimeout(() => {
          if (control.contains(document.activeElement)) {
            return;
          }
          const parsed = parseDisplay(display.value);
          if (parsed) {
            selected = parsed;
            viewing = new Date(selected.getFullYear(), selected.getMonth(), 1);
            control.classList.remove("has-date-error");
            display.classList.remove("input-validation-error");
            if (message) {
              message.textContent = "";
            }
            renderCalendar();
            updateFields();
          } else if (display.value.trim()) {
            control.classList.add("has-date-error");
            display.classList.add("input-validation-error");
            if (message) {
              message.textContent = "Enter a valid date and time.";
            }
          } else {
            hidden.value = "";
            if (display.required) {
              display.classList.add("input-validation-error");
              if (message) {
                message.textContent = "Date and time are required.";
              }
            }
          }
          picker.classList.remove("is-open");
        }, 130);
      });

      if (hasInitialValue) {
        updateFields();
      } else {
        display.value = "";
        hidden.value = "";
        if (hour) {
          hour.value = String(selected.getHours());
        }
        if (minute) {
          minute.value = pad(selected.getMinutes());
        }
      }
      renderCalendar();
    });
  };

  initVaultSheets();
  initMissionCanvas();
  initBlurValidation();
  initAjaxSearch();
  initAutocomplete();
  initDateControls();
})();
