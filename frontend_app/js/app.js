const el = (id) => document.getElementById(id);

const state = {
  services: [],
   employees: [],
  locations: [],
  selectedLocationId: 0
};

function setStatus(id, msg) {
  const target = el(id);
  if (!target) {
    return;
  }
  target.textContent = msg || "";
}

function setAuthenticatedView(email) {
  const isLoggedIn = Boolean(getToken());

  document.querySelectorAll(".booking-panel").forEach((section) => {
    section.classList.toggle("hidden", !isLoggedIn);
  });

  const authCard = el("authCard");
  if (authCard) {
    authCard.classList.toggle("hidden", isLoggedIn);
  }

  const loginSuccessBanner = el("loginSuccessBanner");
  if (loginSuccessBanner) {
    const bannerText = isLoggedIn ? `Sikeres belépés ${email || "Felhasználó"}` : "";
    loginSuccessBanner.textContent = bannerText;
    loginSuccessBanner.classList.toggle("hidden", !bannerText);
  }

  const headerLogoutBtn = el("headerLogoutBtn");
  if (headerLogoutBtn) {
    headerLogoutBtn.classList.toggle("hidden", !isLoggedIn);
  }

  const openAdminBtn = el("openAdminBtn");
  if (openAdminBtn) {
openAdminBtn.classList.toggle("hidden", !isLoggedIn || !isAdmin());
  }
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

  const isLoggedIn = Boolean(getToken());
 openAdminBtn.classList.toggle("hidden", !isLoggedIn || !isAdmin());
}

function updateLocationSelectors() {
  const categorySelect = el("categorySelect");
  const employeeLocation = el("employeeLocation");

  const activeLocations = state.locations.filter((location) => location.isActive);

  if (categorySelect) {
    categorySelect.innerHTML = activeLocations
      .map((location) => `<option value="${location.id}">${location.name}</option>`)
      .join("");

    if (!activeLocations.length) {
      categorySelect.innerHTML = "<option value=''>Nincs elérhető helyszín</option>";
      state.selectedLocationId = 0;
    } else {
      const hasSelected = activeLocations.some((location) => location.id === state.selectedLocationId);
      state.selectedLocationId = hasSelected ? state.selectedLocationId : activeLocations[0].id;
      categorySelect.value = String(state.selectedLocationId);
    }
  }

  if (employeeLocation) {
    employeeLocation.innerHTML = ["<option value=''>Nincs hozzárendelve</option>"]
      .concat(state.locations.map((location) => `<option value="${location.id}">${location.name}${location.isActive ? "" : " (inaktív)"}</option>`))
      .join("");
  }
}

function getEmployeesBySelectedLocation() {
  if (!state.selectedLocationId) {
    return [];
  }

  return state.employees.filter((employee) => employee.isActive && Number(employee.locationId) === Number(state.selectedLocationId));
}

function renderServicesByLocation() {
  const serviceSelect = el("serviceSelect");
  if (!serviceSelect) {
    return;
  }

  const employeesAtLocation = getEmployeesBySelectedLocation();
  const serviceIdSet = new Set();

  for (const employee of employeesAtLocation) {
    const serviceIds = Array.isArray(employee.serviceIds) ? employee.serviceIds : [];
    for (const serviceId of serviceIds) {
      serviceIdSet.add(serviceId);
    }
  }

  const visibleServices = state.services
    .filter((service) => service.isActive && serviceIdSet.has(service.id))
    .sort((a, b) => a.name.localeCompare(b.name, "hu-HU"));

  if (!visibleServices.length) {
    serviceSelect.innerHTML = "<option value=''>Nincs elérhető szolgáltatás ezen a helyszínen</option>";
    return;
  }

  serviceSelect.innerHTML = visibleServices
    .map((service) => `<option value="${service.id}">${service.name} (${service.durationMinutes} perc)</option>`)
    .join("");
}

function onLocationChange() {
  const categorySelect = el("categorySelect");
  if (!categorySelect) {
    return;
  }

  state.selectedLocationId = Number(categorySelect.value || 0);
  renderServicesByLocation();
  filterEmployeesBySelectedService();
  el("slots").innerHTML = "";
  setStatus("bookingStatus", "");
}

function findServiceName(serviceId) {
  return state.services.find((s) => s.id === Number(serviceId))?.name || `Szolgáltatás #${serviceId}`;
}

function findEmployeeName(employeeId) {
  return state.employees.find((e) => e.id === Number(employeeId))?.name || `Dolgozó #${employeeId}`;
}

function getSelectedEmployeeServiceIds() {
  return Array.from(document.querySelectorAll("input[name='employeeService']:checked"))
    .map((input) => Number(input.value))
    .filter((id) => id > 0);
}

