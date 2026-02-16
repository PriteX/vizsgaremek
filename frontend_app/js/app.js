const el = (id) => document.getElementById(id);

const state = {
  services: [],
  employees: []
};

function setStatus(id, msg) {
  const target = el(id);
  if (!target) {
    return;
  }
  target.textContent = msg || "";
}

function getToken() {
  return localStorage.getItem("token");
}

function decodeJwtPayload(token) {
  if (!token || token.split(".").length < 2) {
    return null;
  }

  try {
    const base64Url = token.split(".")[1];
    const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split("")
        .map((c) => `%${`00${c.charCodeAt(0).toString(16)}`.slice(-2)}`)
        .join("")
    );

    return JSON.parse(jsonPayload);
  } catch {
    return null;
  }
}

function normalizeRole(value) {
  return String(value || "").trim().toUpperCase();
}

function getEffectiveRole() {
  const savedRole = normalizeRole(localStorage.getItem("role"));
  if (savedRole) {
    return savedRole;
  }

  const payload = decodeJwtPayload(getToken());
  if (!payload) {
    return "";
  }

  const claimKeys = [
    "role",
    "Role",
    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
  ];

  for (const key of claimKeys) {
    const claimValue = payload[key];
    if (!claimValue) {
      continue;
    }

    if (Array.isArray(claimValue)) {
      const roleFromArray = normalizeRole(claimValue[0]);
      if (roleFromArray) {
        localStorage.setItem("role", roleFromArray);
        return roleFromArray;
      }
    }

    const roleFromSingle = normalizeRole(claimValue);
    if (roleFromSingle) {
      localStorage.setItem("role", roleFromSingle);
      return roleFromSingle;
    }
  }

  return "";
}

function isAdmin() {
  return getEffectiveRole() === "ADMIN";
}

function statusLabel(status) {
  if (typeof status === "string") {
    return status;
  }

  return Number(status) === 1 ? "Booked" : "Cancelled";
}

function updateAdminButtonVisibility() {
  const openAdminBtn = el("openAdminBtn");
  if (!openAdminBtn) {
    return;
  }

  openAdminBtn.classList.toggle("hidden", !isAdmin());
}

function findServiceName(serviceId) {
  return state.services.find((s) => s.id === Number(serviceId))?.name || `Szolgáltatás #${serviceId}`;
}

function findEmployeeName(employeeId) {
  return state.employees.find((e) => e.id === Number(employeeId))?.name || `Dolgozó #${employeeId}`;
}

function resetAdminPanel() {
  setStatus("adminStatus", "");

  const adminAppointments = el("adminAppointments");
  if (adminAppointments) {
    adminAppointments.innerHTML = "";
  }
}

function logout() {
  localStorage.removeItem("token");
  localStorage.removeItem("role");

  setStatus("authStatus", "Kijelentkezve.");

  const myAppointments = el("myAppointments");
  if (myAppointments) {
    myAppointments.innerHTML = "<p class='muted'>A foglalásokhoz jelentkezz be.</p>";
  }

  const slots = el("slots");
  if (slots) {
    slots.innerHTML = "";
  }

  setStatus("bookingStatus", "");
  updateAdminButtonVisibility();
  closeAdminModal();
  resetAdminPanel();
}

async function loadLookups() {
  const baseUrl = window.APP_CONFIG.baseUrl;
  setStatus("baseUrlLabel", baseUrl);

  state.services = await apiFetch("/api/services");
  state.employees = await apiFetch("/api/employees");

  el("serviceSelect").innerHTML = state.services
    .filter((s) => s.isActive)
    .map((s) => `<option value="${s.id}">${s.name} (${s.durationMinutes} perc)</option>`)
    .join("");

  el("employeeSelect").innerHTML = state.employees
    .filter((e) => e.isActive)
    .map((e) => `<option value="${e.id}">${e.name}</option>`)
    .join("");

  const tomorrow = new Date(Date.now() + 24 * 60 * 60 * 1000);
  const today = new Date();
  el("dateInput").value = tomorrow.toISOString().slice(0, 10);

  const adminDateFrom = el("adminDateFrom");
  const adminDateTo = el("adminDateTo");

  if (adminDateFrom && adminDateTo) {
    adminDateFrom.value = today.toISOString().slice(0, 10);
    adminDateTo.value = tomorrow.toISOString().slice(0, 10);
  }
}

