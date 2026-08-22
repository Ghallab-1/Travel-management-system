const API_BASE =
  process.env.REACT_APP_API_URL || 'http://localhost:5044';

function getAuthHeaders() {
  try {
    const token = window.localStorage.getItem('tms_token');

    if (token) {
      return {
        Authorization: `Bearer ${token}`,
        'Content-Type': 'application/json',
      };
    }
  } catch (e) {}

  return {
    'Content-Type': 'application/json',
  };
}

async function request(path, options = {}) {
  const url = `${API_BASE}${path}`;

  const opts = {
    ...options,
    headers: {
      ...getAuthHeaders(),
      ...(options.headers || {}),
    },
  };

  const res = await fetch(url, opts);

  if (!res.ok) {
    const txt = await res.text();

    const err = new Error(
      `HTTP ${res.status}: ${txt}`
    );

    err.status = res.status;

    throw err;
  }

  const contentType =
    res.headers.get('content-type') || '';

  if (contentType.includes('application/json')) {
    return res.json();
  }

  return res.text();
}


// ============================================================
// AUTH
// ============================================================

export async function login(email, password) {
  return request('/api/auth/login', {
    method: 'POST',
    body: JSON.stringify({
      email,
      password,
    }),
  });
}


// ============================================================
// TRAVEL REQUESTS
// ============================================================

export async function getTravelRequests() {
  return request('/api/travelrequests', {
    method: 'GET',
  });
}

export async function getPendingForMe() {
  return request('/api/travelrequests/pending-for-me', {
    method: 'GET',
  });
}

export async function getCoordinatorWork() {
  return request('/api/travelrequests/coordinator-work', {
    method: 'GET',
  });
}

export async function getHrReviewRequests() {
  return request('/api/travelrequests/hr-review', {
    method: 'GET',
  });
}

export async function getMyRequests(userId) {
  const all = await getTravelRequests();

  return (all || []).filter(
    (r) => Number(r.userId) === Number(userId)
  );
}

export async function createRequest(payload) {
  return request('/api/travelrequests', {
    method: 'POST',
    body: JSON.stringify(payload),
  });
}

export async function getRequestById(id) {
  return request(
    `/api/travelrequests/${encodeURIComponent(id)}`,
    {
      method: 'GET',
    }
  );
}

export function getTravelDocumentUrl(id) {
  return `${API_BASE}/api/travelrequests/${encodeURIComponent(id)}/document`;
}

export async function updateCoordinatorDetails(id, payload) {
  return request(
    `/api/travelrequests/${encodeURIComponent(id)}/coordinator-details`,
    {
      method: 'PATCH',
      body: JSON.stringify(payload),
    }
  );
}

export async function updatePerDiem(id, payload) {
  return request(
    `/api/travelrequests/${encodeURIComponent(id)}/per-diem`,
    {
      method: 'PATCH',
      body: JSON.stringify(payload),
    }
  );
}


// ============================================================
// NOTIFICATIONS
// ============================================================

export async function getNotifications(userId) {
  return request(
    `/api/notifications/user/${encodeURIComponent(userId)}`,
    {
      method: 'GET',
    }
  );
}

export async function markNotificationRead(id) {
  return request(
    `/api/notifications/markread/${encodeURIComponent(id)}`,
    {
      method: 'POST',
    }
  );
}


// ============================================================
// CITIES
// ============================================================

export async function getCities() {
  return request('/api/cities', {
    method: 'GET',
  });
}

export async function getHotels() {
  return request('/api/hotels', {
    method: 'GET',
  });
}

export async function getCurrencies() {
  return request('/api/currencies', {
    method: 'GET',
  });
}

export async function getExpenseCategories() {
  return request('/api/expensecategories', {
    method: 'GET',
  });
}


// ============================================================
// BOOKINGS
// ============================================================

export async function getBookings() {
  return request('/api/bookings', {
    method: 'GET',
  });
}

export async function getBookingsByRequest(requestId) {
  return request(
    `/api/bookings/byrequest/${encodeURIComponent(requestId)}`,
    {
      method: 'GET',
    }
  );
}

export async function createBooking(payload) {
  return request('/api/bookings', {
    method: 'POST',
    body: JSON.stringify(payload),
  });
}


// ============================================================
// FLIGHTS
// ============================================================

export async function getFlights() {
  return request('/api/flights', {
    method: 'GET',
  });
}

