import React, { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import api from '../services/api';

function getCurrentUser() {
  try {
    const raw = window.localStorage.getItem('tms_user');
    return raw ? JSON.parse(raw) : null;
  } catch (e) {
    return null;
  }
}

function getRole(user) {
  return (user?.roleName || user?.role || '').toLowerCase();
}

function isApproved(request) {
  return String(request?.status || '').toLowerCase() === 'approved';
}

function formatMoney(value) {
  const amount = Number(value || 0);
  return amount > 0 ? amount.toFixed(2) : 'Pending coordinator estimate';
}

function tripDays(request) {
  if (!request?.departureDate || !request?.returnDate) return 1;

  const start = new Date(request.departureDate);
  const end = new Date(request.returnDate);
  const diff = Math.ceil((end - start) / (1000 * 60 * 60 * 24)) + 1;

  return Number.isFinite(diff) && diff > 0 ? diff : 1;
}

function suggestedPerDiem(request) {
  const role = String(request?.userRole || '').toLowerCase();
  const days = tripDays(request);
  let dailyRate = 60;

  if (role.includes('department manager')) dailyRate = 110;
  else if (role.includes('direct manager')) dailyRate = 90;
  else if (role.includes('manager')) dailyRate = 85;

  return dailyRate * days;
}

export default function RequestDetail({ requestId }) {
  const params = useParams();
  const navigate = useNavigate();
  const id = requestId || params.id;

  const [request, setRequest] = useState(null);
  const [budgetForm, setBudgetForm] = useState({
    estimatedBudget: '',
    coordinatorNotes: '',
  });
  const [perDiemForm, setPerDiemForm] = useState({
    perDiemAmount: '',
    comments: '',
  });
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [message, setMessage] = useState(null);

  const currentUser = getCurrentUser();
  const role = getRole(currentUser);
  const canCoordinate = role.includes('travel coordinator') || role.includes('admin');
  const canReviewPerDiem = role === 'hr' || role.includes('admin');

  async function load() {
    setLoading(true);
    setError(null);

    try {
      const result = await api.getRequestById(id);
      setRequest(result);
      setBudgetForm({
        estimatedBudget: result.estimatedBudget > 0 ? String(result.estimatedBudget) : '',
        coordinatorNotes: result.coordinatorNotes || '',
      });
      setPerDiemForm({
        perDiemAmount: result.perDiemAmount > 0
          ? String(result.perDiemAmount)
          : String(suggestedPerDiem(result)),
        comments: result.perDiemComments || '',
      });
    } catch (e) {
      console.error(e);
      setError('Could not load this request. Is the API running?');
    }

    setLoading(false);
  }

  useEffect(() => {
    if (id) {
      load();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  const rejection = useMemo(() => {
    const approvals = request?.approvals || [];
    return approvals
      .slice()
      .reverse()
      .find((approval) => String(approval.decision || '').toLowerCase() === 'rejected');
  }, [request]);

  async function handleCoordinatorSave(e) {
    e.preventDefault();
    setSaving(true);
    setError(null);
    setMessage(null);

    try {
      const updated = await api.updateCoordinatorDetails(id, {
        estimatedBudget: parseFloat(budgetForm.estimatedBudget) || 0,
        coordinatorId: currentUser.id,
        coordinatorNotes: budgetForm.coordinatorNotes,
      });

      setRequest(updated);
      setMessage('Coordinator details saved.');
    } catch (e) {
      console.error(e);
      setError(e.message || 'Could not save coordinator details.');
    }

    setSaving(false);
  }

  async function handlePerDiem(decision) {
    setSaving(true);
    setError(null);
    setMessage(null);

    try {
      const updated = await api.updatePerDiem(id, {
        hrUserId: currentUser.id,
        perDiemAmount: parseFloat(perDiemForm.perDiemAmount) || 0,
        decision,
        comments: perDiemForm.comments,
      });

      setRequest(updated);
      setMessage(`Per diem ${decision.toLowerCase()}.`);
    } catch (e) {
      console.error(e);
      setError(e.message || 'Could not update per diem.');
    }

    setSaving(false);
  }

  if (!id) {
    return <div style={{ padding: 16 }}>No request selected.</div>;
  }

  return (
    <div className="container">
      <button onClick={() => navigate(-1)}>Back</button>

      {loading ? (
        <p>Loading...</p>
      ) : error && !request ? (
        <p style={{ color: 'red' }}>{error}</p>
      ) : request ? (
        <>
          {error && <p style={{ color: 'red' }}>{error}</p>}
          {message && <p style={{ color: 'green' }}>{message}</p>}

          <div
            style={{
              display: 'grid',
              gridTemplateColumns: '1fr 1fr',
              gap: 12,
              marginTop: 12,
            }}
          >
            <div>
              <strong>User:</strong>
              <div>{request.userName || 'User missing from API'}</div>
            </div>

            <div>
              <strong>User Role:</strong>
              <div>{request.userRole || 'Role missing from API'}</div>
            </div>

            <div>
              <strong>Destination:</strong>
              <div>
                {request.destinationCityName}
                {request.destinationCountryName ? `, ${request.destinationCountryName}` : ''}
              </div>
            </div>

            <div>
              <strong>Purpose:</strong>
              <div>{request.purpose || '-'}</div>
            </div>

            <div>
              <strong>Project:</strong>
              <div>{request.project || '-'}</div>
            </div>

            <div>
              <strong>Travel Type:</strong>
              <div>{request.travelType || '-'}</div>
            </div>

            <div>
              <strong>Departure:</strong>
              <div>{request.departureDate || '-'}</div>
            </div>

            <div>
              <strong>Return:</strong>
              <div>{request.returnDate || '-'}</div>
            </div>

            <div>
              <strong>Estimated Budget:</strong>
              <div>{formatMoney(request.estimatedBudget)}</div>
              {request.estimatedBudgetSetByName && (
                <div className="small-muted">
                  Set by {request.estimatedBudgetSetByName}
                </div>
              )}
            </div>

            <div>
              <strong>Status:</strong>
              <div>
                <span className={`badge ${(request.status || '').toLowerCase()}`}>
                  {request.status || '-'}
                </span>
              </div>
            </div>

            <div>
              <strong>Current Approval Level:</strong>
              <div>{request.currentApprovalRole || '-'}</div>
            </div>

            <div>
              <strong>Per Diem:</strong>
              <div>
                {request.perDiemStatus || 'Not Submitted'}
                {request.perDiemAmount > 0 ? ` - ${Number(request.perDiemAmount).toFixed(2)}` : ''}
              </div>
            </div>
          </div>

          {rejection && (
            <div className="card" style={{ marginTop: 20 }}>
              <h3>Rejection Reason</h3>
              <p>{rejection.comments && rejection.comments.trim() ? rejection.comments : '-'}</p>
            </div>
          )}

          <div className="card" style={{ marginTop: 20 }}>
            <h3>Travel Documents</h3>
            <p>{request.requiredDocumentNotes || '-'}</p>
            {request.hasRequiredDocumentPdf && (
              <a href={api.getTravelDocumentUrl(request.travelRequestId)} target="_blank" rel="noreferrer">
                {request.requiredDocumentFileName || 'Open required documents PDF'}
              </a>
            )}
          </div>

          {canCoordinate && isApproved(request) && (
            <form className="card" style={{ marginTop: 20 }} onSubmit={handleCoordinatorSave}>
              <h3>Coordinator Details</h3>

              <label>Estimated Budget</label>
              <input
                type="number"
                min="0"
                step="0.01"
                value={budgetForm.estimatedBudget}
                onChange={(e) =>
                  setBudgetForm((current) => ({
                    ...current,
                    estimatedBudget: e.target.value,
                  }))
                }
                style={{ padding: 8 }}
                required
              />

              <label>Coordinator Notes</label>
              <textarea
                rows={3}
                value={budgetForm.coordinatorNotes}
                onChange={(e) =>
                  setBudgetForm((current) => ({
                    ...current,
                    coordinatorNotes: e.target.value,
                  }))
                }
                style={{ width: '100%', padding: 8 }}
              />

              <button type="submit" className="primary" disabled={saving}>
                Save Coordinator Details
              </button>
            </form>
          )}

          {canReviewPerDiem && isApproved(request) && (
            <div className="card" style={{ marginTop: 20 }}>
              <h3>HR Per Diem</h3>
              <p className="small-muted">
                Suggested from requester role and trip length: {suggestedPerDiem(request).toFixed(2)}
              </p>

              <label>Per Diem Amount</label>
              <input
                type="number"
                min="0"
                step="0.01"
                value={perDiemForm.perDiemAmount}
                onChange={(e) =>
                  setPerDiemForm((current) => ({
                    ...current,
                    perDiemAmount: e.target.value,
                  }))
                }
                style={{ padding: 8 }}
              />

              <label>HR Comments</label>
              <textarea
                rows={3}
                value={perDiemForm.comments}
                onChange={(e) =>
                  setPerDiemForm((current) => ({
                    ...current,
                    comments: e.target.value,
                  }))
                }
                style={{ width: '100%', padding: 8 }}
              />

              <button
                className="primary"
                disabled={saving}
                onClick={() => handlePerDiem('Approved')}
                style={{ marginRight: 8 }}
              >
                Approve Per Diem
              </button>

              <button disabled={saving} onClick={() => handlePerDiem('Rejected')}>
                Reject Per Diem
              </button>
            </div>
          )}

          <div style={{ marginTop: 24 }}>
            <h3>Approval History</h3>

            {request.approvals && request.approvals.length > 0 ? (
              <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                <thead>
                  <tr style={{ textAlign: 'left', borderBottom: '1px solid #ddd' }}>
                    <th>Level</th>
                    <th>Approver</th>
                    <th>Approver Role</th>
                    <th>Decision</th>
                    <th>Comments</th>
                    <th>Date</th>
                  </tr>
                </thead>
                <tbody>
                  {request.approvals.map((approval) => (
                    <tr key={approval.travelApprovalId} style={{ borderBottom: '1px solid #f0f0f0' }}>
                      <td>{approval.approvalLevel || '-'}</td>
                      <td>{approval.approverName || '-'}</td>
                      <td>{approval.approverRole || '-'}</td>
                      <td>
                        <span className={`badge ${(approval.decision || '').toLowerCase()}`}>
                          {approval.decision || '-'}
                        </span>
                      </td>
                      <td>
                        {approval.comments && approval.comments.trim() ? approval.comments : '-'}
                      </td>
                      <td>
                        {approval.actionDate
                          ? new Date(approval.actionDate).toLocaleString()
                          : '-'}
                      </td>
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
