import React from 'react';
import { useNavigate } from 'react-router-dom';
import Api from '../services/api';

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

function getApproverId(user) {
  return user?.id || user?.userId || user?.UserId || null;
}

function missing(label) {
  return <span style={{ color: '#b3261e' }}>{label} missing from API</span>;
}

export default function Approvals() {
  const [items, setItems] = React.useState([]);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState(null);
  const navigate = useNavigate();

  const currentUser = getCurrentUser();
  const role = getRole(currentUser);
  const isCoordinator = role.includes('travel coordinator');
  const isHr = role === 'hr';
  const isManager = role.includes('manager');

  React.useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  async function load() {
    setLoading(true);
    setError(null);

    try {
      let data;

      if (isCoordinator) {
        data = await Api.getCoordinatorWork();
      } else if (isHr) {
        data = await Api.getHrReviewRequests();
      } else {
        data = await Api.getPendingForMe();
      }

      setItems(data || []);
    } catch (e) {
      console.error(e);
      setError('Failed to load this work queue. Ensure the API is running and your role is correct.');
    }

    setLoading(false);
  }

  function getOptionalComment(decision) {
    const comment = window.prompt(
      `${decision} this request.\n\nOptional comment (leave empty if you do not want to add one):`
    );

    return comment === null ? '' : comment.trim();
  }

  async function handleApprove(id) {
    setError(null);

    const approverId = getApproverId(currentUser);
    if (!approverId) {
      setError('You must be logged in as an approver to perform this action.');
      return;
    }

    try {
      await Api.approveRequest(id, approverId, getOptionalComment('Approve'), '');
      await load();
    } catch (e) {
      console.error(e);
      setError(e.message || 'Failed to approve request.');
    }
  }

  async function handleReject(id) {
    setError(null);

    const approverId = getApproverId(currentUser);
    if (!approverId) {
      setError('You must be logged in as an approver to perform this action.');
      return;
    }

    try {
      await Api.rejectRequest(id, approverId, getOptionalComment('Reject'), '');
      await load();
    } catch (e) {
      console.error(e);
      setError(e.message || 'Failed to reject request.');
    }
  }

  const title = isCoordinator ? 'Coordination' : isHr ? 'HR Review' : 'Approvals';
  const subtitle = isCoordinator
    ? 'Manager decisions and requests ready for bookings, hotels, and expenses'
    : isHr
      ? 'Manager decisions and per diem review'
      : 'Requests awaiting your approval';

  return (
    <div className="container">
      <h2>{title}</h2>
      <p className="small-muted">{subtitle}</p>

      {error && <p style={{ color: 'red' }}>{error}</p>}

      {loading ? (
        <p>Loading...</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>User</th>
              <th>Role</th>
              <th>Purpose</th>
              <th>Status</th>
              <th></th>
            </tr>
          </thead>

          <tbody>
            {items
              .slice()
              .sort(
                (a, b) =>
                  Number(a.travelRequestId || a.TravelRequestId || a.id || 0) -
                  Number(b.travelRequestId || b.TravelRequestId || b.id || 0)
              )
              .map((item, index) => {
                const requestId = item.travelRequestId || item.TravelRequestId || item.id;
                const status = String(item.status || '').toLowerCase();
                const canApprove = isManager && status === 'pending';

                return (
                  <tr key={requestId}>
                    <td>{index + 1}</td>
                    <td>{item.userName || item.UserName || missing('User')}</td>
                    <td>{item.userRole || item.UserRole || missing('Role')}</td>
                    <td>{item.purpose || item.Purpose || ''}</td>
                    <td>
                      <span className={`badge ${status}`}>{item.status || ''}</span>
                    </td>
                    <td>
                      {canApprove ? (
                        <>
                          <button
                            className="primary"
                            onClick={() => handleApprove(requestId)}
                            style={{ marginRight: 8 }}
                          >
                            Approve
                          </button>

                          <button
                            onClick={() => handleReject(requestId)}
                            style={{
                              background: '#eee',
                              border: '1px solid #ddd',
                              padding: '6px 8px',
                            }}
                          >
                            Reject
                          </button>
                        </>
                      ) : (
                        <button className="primary" onClick={() => navigate(`/requests/${requestId}`)}>
                          Open
                        </button>
                      )}
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
