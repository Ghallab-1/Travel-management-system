// Simple mock API service that simulates async operations using local JSON data.
// This keeps the frontend self-contained and easy for beginners to understand.

import requestsData from '../data/requests.json';
import usersData from '../data/users.json';

// Storage key for persisting mock data in localStorage
const STORAGE_KEY = 'tms_requests_v1';

function delay(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

function loadInitialRequests() {
  try {
    const raw = window.localStorage.getItem(STORAGE_KEY);
    if (raw) return JSON.parse(raw);
  } catch (e) {
    // ignore
  }
  // fallback to bundled JSON
  return JSON.parse(JSON.stringify(requestsData));
}

function saveRequests(reqs) {
  try {
    window.localStorage.setItem(STORAGE_KEY, JSON.stringify(reqs));
  } catch (e) {
    // ignore
  }
}

// In-memory copy (initialized from localStorage if available)
const requests = loadInitialRequests();
const users = JSON.parse(JSON.stringify(usersData));

export async function getUser(userId) {
  await delay(200);
  return users.find((u) => u.id === userId) || null;
}

export async function getRequestsForUser(userId) {
  await delay(250);
  return requests.filter((r) => r.userId === userId);
}

export async function getAllRequests() {
  await delay(250);
  return requests;
}

export async function getRequestById(id) {
  await delay(200);
  return requests.find((r) => r.id === id) || null;
}

export async function createRequest(payload) {
  await delay(300);
  const max = requests.reduce((m, r) => Math.max(m, parseInt((r.id || '').replace('req-', '') || 0)), 0);
  const id = 'req-' + (max + 1);
  const newReq = Object.assign({ id, status: 'Draft', createdAt: new Date().toISOString() }, payload);
  requests.push(newReq);
  saveRequests(requests);
  return newReq;
}

export async function updateRequest(id, patch) {
  await delay(200);
  const idx = requests.findIndex((r) => r.id === id);
  if (idx === -1) return null;
  requests[idx] = { ...requests[idx], ...patch, updatedAt: new Date().toISOString() };
  saveRequests(requests);
  return requests[idx];
}

export async function updateRequestStatus(id, status) {
  return updateRequest(id, { status });
}

export async function approveRequest(id, approverId, comments) {
  // Implement multi-level approval sequence
  await delay(300);
  const idx = requests.findIndex((r) => r.id === id);
  if (idx === -1) return null;
  const req = requests[idx];

  // Ensure approvals structure
  if (!req.approvals) {
    req.approvals = [];
  }
  if (!req._approvalSequence) {
    req._approvalSequence = ['Direct Manager','Department Manager','HR','Finance','Travel Coordinator'];
  }

  // Determine next level
  const completed = req.approvals.filter(a => a.decision === 'Approved').map(a => a.level);
  const nextLevel = req._approvalSequence.find(l => !completed.includes(l));

  // Add approval record for this approver at nextLevel
  const approvalRecord = { approverId, level: nextLevel || 'Unknown', decision: 'Approved', comments, date: new Date().toISOString() };
  req.approvals.push(approvalRecord);

  // Decide new status
  const remaining = req._approvalSequence.filter(l => !req.approvals.some(a => a.level === l && a.decision === 'Approved'));
  if (remaining.length === 0) {
    req.status = 'Approved';
  } else {
    req.status = 'Under Review';
  }
  req.updatedAt = new Date().toISOString();
  saveRequests(requests);

  // Add notification to the request owner (lazy, will import notifications service dynamically to avoid cycle)
  try {
    const notifSvc = await import('./notifications');
    notifSvc.addNotification(req.userId, {
      title: 'Request Updated',
      message: `Your request ${req.id} was approved by ${approverId} at level ${approvalRecord.level}`,
      createdAt: new Date().toISOString(),
    });
  } catch (e) {
    // ignore
  }

  return req;
}

export async function rejectRequest(id, approverId, comments) {
  await delay(300);
  const idx = requests.findIndex((r) => r.id === id);
  if (idx === -1) return null;
  const req = requests[idx];
  if (!req.approvals) req.approvals = [];
  if (!req._approvalSequence) {
    req._approvalSequence = ['Direct Manager','Department Manager','HR','Finance','Travel Coordinator'];
  }
  const approvalRecord = { approverId, level: 'Rejected', decision: 'Rejected', comments, date: new Date().toISOString() };
  req.approvals.push(approvalRecord);
  req.status = 'Rejected';
  req.updatedAt = new Date().toISOString();
  saveRequests(requests);
  try {
    const notifSvc = await import('./notifications');
    notifSvc.addNotification(req.userId, {
      title: 'Request Rejected',
      message: `Your request ${req.id} was rejected by ${approverId}`,
      createdAt: new Date().toISOString(),
    });
  } catch (e) {}
  return req;
}

const MockApi = {
  getUser,
  getRequestsForUser,
  getAllRequests,
  getRequestById,
  createRequest,
  updateRequest,
  updateRequestStatus,
  approveRequest,
  rejectRequest,
};

export default MockApi