async function onLogin(e) {
  e.preventDefault();
  setStatus("authStatus", "");

  el("myAppointments").innerHTML = "";
  el("slots").innerHTML = "";
  setStatus("bookingStatus", "");

  try {
    const email = el("loginEmail").value;
    const password = el("loginPassword").value;

    const res = await apiFetch("/api/auth/login", {
      method: "POST",
      body: JSON.stringify({ email, password })
    });

    localStorage.setItem("token", res.token);
    localStorage.setItem("role", normalizeRole(res.role));

    setStatus("authStatus", `Sikeres belépés: ${res.email} (${normalizeRole(res.role) || "USER"})`);
    updateAdminButtonVisibility();
    await loadMyAppointments();
  } catch (err) {
    localStorage.removeItem("token");
    localStorage.removeItem("role");
    updateAdminButtonVisibility();
    setStatus("authStatus", `Hiba: ${err.message}`);
  }
}

async function onRegister(e) {
  e.preventDefault();
  setStatus("authStatus", "");

  try {
    const email = el("regEmail").value;
    const password = el("regPassword").value;
    const firstName = el("regFirstName").value || null;
    const lastName = el("regLastName").value || null;

    const res = await apiFetch("/api/auth/register", {
      method: "POST",
      body: JSON.stringify({ email, password, firstName, lastName })
    });

    localStorage.setItem("token", res.token);
    localStorage.setItem("role", normalizeRole(res.role));

    setStatus("authStatus", `Sikeres regisztráció és belépés: ${res.email}`);
    updateAdminButtonVisibility();
    await loadMyAppointments();
  } catch (err) {
    localStorage.removeItem("token");
    localStorage.removeItem("role");
    updateAdminButtonVisibility();
    setStatus("authStatus", `Hiba: ${err.message}`);
  }
}

async function loadSlots() {
  setStatus("bookingStatus", "");
  el("slots").innerHTML = "";

  try {
    const employeeId = Number(el("employeeSelect").value);
    const serviceId = Number(el("serviceSelect").value);
    const date = el("dateInput").value;

    const slots = await apiFetch(`/api/availability/slots?employeeId=${employeeId}&serviceId=${serviceId}&date=${date}`);

    if (!slots.length) {
      el("slots").innerHTML = "<p class='muted'>Nincs elérhető időpont erre a napra.</p>";
      return;
    }

    for (const s of slots) {
      const b = document.createElement("button");
      b.className = "slot-btn";
      const start = new Date(s.startAt);
      b.textContent = start.toLocaleTimeString("hu-HU", { hour: "2-digit", minute: "2-digit" });
      b.addEventListener("click", () => bookSlot(employeeId, serviceId, s.startAt));
      el("slots").appendChild(b);
    }
  } catch (err) {
    setStatus("bookingStatus", `Hiba: ${err.message}`);
  }
}

async function bookSlot(employeeId, serviceId, startAt) {
  setStatus("bookingStatus", "");

  try {
    await apiFetch("/api/appointments", {
      method: "POST",
      body: JSON.stringify({ employeeId, serviceId, startAt })
    });

    setStatus("bookingStatus", "Sikeres foglalás!");
    await loadSlots();
    await loadMyAppointments();
  } catch (err) {
    setStatus("bookingStatus", `Hiba: ${err.message}`);
  }
}

async function loadMyAppointments() {
  const container = el("myAppointments");
  const token = getToken();

  if (!token) {
    container.innerHTML = "<p class='muted'>A foglalásokhoz jelentkezz be.</p>";
    return;
  }

  container.innerHTML = "";

  try {
    const items = await apiFetch("/api/appointments/me");
    const visible = items.filter((a) => Number(a.status) === 1);

    if (!visible.length) {
      container.innerHTML = "<p class='muted'>Még nincs foglalásod.</p>";
      return;
    }

    for (const a of visible) {
      const row = document.createElement("div");
      row.className = "app-item";

      const left = document.createElement("div");
      left.innerHTML = `<div>${fmtDateTime(a.startAt)} → ${fmtDateTime(a.endAt)}</div>
                        <div class="muted">Dolgozó: ${findEmployeeName(a.employeeId)} · Szolgáltatás: ${findServiceName(a.serviceId)} · Státusz: ${statusLabel(a.status)}</div>`;

      const btn = document.createElement("button");
      btn.textContent = "Lemondás";

      btn.addEventListener("click", async () => {
        try {
          await apiFetch(`/api/appointments/${a.id}`, { method: "DELETE" });
          await loadMyAppointments();
          await loadSlots();
        } catch (err) {
          alert(`Lemondás hiba: ${err.message}`);
        }
      });

      row.appendChild(left);
      row.appendChild(btn);
      container.appendChild(row);
    }
  } catch (err) {
    container.innerHTML = `<p class='muted'>Hiba a foglalások lekérésekor: ${err.message}</p>`;
  }
}

