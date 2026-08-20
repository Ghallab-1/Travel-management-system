import React, { useEffect, useState } from 'react';
import api from '../services/api';
import { useNavigate } from 'react-router-dom';

export default function CreateRequest({ currentUser, onCreated }) {
  const [form, setForm] = useState({
    destinationCityId: '',
    purpose: '',
    project: '',
    travelType: 'Domestic',
    departureDate: '',
    returnDate: '',
    estimatedBudget: ''
  });
  const [cities, setCities] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [message, setMessage] = useState(null);
  const navigate = useNavigate();

  useEffect(() => {
    api.getCities()
      .then((c) => {
        setCities(c);
        if (c.length > 0) updateField('destinationCityId', c[0].id);
      })
      .catch(() => setError('Could not load destination cities. Is the API running?'));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  function updateField(key, value) {
    setForm((f) => ({ ...f, [key]: value }));
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setLoading(true);
    setMessage(null);
    setError(null);

    const payload = {
      userId: currentUser.id,
      departmentId: currentUser.departmentId,
      travelPolicyId: 1, // Default Policy, seeded by DbSeeder
      destinationCityId: parseInt(form.destinationCityId, 10),
      purpose: form.purpose,
      project: form.project,
      travelType: form.travelType,
      departureDate: form.departureDate,
      returnDate: form.returnDate,
      estimatedBudget: parseFloat(form.estimatedBudget) || 0
    };

    try {
      const created = await api.createRequest(payload);
      setMessage('Request submitted successfully.');
      if (onCreated) onCreated(created.travelRequestId);
      navigate(`/requests/${created.travelRequestId}`);
    } catch (err) {
      console.error(err);
      setError('Failed to submit request. Check console for details.');
    }
    setLoading(false);
  }

  return (
    <div className="container">
      <h2>Create Travel Request</h2>
      {error && <p style={{ color: 'red' }}>{error}</p>}
      <form onSubmit={handleSubmit} style={{ maxWidth: 600 }}>
        <div>
          <label>Destination</label>
          <select
            value={form.destinationCityId}
            onChange={(e) => updateField('destinationCityId', e.target.value)}
            style={{ width: '100%', padding: 8 }}
          >
            {cities.map((c) => (
              <option key={c.id} value={c.id}>{c.name}, {c.country}</option>
            ))}
          </select>
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
          <label>Travel Type</label>
          <select value={form.travelType} onChange={(e) => updateField('travelType', e.target.value)} style={{ width: '100%', padding: 8 }}>
            <option value="Domestic">Domestic</option>
            <option value="International">International</option>
          </select>
        </div>
        <div>
          <label>Departure Date</label>
          <input type="date" value={form.departureDate} onChange={(e) => updateField('departureDate', e.target.value)} style={{ padding: 8 }} />
        </div>
        <div>
          <label>Return Date</label>
          <input type="date" value={form.returnDate} onChange={(e) => updateField('returnDate', e.target.value)} style={{ padding: 8 }} />
        </div>
        <div>
          <label>Estimated Budget</label>
          <input type="number" value={form.estimatedBudget} onChange={(e) => updateField('estimatedBudget', e.target.value)} style={{ padding: 8 }} />
        </div>
        <div style={{ marginTop: 8 }}>
          <button type="submit" disabled={loading || cities.length === 0}>Submit</button>
        </div>
      </form>
      {message && <p style={{ color: 'green' }}>{message}</p>}
    </div>
  );
}