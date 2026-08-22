import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../services/api';

export default function MyRequests({ currentUser }) {
  const [requests, setRequests] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const navigate = useNavigate();

  useEffect(() => {
    async function load() {
      setLoading(true);
      setError(null);

      try {
        const result = await api.getMyRequests(currentUser.id);
        const sorted = (result || [])
          .slice()
          .sort(
            (a, b) =>
              Number(a.travelRequestId ?? a.id ?? 0) -
              Number(b.travelRequestId ?? b.id ?? 0)
          );

        setRequests(sorted);
      } catch (e) {
        console.error(e);
        setError('Could not load your requests from the backend.');
        setRequests([]);
      }

      setLoading(false);
    }

    if (currentUser) {
      load();
    }
  }, [currentUser]);

  return (
    <div className="container">
      <h2>My Requests</h2>

      {error && <p style={{ color: 'red' }}>{error}</p>}

      {loading ? (
        <p>Loading...</p>
      ) : (
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr style={{ textAlign: 'left', borderBottom: '1px solid #ddd' }}>
              <th>ID</th>
              <th>Destination</th>
              <th>Departure</th>
              <th>Status</th>
              <th></th>
            </tr>
          </thead>

          <tbody>
            {requests.map((request, index) => {
              const requestId = request.travelRequestId ?? request.id;
              const destination = request.destinationCityName || 'Destination missing from API';
              const status = request.status || 'Unknown';
              const departureDate = request.departureDate || '';

              return (
                <tr key={requestId} style={{ borderBottom: '1px solid #f0f0f0' }}>
                  <td>{index + 1}</td>
                  <td>{destination}</td>
                  <td>{departureDate}</td>
                  <td>
                    <span className={`badge ${String(status).toLowerCase()}`}>
                      {status}
                    </span>
                  </td>
                  <td className="row-actions">
                    <button
                      className="primary"
                      onClick={() => navigate(`/requests/${requestId}`)}
                    >
                      Open
                    </button>
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      )}
    </div>
  );
}
