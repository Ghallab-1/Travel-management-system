import React, { useEffect, useState } from 'react';
import api from '../services/api';
import { useParams, useNavigate } from 'react-router-dom';

export default function RequestDetail({ requestId, onBack }) {
  const params = useParams();
  const navigate = useNavigate();
  const id = requestId || params.id;
  const [request, setRequest] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    async function load() {
      setLoading(true);
      setError(null);
      try {
        const r = await api.getRequestById(id);
        setRequest(r);
      } catch (e) {
        console.error(e);
        setError('Could not load this request. Is the API running?');
      }
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
      ) : error ? (
        <p style={{ color: 'red' }}>{error}</p>
      ) : request ? (
        <>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12, marginTop: 12 }}>
            <div><strong>Destination:</strong><div>{request.destinationCityName}</div></div>
            <div><strong>Purpose:</strong><div>{request.purpose}</div></div>
            <div><strong>Project:</strong><div>{request.project}</div></div>
            <div><strong>Travel Type:</strong><div>{request.travelType}</div></div>
            <div><strong>Departure:</strong><div>{request.departureDate}</div></div>
            <div><strong>Return:</strong><div>{request.returnDate}</div></div>
            <div><strong>Estimated Budget:</strong><div>{request.estimatedBudget}</div></div>
            <div><strong>Status:</strong><div><span className={`badge ${(request.status || '').toLowerCase()}`}>{request.status}</span></div></div>
            <div><strong>Current Approval Level:</strong><div>{request.currentApprovalLevel}</div></div>
          </div>

          <div style={{ marginTop: 24 }}>
            <h3>Approval History</h3>
            {request.approvals && request.approvals.length > 0 ? (
              <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                <thead>
                  <tr style={{ textAlign: 'left', borderBottom: '1px solid #ddd' }}>
                    <th>Level</th>
                    <th>Approver</th>
                    <th>Decision</th>
                    <th>Comments</th>
                    <th>Date</th>
                  </tr>
                </thead>
                <tbody>
                  {request.approvals.map((a) => (
                    <tr key={a.travelApprovalId} style={{ borderBottom: '1px solid #f0f0f0' }}>
                      <td>{a.approvalLevel}</td>
                      <td>{a.approverName}</td>
                      <td><span className={`badge ${(a.decision || '').toLowerCase()}`}>{a.decision}</span></td>
                      <td>{a.comments}</td>
                      <td>{a.actionDate ? new Date(a.actionDate).toLocaleString() : ''}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            ) : (
              <p className="small-muted">No approval action taken yet.</p>
            )}
          </div>
        </>
      ) : (
        <p>Request not found.</p>
      )}
    </div>
  );
}