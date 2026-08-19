import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import { FaInbox, FaPlane, FaHotel, FaFileInvoiceDollar, FaChartLine } from 'react-icons/fa';

export default function Sidebar() {
  const location = useLocation();
  const links = [
    { to: '/approvals', icon: <FaInbox />, label: 'Approvals' },
    { to: '/bookings', icon: <FaPlane />, label: 'Bookings' },
    { to: '/hotels', icon: <FaHotel />, label: 'Hotels' },
    { to: '/expenses', icon: <FaFileInvoiceDollar />, label: 'Expenses' },
    { to: '/reports', icon: <FaChartLine />, label: 'Reports' }
  ];

  return (
    <aside className="sidebar">
            <nav className="sidebar-nav">
              {links.map(l => (
                <Link key={l.to} to={l.to} className={`sidebar-link ${location.pathname === l.to ? 'active' : ''}`} aria-current={location.pathname === l.to ? 'page' : undefined}>
                  <span className="sidebar-icon">{l.icon}</span>
                  <span className="sidebar-label">{l.label}</span>
                </Link>
              ))}
            </nav>
    </aside>
  );
}
