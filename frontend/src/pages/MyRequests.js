import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import mockApi from '../services/mockApi';
import api from '../services/api';

export default function MyRequests({ currentUser }) {
  const [requests, setRequests] = useState([]);
  const [loading, setLoading] = useState(true);
  const navigate = useNavigate();

  useEffect(() => {
    async function load() {
      setLoading(true);

      try {
        // Try the real API first
        const r = await api.getMyRequests(currentUser.id);
        setRequests(r);
      } catch (e) {
        console.error(e);

        // Fallback to mock API if backend is unreachable
        try {
          const r = await mockApi.getRequestsForUser(currentUser.id);
          setRequests(r);
        } catch (mockError) {
          console.error(mockError);
          setRequests([]);
        }
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

      {loading ? (
        <p>Loading...</p>
      ) : (
        <table style={{ width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr
              style={{
                textAlign: 'left',
                borderBottom: '1px solid #ddd'
              }}
            >
              <th>ID</th>
              <th>Destination</th>
              <th>Departure</th>
              <th>Status</th>
              <th></th>
            </tr>
          </thead>

          <tbody>
            {requests.map((r) => {
              const requestId = r.travelRequestId ?? r.id;
              const destination =
                r.destinationCityName ??
                r.destination ??
                (r.destinationCityId != null
                  ? `City ${r.destinationCityId}`
                  : '—');

              const status = r.status ?? 'Unknown';
              const departureDate = r.departureDate ?? '—';

              return (
                <tr
                  key={requestId}
                  style={{
                    borderBottom: '1px solid #f0f0f0'
                  }}
                >
                  <td>{requestId}</td>

                  <td>{destination}</td>

                  <td>{departureDate}</td>

                  <td>
                    <span
                      className={`badge ${String(status).toLowerCase()}`}
                    >
                      {status}
                    </span>
                  </td>

                  <td className="row-actions">
                    <button
                      className="primary"
                      onClick={() =>
                        navigate(`/requests/${requestId}`)
                      }
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