function renderEmployeeServicesSelector(selectedIds = []) {
  const container = el("employeeServices");
  if (!container) {
    return;
  }

  const selected = new Set((selectedIds || []).map((id) => Number(id)));
  const activeServices = state.services.filter((service) => service.isActive);

  if (!activeServices.length) {
    container.innerHTML = "<p class='muted'>Nincs aktív szolgáltatás.</p>";
    return;
  }

  container.innerHTML = activeServices
    .map(
      (service) => `<label><input type="checkbox" name="employeeService" value="${service.id}" ${selected.has(service.id) ? "checked" : ""}/> ${service.name}</label>`
    )
    .join("");
}

function filterEmployeesBySelectedService() {
  const employeeSelect = el("employeeSelect");
  const serviceSelect = el("serviceSelect");

  if (!employeeSelect || !serviceSelect) {
    return;
  }

  const selectedServiceId = Number(serviceSelect.value || 0);
    const servicesInCategory = getServicesBySelectedCategory();
  const categoryServiceIds = new Set(servicesInCategory.map((s) => s.id));
  const employees = getEmployeesBySelectedLocation();

  const eligible = employees.filter((employee) => {
    const employeeServiceIds = Array.isArray(employee.serviceIds) ? employee.serviceIds : [];
    return selectedServiceId > 0 ? employeeServiceIds.includes(selectedServiceId) : employeeServiceIds.length > 0;
  });


  employeeSelect.innerHTML = eligible.map((employee) => `<option value="${employee.id}">${employee.name}</option>`).join("");

  if (!eligible.length) {
   employeeSelect.innerHTML = "<option value=''>Nincs megfelelő dolgozó ezen a helyszínen</option>";
  }
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
   localStorage.removeItem("email");

  setStatus("authStatus", "Kijelentkezve.");
   setAuthenticatedView();

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
  state.locations = await apiFetch("/api/locations");

  const baseEmployees = await apiFetch("/api/employees");
  state.employees = await Promise.all(baseEmployees.map(async (employee) => {
    try {
      const serviceIds = await apiFetch(`/api/employees/${employee.id}/services`);
      return { ...employee, serviceIds };
    } catch {
      return { ...employee, serviceIds: [] };
    }
  }));

 updateLocationSelectors();
  renderServicesByLocation();

  filterEmployeesBySelectedService();

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
    localStorage.setItem("email", res.email);

    setStatus("authStatus", `Sikeres belépés: ${res.email} (${normalizeRole(res.role) || "USER"})`);
    setAuthenticatedView(res.email);
    updateAdminButtonVisibility();
    await loadMyAppointments();
  } catch (err) {
    localStorage.removeItem("token");
    localStorage.removeItem("role");
     localStorage.removeItem("email");
    setAuthenticatedView();
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
        localStorage.setItem("email", res.email);

    setStatus("authStatus", `Sikeres regisztráció és belépés: ${res.email}`);
    setAuthenticatedView(res.email);
    updateAdminButtonVisibility();
    await loadMyAppointments();
  } catch (err) {
    localStorage.removeItem("token");
    localStorage.removeItem("role");
      localStorage.removeItem("email");
    setAuthenticatedView();
    updateAdminButtonVisibility();
    setStatus("authStatus", `Hiba: ${err.message}`);
  }
}

async function loadSlots() {
  setStatus("bookingStatus", "");
  el("slots").innerHTML = "";

    if (!getToken()) {
    setStatus("bookingStatus", "Előbb jelentkezz be a foglaláshoz.");
    return;
  }
  try {
    const employeeId = Number(el("employeeSelect").value);
    const serviceId = Number(el("serviceSelect").value);
    const date = el("dateInput").value;

   if (!employeeId || !serviceId || !date) {
      setStatus("bookingStatus", "Válassz helyszínt, szolgáltatást, dolgozót és dátumot.");
      return;
    }
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

  if (!getToken()) {
    setStatus("bookingStatus", "Előbb jelentkezz be a foglaláshoz.");
    return;
  }


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
        if (/401|403/.test(String(err.message))) {
      logout();
      container.innerHTML = "<p class='muted'>A munkamenet lejárt, jelentkezz be újra.</p>";
      return;
    }

    container.innerHTML = `<p class='muted'>Hiba a foglalások lekérésekor: ${err.message}</p>`;
  }
}

function openAdminModal() {
  if (!isAdmin()) {
     setStatus("authStatus", "Nincs admin jogosultságod.");
    return;
  }

  const modal = el("adminModal");
  if (!modal) {
    return;
  }

  modal.classList.remove("hidden");
  modal.setAttribute("aria-hidden", "false");
   resetEmployeeForm();
  loadAdminEmployees();
   loadAdminLocations();
  resetLocationForm();
}



