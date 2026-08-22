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
        const r = await api.getMyRequests(currentUser.id);

        const sorted = (r || [])
          .slice()
          .sort((a, b) => {
            const aId = Number(a.travelRequestId ?? a.id ?? 0);
            const bId = Number(b.travelRequestId ?? b.id ?? 0);

            return aId - bId;
          });

        setRequests(sorted);
      } catch (e) {
        console.error(e);

        try {
          const r = await mockApi.getRequestsForUser(
            currentUser.id
          );

          const sorted = (r || [])
            .slice()
            .sort((a, b) => {
              const aId = Number(a.travelRequestId ?? a.id ?? 0);
              const bId = Number(b.travelRequestId ?? b.id ?? 0);

              return aId - bId;
            });

          setRequests(sorted);
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
        <table
          style={{
            width: '100%',
            borderCollapse: 'collapse'
          }}
        >
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
            {requests.map((r, index) => {
              const requestId =
                r.travelRequestId ?? r.id;

              const destination =
                r.destinationCityName ??
                r.destination ??
                (r.destinationCityId != null
                  ? `City ${r.destinationCityId}`
                  : '—');

              const status =
                r.status ?? 'Unknown';

              const departureDate =
                r.departureDate ?? '—';

              /*
               * Display ID is always sequential:
               * 1, 2, 3, 4...
               *
               * requestId remains the real database ID.
               */
              const displayId = index + 1;

              return (
                <tr
                  key={requestId}
                  style={{
                    borderBottom: '1px solid #f0f0f0'
                  }}
                >
                  <td>{displayId}</td>

                  <td>{destination}</td>

                  <td>{departureDate}</td>

                  <td>
                    <span
                      className={`badge ${String(
                        status
                      ).toLowerCase()}`}
                    >
                      {status}
                    </span>
                  </td>

                  <td className="row-actions">
                    <button
                      className="primary"
                      onClick={() =>
                        navigate(
                          `/requests/${requestId}`
                        )
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