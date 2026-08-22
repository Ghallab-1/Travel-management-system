import React from 'react';
import Api from '../services/api';

export default function Notifications({ currentUser }) {
  const [items, setItems] = React.useState([]);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState(null);

  React.useEffect(() => {
    if (currentUser?.id) {
      load(currentUser.id);
    } else {
      setLoading(false);
    }
  }, [currentUser?.id]);

  async function load(userId) {
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

  async function markRead(id) {
    try {
      await Api.markNotificationRead(id);
      if (currentUser?.id) {
        await load(currentUser.id);
      }
    } catch (e) {
      console.error(e);
      setError('Failed to mark read.');
    }
  }

  return (
    <div className="container">
      <h2>Notifications</h2>
      <p className="small-muted">Recent system notifications</p>

      {error && <p style={{ color: 'red' }}>{error}</p>}

      {loading ? (
        <p>Loading...</p>
      ) : items.length === 0 ? (
        <p>No notifications</p>
      ) : (
        <ul>
          {items.map((notification) => {
            const id = notification.notificationId || notification.NotificationId || notification.id;
            const isRead = notification.isRead || notification.IsRead;
            const created = notification.createdDate || notification.CreatedDate;

            return (
              <li key={id} style={{ padding: 8, borderBottom: '1px solid #eee' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', gap: 12 }}>
                  <div>
                    <strong>{notification.title || notification.Title}</strong>
                    <div className="small-muted">{notification.message || notification.Message}</div>
                  </div>

                  <div>
                    <div className="small-muted">
                      {created ? new Date(created).toLocaleString() : ''}
                    </div>
                    {!isRead && (
                      <button className="primary" onClick={() => markRead(id)} style={{ marginTop: 8 }}>
                        Mark read
                      </button>
                    )}
                  </div>
                </div>
              </li>
            );
          })}
        </ul>
      )}
    </div>
  );
}
