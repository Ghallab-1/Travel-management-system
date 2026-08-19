import React from 'react';
import Api from '../services/api';

export default function Notifications(){
  const [items, setItems] = React.useState([]);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState(null);

  React.useEffect(() => {
    const userId = (function(){ try { const raw = window.localStorage.getItem('tms_user'); return raw? JSON.parse(raw).id: null } catch(e){return null} })();
    if (userId) load(userId);
  }, []);

  async function load(userId){
    setLoading(true);
    setError(null);
    try {
      const list = await Api.getNotifications(userId);
      setItems(list || []);
    } catch (e) {
      console.error(e);
      setError('Failed to load notifications. Ensure the API is running.');
    }
    setLoading(false);
  }

  async function markRead(id){
    try {
      if (typeof Api.markNotificationRead === 'function') {
        await Api.markNotificationRead(id); // backend has POST /api/notifications/markread/{id}
      } else {
        console.warn('No markNotificationRead available on Api client');
      }
      const userId = (function(){ try { const raw = window.localStorage.getItem('tms_user'); return raw? JSON.parse(raw).id: null } catch(e){return null} })();
      if (userId) load(userId);
    } catch (e) {
      console.error(e);
      setError('Failed to mark read');
    }
  }

  return (
    <div className="container">
      <h2>Notifications</h2>
      <p className="small-muted">Recent system notifications</p>
      {error && <p style={{color:'red'}}>{error}</p>}
      {loading ? <p>Loading...</p> : (
        items.length===0 ? <p>No notifications</p> : (
        <ul>
          {items.map(n => (
            <li key={n.notificationId || n.NotificationId || n.id} style={{padding:8, borderBottom:'1px solid #eee'}}>
              <div style={{display:'flex',justifyContent:'space-between'}}>
                <div>
                  <strong>{n.title}</strong>
                  <div className="small-muted">{n.message}</div>
                </div>
                <div>
                  <div className="small-muted">{new Date(n.createdAt).toLocaleString()}</div>
                  {!n.isRead && !n.read && <button className="primary" onClick={() => markRead(n.notificationId || n.NotificationId || n.id)} style={{marginTop:8}}>Mark read</button>}
                </div>
              </div>
            </li>
          ))}
        </ul>))}
    </div>
  )
}
