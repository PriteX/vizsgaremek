const el = (id) => document.getElementById(id);

function setStatus(id, msg) {
  el(id).textContent = msg || "";
}
function getToken() {
  return localStorage.getItem("token"); 
}
function logout() {
  localStorage.removeItem("token");
  localStorage.removeItem("role");

  setStatus("authStatus", "Kijelentkezve.");
  el("myAppointments").innerHTML = "<p class='muted'>A foglalásokhoz jelentkezz be.</p>";
  el("slots").innerHTML = "";
  setStatus("bookingStatus", "");
}


async function loadLookups() {
  const baseUrl = window.APP_CONFIG.baseUrl;
  el("baseUrlLabel").textContent = baseUrl;

  const services = await apiFetch("/api/services");
  const employees = await apiFetch("/api/employees");

  el("serviceSelect").innerHTML = services
    .filter(s => s.isActive)
    .map(s => `<option value="${s.id}">${s.name} (${s.durationMinutes} perc)</option>`)
    .join("");

  el("employeeSelect").innerHTML = employees
    .filter(e => e.isActive)
    .map(e => `<option value="${e.id}">${e.name}</option>`)
    .join("");

  const tomorrow = new Date(Date.now() + 24*60*60*1000);
  el("dateInput").value = tomorrow.toISOString().slice(0,10);
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
    localStorage.setItem("role", res.role);
    setStatus("authStatus", "Sikeres belépés: " + res.email + " (" + res.role + ")");
    el("myAppointments").innerHTML = "";
await loadMyAppointments();
  } catch (err) {
    localStorage.removeItem("token");
localStorage.removeItem("role");

    setStatus("authStatus", "Hiba: " + err.message);
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
    localStorage.setItem("role", res.role);
    setStatus("authStatus", "Sikeres regisztráció és belépés: " + res.email);
    el("myAppointments").innerHTML = "";
await loadMyAppointments();
  } catch (err) {
    localStorage.removeItem("token");
localStorage.removeItem("role");

    setStatus("authStatus", "Hiba: " + err.message);
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
      b.textContent = start.toLocaleTimeString("hu-HU", { hour:"2-digit", minute:"2-digit" });
      b.addEventListener("click", () => bookSlot(employeeId, serviceId, s.startAt));
      el("slots").appendChild(b);
    }
  } catch (err) {
    setStatus("bookingStatus", "Hiba: " + err.message);
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
    setStatus("bookingStatus", "Hiba: " + err.message);
  }
}

async function loadMyAppointments() {
  const container = el("myAppointments");
  const token = getToken();

  if (!token) {
    localStorage.removeItem("token");
    localStorage.removeItem("role");
    container.innerHTML = "<p class='muted'>A foglalásokhoz jelentkezz be.</p>";
    return;
  }

  container.innerHTML = "";

  try {
    const items = await apiFetch("/api/appointments/me");

    // csak a foglaltakat mutatjuk (Booked = 1)
    const visible = items.filter(a => a.status === 1);

    if (!visible.length) {
      container.innerHTML = "<p class='muted'>Még nincs foglalásod.</p>";
      return;
    }

    for (const a of visible) {
      const row = document.createElement("div");
      row.className = "app-item";

      const left = document.createElement("div");
      left.innerHTML = `<div><strong>#${a.id}</strong> – ${fmtDateTime(a.startAt)} → ${fmtDateTime(a.endAt)}</div>
                        <div class="muted">employeeId=${a.employeeId}, serviceId=${a.serviceId}, status=${a.status}</div>`;

      const btn = document.createElement("button");
      btn.textContent = "Lemondás";

      btn.addEventListener("click", async () => {
        try {
          console.log("DELETE id =", a.id, a);
          await apiFetch("/api/appointments/" + a.id, { method: "DELETE" });
          await loadMyAppointments();
          await loadSlots();
        } catch (err) {
          console.error("DELETE failed:", err);
          alert("Lemondás hiba: " + err.message);
        }
      });

      row.appendChild(left);
      row.appendChild(btn);
      container.appendChild(row);
    }
  } catch (err) {
    console.error("loadMyAppointments error:", err);
    container.innerHTML = `<p class='muted'>Hiba a foglalások lekérésekor: ${err.message}</p>`;
  }
}


document.addEventListener("DOMContentLoaded", async () => {
  // gombok / formok
  el("loginForm").addEventListener("submit", onLogin);
  el("registerForm").addEventListener("submit", onRegister);
  el("logoutBtn").addEventListener("click", logout);
  el("loadSlotsBtn").addEventListener("click", loadSlots);

  el("loadMyAppointmentsBtn").addEventListener("click", () => {
    if (!getToken()) {
      el("myAppointments").innerHTML = "<p class='muted'>A foglalásokhoz jelentkezz be.</p>";
      return;
    }
    loadMyAppointments();
  });

  // induló üzenet + automatikus betöltés
  const token = getToken();

  if (token) {
    setStatus("authStatus", "Bejelentkezve (mentett munkamenet).");
    await loadMyAppointments(); // F5 után is töltse be
  } else {
    setStatus("authStatus", "Nem vagy bejelentkezve.");
    el("myAppointments").innerHTML = "<p class='muted'>A foglalásokhoz jelentkezz be.</p>";
  }

  await loadLookups(); // service/employee lista
});

