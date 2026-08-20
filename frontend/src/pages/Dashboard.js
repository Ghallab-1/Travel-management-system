import React, { useEffect, useState } from 'react';
import api from '../services/api';

export default function Dashboard({ currentUser }) {
  const [summary, setSummary] = useState({ total: 0, approved: 0, submitted: 0 });
  const [recent, setRecent] = useState([]);
  const [error, setError] = useState(null);

  useEffect(() => {
    async function load() {
      try {
        const all = await api.getTravelRequests();
        const total = all.length;
        const approved = all.filter((r) => (r.status || '').toLowerCase() === 'approved').length;
        const submitted = all.filter((r) => (r.status || '').toLowerCase() === 'pending').length;
        setSummary({ total, approved, submitted });
        setRecent(all.slice(-6).reverse());
      } catch (e) {
        console.error(e);
        setError('Could not load requests. Is the API running?');
      }
    }
    load();
  }, []);

  return (
    <div className="container">
      <div className="header-row">
        <div>
          <h2>Dashboard</h2>
          <div className="small-muted">Welcome, {currentUser?.fullName}</div>
        </div>
      </div>

      {error && <p style={{ color: 'red' }}>{error}</p>}

      <div className="grid">
        <div className="card">
          <h3>Total Requests</h3>
          <div className="big">{summary.total}</div>
        </div>
        <div className="card">
          <h3>Approved</h3>
          <div className="big">{summary.approved}</div>
        </div>
        <div className="card">
          <h3>Pending</h3>
          <div className="big">{summary.submitted}</div>
        </div>
      </div>

      <div style={{ marginTop: 20 }} className="card">
        <h3>Recent Requests</h3>
        <table>
          <thead>
            <tr><th>ID</th><th>Purpose</th><th>Status</th><th>Departure</th></tr>
          </thead>
          <tbody>
            {recent.map((i) => (
              <tr key={i.travelRequestId}>
                <td>{i.travelRequestId}</td>
                <td>{i.purpose}</td>
                <td><span className={`badge ${(i.status || '').toLowerCase()}`}>{i.status}</span></td>
                <td>{i.departureDate}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}