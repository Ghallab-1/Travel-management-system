import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import {
  FaChartLine,
  FaFileInvoiceDollar,
  FaHotel,
  FaInbox,
  FaPlane,
} from 'react-icons/fa';

function roleName(user) {
  return (user?.roleName || user?.role || '').toLowerCase();
}

function buildLinks(user) {
  const role = roleName(user);

  if (role.includes('travel coordinator')) {
    return [
      { to: '/approvals', icon: <FaInbox />, label: 'Coordination' },
      { to: '/bookings', icon: <FaPlane />, label: 'Flights' },
      { to: '/hotels', icon: <FaHotel />, label: 'Hotels' },
      { to: '/expenses', icon: <FaFileInvoiceDollar />, label: 'Expenses' },
      { to: '/reports', icon: <FaChartLine />, label: 'Reports' },
    ];
  }

  if (role === 'hr') {
    return [
      { to: '/approvals', icon: <FaInbox />, label: 'HR Review' },
      { to: '/reports', icon: <FaChartLine />, label: 'Reports' },
    ];
  }

  if (role.includes('manager')) {
    return [
      { to: '/approvals', icon: <FaInbox />, label: 'Approvals' },
      { to: '/reports', icon: <FaChartLine />, label: 'Reports' },
    ];
  }

  if (role.includes('admin')) {
    return [
      { to: '/approvals', icon: <FaInbox />, label: 'Approvals' },
      { to: '/bookings', icon: <FaPlane />, label: 'Flights' },
      { to: '/hotels', icon: <FaHotel />, label: 'Hotels' },
      { to: '/expenses', icon: <FaFileInvoiceDollar />, label: 'Expenses' },
      { to: '/reports', icon: <FaChartLine />, label: 'Reports' },
    ];
  }

  return [];
}

export default function Sidebar({ currentUser }) {
  const location = useLocation();
  const links = buildLinks(currentUser);

  return (
    <aside className="sidebar">
      <nav className="sidebar-nav">
        {links.map((link) => (
          <Link
            key={link.to}
            to={link.to}
            className={`sidebar-link ${location.pathname === link.to ? 'active' : ''}`}
            aria-current={location.pathname === link.to ? 'page' : undefined}
          >
            <span className="sidebar-icon">{link.icon}</span>
            <span className="sidebar-label">{link.label}</span>
          </Link>
        ))}
      </nav>
    </aside>
  );
}
