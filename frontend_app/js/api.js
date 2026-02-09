async function apiFetch(path, options = {}) {
  const baseUrl = window.APP_CONFIG.baseUrl;
  const token = localStorage.getItem("token");

  const headers = Object.assign({}, options.headers || {});
  if (!headers["Content-Type"] && options.body) headers["Content-Type"] = "application/json";
  if (token) headers["Authorization"] = "Bearer " + token;

  const res = await fetch(baseUrl + path, { ...options, headers });

  let data = null;
  const contentType = res.headers.get("content-type") || "";
  if (contentType.includes("application/json")) {
    data = await res.json().catch(() => null);
  } else {
    data = await res.text().catch(() => null);
  }

  if (!res.ok) {
    const msg = (data && data.error) ? data.error : ("HTTP hiba: " + res.status);
    throw new Error(msg);
  }

  return data;
}

function fmtDateTime(dtStr) {
  const d = new Date(dtStr);
  return d.toLocaleString("hu-HU", { year:"numeric", month:"2-digit", day:"2-digit", hour:"2-digit", minute:"2-digit" });
}