function openAdminModal() {
  if (!isAdmin()) {
    setStatus("adminStatus", "Nincs jogosultság.");
    return;
  }

  const modal = el("adminModal");
  if (!modal) {
    return;
  }

  modal.classList.remove("hidden");
  modal.setAttribute("aria-hidden", "false");
}

function closeAdminModal() {
  const modal = el("adminModal");
  if (!modal) {
    return;
  }

  modal.classList.add("hidden");
  modal.setAttribute("aria-hidden", "true");
}

async function loadAdminAppointments() {
  if (!isAdmin()) {
    setStatus("adminStatus", "Nincs jogosultság.");
    return;
  }

  setStatus("adminStatus", "");
  const container = el("adminAppointments");
  container.innerHTML = "";

  const dateFrom = el("adminDateFrom").value;
  const dateTo = el("adminDateTo").value;

  if (!dateFrom || !dateTo) {
    setStatus("adminStatus", "A dátum mezők kitöltése kötelező.");
    return;
  }

  try {
    const items = await apiFetch(`/api/admin/appointments?dateFrom=${dateFrom}&dateTo=${dateTo}`);

    if (!items.length) {
      container.innerHTML = "<p class='muted'>Nincs foglalás a kiválasztott időszakban.</p>";
      return;
    }

    for (const a of items) {
      const row = document.createElement("div");
      row.className = "app-item";

      const userName = a.userName || `Felhasználó #${a.userId}`;
      const employeeName = a.employeeName || findEmployeeName(a.employeeId);
      const serviceName = a.serviceName || findServiceName(a.serviceId);

      row.innerHTML = `<div>
        <div>${fmtDateTime(a.startAt)} → ${fmtDateTime(a.endAt)}</div>
        <div class="muted">Vendég: ${userName} · Dolgozó: ${employeeName} · Szolgáltatás: ${serviceName} · Státusz: ${statusLabel(a.status)}</div>
      </div>`;

      container.appendChild(row);
    }
  } catch (err) {
    setStatus("adminStatus", `Hiba: ${err.message}`);
  }
}

document.addEventListener("DOMContentLoaded", async () => {
  el("loginForm").addEventListener("submit", onLogin);
  el("registerForm").addEventListener("submit", onRegister);
  el("logoutBtn").addEventListener("click", logout);
  el("loadSlotsBtn").addEventListener("click", loadSlots);
  el("loadMyAppointmentsBtn").addEventListener("click", loadMyAppointments);

  const openAdminBtn = el("openAdminBtn");
  if (openAdminBtn) {
    openAdminBtn.addEventListener("click", openAdminModal);
  }

  const adminModalCloseBtn = el("adminModalCloseBtn");
  if (adminModalCloseBtn) {
    adminModalCloseBtn.addEventListener("click", closeAdminModal);
  }

  const adminModalCloseBackdrop = el("adminModalCloseBackdrop");
  if (adminModalCloseBackdrop) {
    adminModalCloseBackdrop.addEventListener("click", closeAdminModal);
  }

  const loadAdminAppointmentsBtn = el("loadAdminAppointmentsBtn");
  if (loadAdminAppointmentsBtn) {
    loadAdminAppointmentsBtn.addEventListener("click", loadAdminAppointments);
  }

  const token = getToken();

  if (token) {
    setStatus("authStatus", "Bejelentkezve (mentett munkamenet).");
    await loadMyAppointments();
  } else {
    setStatus("authStatus", "Nem vagy bejelentkezve.");
    el("myAppointments").innerHTML = "<p class='muted'>A foglalásokhoz jelentkezz be.</p>";
  }

  try {
    await loadLookups();
  } catch (err) {
    setStatus("bookingStatus", `Hiba a törzsadatok betöltésekor: ${err.message}`);
  }

  updateAdminButtonVisibility();
});
