// Simple notifications service persisted in localStorage
const STORAGE_KEY = 'tms_notifications_v1';

function load() {
  try { const raw = window.localStorage.getItem(STORAGE_KEY); return raw ? JSON.parse(raw) : []; } catch(e) { return []; }
}

function save(items) {
  try { window.localStorage.setItem(STORAGE_KEY, JSON.stringify(items)); } catch(e) {}
}

export function getNotifications(userId) {
  const all = load();
  return all.filter(n => n.userId === userId).sort((a,b)=> new Date(b.createdAt)-new Date(a.createdAt));
}

export function getUnreadCount(userId) {
  return getNotifications(userId).filter(n=>!n.read).length;
}

export function addNotification(userId, payload) {
  const all = load();
  const id = 'n-' + (all.length + 1) + '-' + Date.now();
  const notif = Object.assign({ id, userId, read: false }, payload);
  all.push(notif);
  save(all);
  return notif;
}

export function markRead(id) {
  const all = load();
  const i = all.findIndex(x=>x.id===id);
  if (i===-1) return null;
  all[i].read = true;
  save(all);
  return all[i];
}

const Notifications = { getNotifications, getUnreadCount, addNotification, markRead };
export default Notifications
