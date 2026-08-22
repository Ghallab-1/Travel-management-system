import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { FaBell } from 'react-icons/fa';
import logo from '../assets/logo.svg';
import Api from '../services/api';

function initials(name) {
  if (!name) return '';
  return name
    .split(' ')
    .map((part) => part[0])
    .slice(0, 2)
    .join('')
    .toUpperCase();
}

function getRole(user) {
  return (user?.roleName || user?.role || '').toLowerCase();
}

function canCreateRequests(user) {
  const role = getRole(user);
  return (
    role.includes('employee') ||
    role.includes('manager') ||
    role.includes('admin')
  );
}

function canSeeMyRequests(user) {
  const role = getRole(user);
  return (
    role.includes('employee') ||
    role.includes('manager') ||
    role.includes('admin')
  );
}

export default function NavBar({ currentUser, onLogout }) {
  const navigate = useNavigate();
  const [unread, setUnread] = React.useState(0);

  React.useEffect(() => {
    let cancelled = false;

    async function loadUnread() {
      if (!currentUser?.id) {
        setUnread(0);
        return;
      }

      try {
        const items = await Api.getNotifications(currentUser.id);
        if (!cancelled) {
          setUnread((items || []).filter((item) => !item.isRead && !item.IsRead).length);
        }
      } catch (e) {
        if (!cancelled) {
          setUnread(0);
        }
      }
    }

    loadUnread();
    const timer = setInterval(loadUnread, 10000);

    return () => {
      cancelled = true;
      clearInterval(timer);
    };
  }, [currentUser?.id]);

  function handleLogout() {
    if (onLogout) {
      onLogout();
    } else {
      navigate('/login');
      window.location.reload();
    }
  }

  const isAdmin = getRole(currentUser).includes('admin');

  return (
    <nav>
      <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
        <img src={logo} alt="logo" style={{ height: 28 }} />
        <div className="logo">Travel Management</div>
      </div>

      <Link to="/">Dashboard</Link>

      {canCreateRequests(currentUser) && (
        <Link to="/create">Create Request</Link>
      )}

      {canSeeMyRequests(currentUser) && (
        <Link to="/myrequests">My Requests</Link>
      )}

      {isAdmin && (
        <Link to="/admin/departments">Admin</Link>
      )}

      <div className="nav-right">
        {currentUser ? (
          <>
            <button
              onClick={() => navigate('/notifications')}
              style={{
                position: 'relative',
                background: 'transparent',
                border: 'none',
                cursor: 'pointer',
                color: 'var(--primary)',
              }}
              title="Notifications"
            >
              <FaBell />
              {unread > 0 && (
                <span
                  style={{
                    position: 'absolute',
                    top: -6,
                    right: -6,
                    background: '#e05a4f',
                    color: '#fff',
                    borderRadius: 8,
                    padding: '2px 6px',
                    fontSize: 10,
                  }}
                >
                  {unread}
                </span>
              )}
            </button>

            <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              <div
                style={{
                  width: 32,
                  height: 32,
                  borderRadius: 16,
                  background: '#eef3fb',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  color: 'var(--primary)',
                  fontWeight: 700,
                }}
              >
                {initials(currentUser.fullName)}
              </div>

              <div>
                <div className="small-muted">Signed in as</div>
                <strong>{currentUser.fullName}</strong>
              </div>
            </div>

            <button onClick={handleLogout} className="primary" style={{ marginLeft: 8 }}>
              Logout
            </button>
          </>
        ) : (
          <Link to="/login">Login</Link>
        )}
      </div>
    </nav>
  );
}