function resetEmployeeForm() {
  const employeeForm = el("employeeForm");
  if (employeeForm) {
    employeeForm.reset();
  }

  const employeeId = el("employeeId");
  const employeeIsActive = el("employeeIsActive");

  if (employeeId) {
    employeeId.value = "";
  }

  if (employeeIsActive) {
    employeeIsActive.checked = true;
  }
}

  renderEmployeeServicesSelector([]);

async function loadAdminEmployees() {
  if (!isAdmin()) {
    return;
  }

  const container = el("adminEmployees");
  if (!container) {
    return;
  }

  container.innerHTML = "";

  try {
    const baseItems = await apiFetch("/api/employees");
    const items = await Promise.all(baseItems.map(async (employee) => {
      try {
        const serviceIds = await apiFetch(`/api/employees/${employee.id}/services`);
        return { ...employee, serviceIds };
      } catch {
        return { ...employee, serviceIds: [] };
      }
    }));

    if (!items.length) {
      container.innerHTML = "<p class='muted'>Nincs dolgozó az adatbázisban.</p>";
      return;
    }

    for (const employee of items) {
      const row = document.createElement("div");
      row.className = "app-item";

      const info = document.createElement("div");
      const serviceNames = (employee.serviceIds || [])
        .map((serviceId) => findServiceName(serviceId))
        .join(", ");

      info.innerHTML = `<div>${employee.name}</div>
            <div class="muted">E-mail: ${employee.email || "-"} · Telefon: ${employee.phone || "-"} · Helyszín: ${employee.locationName || "nincs"} · Állapot: ${employee.isActive ? "Aktív" : "Inaktív"}</div>
        <div class="muted">Szolgáltatások: ${serviceNames || "nincs kiválasztva"}</div>`;

      const actions = document.createElement("div");
      actions.className = "row";

      const editBtn = document.createElement("button");
      editBtn.type = "button";
      editBtn.textContent = "Szerkesztés";
      editBtn.addEventListener("click", async () => {
        el("employeeId").value = employee.id;
        el("employeeName").value = employee.name || "";
        el("employeeEmail").value = employee.email || "";
        el("employeePhone").value = employee.phone || "";
        el("employeeIsActive").checked = !!employee.isActive;
         el("employeeLocation").value = employee.locationId ? String(employee.locationId) : "";
      
        try {
          const serviceIds = await apiFetch(`/api/employees/${employee.id}/services`);
          renderEmployeeServicesSelector(serviceIds);
        } catch (err) {
          setStatus("adminStatus", `Hiba a dolgozó szolgáltatásainak betöltésekor: ${err.message}`);
        }
      });

      const deleteBtn = document.createElement("button");
      deleteBtn.type = "button";
      deleteBtn.textContent = "Törlés";
      deleteBtn.addEventListener("click", async () => {
        if (!confirm(`Biztosan törlöd ezt a dolgozót: ${employee.name}?`)) {
          return;
        }

        try {
          await apiFetch(`/api/employees/${employee.id}`, { method: "DELETE" });
          setStatus("adminStatus", "Dolgozó törölve.");
          await loadLookups();
          await loadAdminEmployees();
        } catch (err) {
          setStatus("adminStatus", `Hiba: ${err.message}`);
        }
      });

      actions.appendChild(editBtn);
      actions.appendChild(deleteBtn);

      row.appendChild(info);
      row.appendChild(actions);
      container.appendChild(row);
    }
  } catch (err) {
    setStatus("adminStatus", `Hiba: ${err.message}`);
  }
}

async function onEmployeeSubmit(e) {
  e.preventDefault();

  if (!isAdmin()) {
    setStatus("adminStatus", "Nincs jogosultság.");
    return;
  }

  const id = Number(el("employeeId").value || 0);
  const name = el("employeeName").value.trim();
  const email = el("employeeEmail").value.trim() || null;
  const phone = el("employeePhone").value.trim() || null;
  const isActive = !!el("employeeIsActive").checked;
    const locationIdRaw = el("employeeLocation").value;
  const locationId = locationIdRaw ? Number(locationIdRaw) : null;
const serviceIds = getSelectedEmployeeServiceIds();
  if (!name) {
    setStatus("adminStatus", "A név megadása kötelező.");
    return;
  }

    const payload = { name, email, phone, isActive, locationId };

  try {
    if (id > 0) {
      await apiFetch(`/api/employees/${id}`, {
        method: "PUT",
        body: JSON.stringify(payload)
      });
      setStatus("adminStatus", "Dolgozó frissítve.");
         await apiFetch(`/api/employees/${id}/services`, {
        method: "PUT",
        body: JSON.stringify({ serviceIds })
      });
    } else {
      const created = await apiFetch("/api/employees", {
        method: "POST",
        body: JSON.stringify(payload)
      });
           await apiFetch(`/api/employees/${created.id}/services`, {
        method: "PUT",
        body: JSON.stringify({ serviceIds })
      });
      setStatus("adminStatus", "Dolgozó létrehozva.");
    }

    resetEmployeeForm();
    await loadLookups();
    await loadAdminEmployees();
  } catch (err) {
    setStatus("adminStatus", `Hiba: ${err.message}`);
  }
}
function resetLocationForm() {
  const locationForm = el("locationForm");
  if (locationForm) {
    locationForm.reset();
  }

  el("locationId").value = "";
  el("locationIsActive").checked = true;
}

