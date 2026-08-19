import React, { useState } from 'react';
import mockApi from '../services/mockApi';
import { useNavigate } from 'react-router-dom';

export default function CreateRequest({ currentUser, onCreated }) {
  const [form, setForm] = useState({
    destination: '',
    purpose: '',
    project: '',
    travelType: 'Domestic',
    departureDate: '',
    returnDate: '',
    preferredAirline: '',
    preferredHotel: '',
    estimatedBudget: ''
  });
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState(null);
  const navigate = useNavigate();

  function updateField(key, value) {
    setForm((f) => ({ ...f, [key]: value }));
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setLoading(true);
    setMessage(null);
    const payload = Object.assign({}, form, { userId: currentUser.id });
    const created = await mockApi.createRequest(payload);
    setLoading(false);
    setMessage('Created request ' + created.id);
    if (onCreated) onCreated(created.id);
    navigate(`/requests/${created.id}`);
  }

  return (
    <div className="container">
      <h2>Create Travel Request</h2>
      <form onSubmit={handleSubmit} style={{ maxWidth: 600 }}>
        <div>
          <label>Destination</label>
          <input value={form.destination} onChange={(e) => updateField('destination', e.target.value)} style={{ width: '100%', padding: 8 }} />
        </div>
        <div>
          <label>Purpose</label>
          <input value={form.purpose} onChange={(e) => updateField('purpose', e.target.value)} style={{ width: '100%', padding: 8 }} />
        </div>
        <div>
          <label>Project</label>
          <input value={form.project} onChange={(e) => updateField('project', e.target.value)} style={{ width: '100%', padding: 8 }} />
        </div>
        <div>
          <label>Departure Date</label>
          <input type="date" value={form.departureDate} onChange={(e) => updateField('departureDate', e.target.value)} style={{ padding: 8 }} />
        </div>
        <div>
          <label>Return Date</label>
          <input type="date" value={form.returnDate} onChange={(e) => updateField('returnDate', e.target.value)} style={{ padding: 8 }} />
        </div>
        <div style={{ marginTop: 8 }}>
          <button type="submit" disabled={loading}>Submit</button>
        </div>
      </form>
      {message && <p style={{ color: 'green' }}>{message}</p>}
    </div>
  );
}