export async function createFlight(payload) {
  return request('/api/flights', {
    method: 'POST',
    body: JSON.stringify(payload),
  });
}


// ============================================================
// HOTEL RESERVATIONS
// ============================================================

export async function getHotelReservations() {
  return request('/api/hotelreservations', {
    method: 'GET',
  });
}

export async function createHotelReservation(payload) {
  return request('/api/hotelreservations', {
    method: 'POST',
    body: JSON.stringify(payload),
  });
}


// ============================================================
// EXPENSES
// ============================================================

export async function getExpenses() {
  return request('/api/expenses', {
    method: 'GET',
  });
}

export async function getExpensesByRequest(requestId) {
  return request(
    `/api/expenses/travelrequest/${encodeURIComponent(requestId)}`,
    {
      method: 'GET',
    }
  );
}

export async function createExpense(payload) {
  return request('/api/expenses', {
    method: 'POST',
    body: JSON.stringify(payload),
  });
}


// ============================================================
// APPROVE / REJECT
// ============================================================

export async function approveRequest(
  travelRequestId,
  approverId,
  comments = '',
  approvalLevel = ''
) {
  const payload = {
    TravelRequestId: travelRequestId,
    ApproverId: approverId,
    ApprovalLevel: approvalLevel,
    Decision: 'Approved',
    Comments: comments || '',
  };

  return request(
    `/api/travelrequests/${encodeURIComponent(travelRequestId)}/approve`,
    {
      method: 'POST',
      body: JSON.stringify(payload),
    }
  );
}

export async function rejectRequest(
  travelRequestId,
  approverId,
  comments = '',
  approvalLevel = ''
) {
  const payload = {
    TravelRequestId: travelRequestId,
    ApproverId: approverId,
    ApprovalLevel: approvalLevel,
    Decision: 'Rejected',
    Comments: comments || '',
  };

  return request(
    `/api/travelrequests/${encodeURIComponent(travelRequestId)}/reject`,
    {
      method: 'POST',
      body: JSON.stringify(payload),
    }
  );
}


// ============================================================
// DEPARTMENTS
// ============================================================

export async function getDepartments() {
  return request('/api/departments', {
    method: 'GET',
  });
}

export async function createDepartment(payload) {
  return request('/api/departments', {
    method: 'POST',
    body: JSON.stringify(payload),
  });
}

export async function updateDepartment(id, payload) {
  return request(
    `/api/departments/${encodeURIComponent(id)}`,
    {
      method: 'PUT',
      body: JSON.stringify(payload),
    }
  );
}

export async function deleteDepartment(id) {
  return request(
    `/api/departments/${encodeURIComponent(id)}`,
    {
      method: 'DELETE',
    }
  );
}


// ============================================================
// AIRLINES
// ============================================================

export async function getAirlines() {
  return request('/api/airlines', {
    method: 'GET',
  });
}

export async function createAirline(payload) {
  return request('/api/airlines', {
    method: 'POST',
    body: JSON.stringify(payload),
  });
}

export async function updateAirline(id, payload) {
  return request(
    `/api/airlines/${encodeURIComponent(id)}`,
    {
      method: 'PUT',
      body: JSON.stringify(payload),
    }
  );
}

export async function deleteAirline(id) {
  return request(
    `/api/airlines/${encodeURIComponent(id)}`,
    {
      method: 'DELETE',
    }
  );
}


// ============================================================
// API OBJECT
// ============================================================

const Api = {
  login,

  getTravelRequests,
  getPendingForMe,
  getCoordinatorWork,
  getHrReviewRequests,
  getMyRequests,
  createRequest,
  getRequestById,
  getTravelDocumentUrl,
  updateCoordinatorDetails,
  updatePerDiem,

  getNotifications,
  markNotificationRead,

  getCities,
  getHotels,
  getCurrencies,
  getExpenseCategories,

  getBookings,
  getBookingsByRequest,
  createBooking,

  getFlights,
  createFlight,

  getHotelReservations,
  createHotelReservation,

  getExpenses,
  getExpensesByRequest,
  createExpense,

  approveRequest,
  rejectRequest,

  getDepartments,
  createDepartment,
  updateDepartment,
  deleteDepartment,

  getAirlines,
  createAirline,
  updateAirline,
  deleteAirline,
};

export default Api;
