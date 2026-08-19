import React, { useEffect, useState } from 'react';
import mockApi from '../services/mockApi';
import { useParams, useNavigate } from 'react-router-dom';

export default function RequestDetail({ requestId, onBack }) {
  const params = useParams();
  const navigate = useNavigate();
  const id = requestId || params.id;
  const [request, setRequest] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function load() {
      setLoading(true);
      const r = await mockApi.getRequestById(id);
      setRequest(r);
      setLoading(false);
    }
    if (id) load();
  }, [id]);

  if (!id) return <div style={{ padding: 16 }}>No request selected.</div>;

  return (
    <div className="container">
      <button onClick={() => navigate(-1)}>Back</button>
      {loading ? (
        <p>Loading...</p>
      ) : request ? (
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
          <div><strong>Destination:</strong><div>{request.destination}</div></div>
          <div><strong>Purpose:</strong><div>{request.purpose}</div></div>
          <div><strong>Project:</strong><div>{request.project}</div></div>
          <div><strong>Travel Type:</strong><div>{request.travelType}</div></div>
          <div><strong>Departure:</strong><div>{request.departureDate}</div></div>
          <div><strong>Return:</strong><div>{request.returnDate}</div></div>
          <div><strong>Preferred Airline:</strong><div>{request.preferredAirline}</div></div>
          <div><strong>Preferred Hotel:</strong><div>{request.preferredHotel}</div></div>
          <div><strong>Estimated Budget:</strong><div>{request.estimatedBudget}</div></div>
          <div><strong>Status:</strong><div><span className={`badge ${request.status.toLowerCase()}`}>{request.status}</span></div></div>
        </div>
      ) : (
        <p>Request not found.</p>
      )}
    </div>
  );
}