async function loadAdminLocations() {
  if (!isAdmin()) {
    return;
  }

  const container = el("adminLocations");
  if (!container) {
    return;
  }

  container.innerHTML = "";

  const items = await apiFetch("/api/locations");
  if (!items.length) {
    container.innerHTML = "<p class='muted'>Nincs helyszín.</p>";
    return;
  }

  for (const location of items) {
    const row = document.createElement("div");
    row.className = "app-item";
    row.innerHTML = `<div><div>${location.name}</div><div class='muted'>Állapot: ${location.isActive ? "Aktív" : "Inaktív"}</div></div>`;

    const actions = document.createElement("div");
    actions.className = "row";

    const editBtn = document.createElement("button");
    editBtn.type = "button";
    editBtn.textContent = "Szerkesztés";
    editBtn.addEventListener("click", () => {
      el("locationId").value = location.id;
      el("locationName").value = location.name;
      el("locationIsActive").checked = !!location.isActive;
    });

    const deleteBtn = document.createElement("button");
    deleteBtn.type = "button";
    deleteBtn.textContent = "Inaktiválás";
    deleteBtn.addEventListener("click", async () => {
      await apiFetch(`/api/locations/${location.id}`, { method: "DELETE" });
      await loadLookups();
      await loadAdminLocations();
      await loadAdminEmployees();
    });

    actions.appendChild(editBtn);
    actions.appendChild(deleteBtn);
    row.appendChild(actions);
    container.appendChild(row);
  }
}

async function onLocationSubmit(e) {
  e.preventDefault();

  if (!isAdmin()) {
    setStatus("adminStatus", "Nincs jogosultság.");
    return;
  }

  const id = Number(el("locationId").value || 0);
  const name = el("locationName").value.trim();
  const isActive = !!el("locationIsActive").checked;

  if (!name) {
    setStatus("adminStatus", "A helyszín neve kötelező.");
    return;
  }

  try {
    if (id > 0) {
      await apiFetch(`/api/locations/${id}`, { method: "PUT", body: JSON.stringify({ name, isActive }) });
      setStatus("adminStatus", "Helyszín frissítve.");
    } else {
      await apiFetch("/api/locations", { method: "POST", body: JSON.stringify({ name, isActive }) });
      setStatus("adminStatus", "Helyszín létrehozva.");
    }

    resetLocationForm();
    await loadLookups();
    await loadAdminLocations();
    await loadAdminEmployees();
  } catch (err) {
    setStatus("adminStatus", `Hiba: ${err.message}`);
  }
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
el("headerLogoutBtn").addEventListener("click", logout);
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
    const employeeForm = el("employeeForm");
  if (employeeForm) {
    employeeForm.addEventListener("submit", onEmployeeSubmit);
  }

  const employeeResetBtn = el("employeeResetBtn");
  if (employeeResetBtn) {
    employeeResetBtn.addEventListener("click", resetEmployeeForm);
  }

const categorySelect = el("categorySelect");
  if (categorySelect) {
    categorySelect.addEventListener("change", onLocationChange);
  }

  const locationForm = el("locationForm");
  if (locationForm) {
    locationForm.addEventListener("submit", onLocationSubmit);
  }

  const locationResetBtn = el("locationResetBtn");
  if (locationResetBtn) {
    locationResetBtn.addEventListener("click", resetLocationForm);
  }

  const serviceSelect = el("serviceSelect");
  if (serviceSelect) {
    serviceSelect.addEventListener("change", filterEmployeesBySelectedService);
  }


  const token = getToken();

  if (token) {
        const payload = decodeJwtPayload(token);
    const savedEmail = payload?.email || payload?.unique_name || payload?.sub || localStorage.getItem("email") || "Felhasználó";
    setStatus("authStatus", "Bejelentkezve (mentett munkamenet).");
    setAuthenticatedView(savedEmail);
    await loadMyAppointments();
  } else {
       setAuthenticatedView();
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
