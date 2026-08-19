import React, { useEffect, useState } from 'react';
import mockApi from '../services/mockApi';

export default function Dashboard({ currentUser }) {
  const [summary, setSummary] = useState({ total: 0, approved: 0, submitted: 0 });

  useEffect(() => {
    async function load() {
      const all = await mockApi.getAllRequests();
      const total = all.length;
      const approved = all.filter((r) => r.status === 'Approved').length;
      const submitted = all.filter((r) => r.status === 'Submitted').length;
      setSummary({ total, approved, submitted });
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
          <h3>Submitted</h3>
          <div className="big">{summary.submitted}</div>
        </div>
      </div>

      <div style={{ marginTop: 20 }} className="card">
        <h3>Recent Requests</h3>
        <RecentRequests />
      </div>
    </div>
  );
}

function RecentRequests(){
  const [items, setItems] = React.useState([]);
  React.useEffect(() => { mockApi.getAllRequests().then(r => setItems(r.slice(0,6))) }, []);
  return (
    <table>
      <thead>
        <tr><th>ID</th><th>Destination</th><th>Status</th><th>Departure</th></tr>
      </thead>
      <tbody>
        {items.map(i => (
          <tr key={i.id}>
            <td>{i.id}</td>
            <td>{i.destination}</td>
            <td><span className={`badge ${i.status.toLowerCase()}`}>{i.status}</span></td>
            <td>{i.departureDate}</td>
          </tr>
        ))}
      </tbody>
    </table>
  )
}
