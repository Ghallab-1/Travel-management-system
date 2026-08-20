import React from 'react';
import Api from '../services/api';

export default function Approvals() {
  const [items, setItems] = React.useState([]);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState(null);

  React.useEffect(() => {
    load();
  }, []);

  async function load() {
    setLoading(true);
    setError(null);

    try {
      let r = [];

      try {
        r = await Api.getPendingForMe();
      } catch (e) {
        r = await Api.getTravelRequests();
      }

      setItems(r || []);
    } catch (e) {
      console.error(e);

      setError(
        'Failed to load requests. Ensure the API is running and you are logged in.'
      );
    }

    setLoading(false);
  }

  function getCurrentUser() {
    try {
      const raw = window.localStorage.getItem('tms_user');

      if (!raw) return null;

      return JSON.parse(raw);
    } catch (e) {
      return null;
    }
  }

  function getApproverId() {
    const u = getCurrentUser();

    if (!u) return null;

    return u.id || u.userId || u.Id || null;
  }

  function isApproverRole() {
    const u = getCurrentUser();

    if (!u) return false;

    const role = (
      u.role ||
      u.roleName ||
      ''
    ).toString().toLowerCase();

    return (
      role.includes('approver') ||
      role.includes('manager') ||
      role.includes('admin')
    );
  }

  async function handleApprove(id) {
    setError(null);

    const approverId = getApproverId();

    if (!approverId) {
      setError(
        'You must be logged in as an approver to perform this action.'
      );
      return;
    }

    try {
      await Api.approveRequest(
        id,
        approverId,
        'Approved from UI',
        'Direct Manager'
      );

      await load();
    } catch (e) {
      console.error(e);

      setError(
        'Failed to approve request. Check console for details.'
      );
    }
  }

  async function handleReject(id) {
    setError(null);

    const approverId = getApproverId();

    if (!approverId) {
      setError(
        'You must be logged in as an approver to perform this action.'
      );
      return;
    }

    try {
      await Api.rejectRequest(
        id,
        approverId,
        'Rejected from UI',
        'Direct Manager'
      );

      await load();
    } catch (e) {
      console.error(e);

      setError(
        'Failed to reject request. Check console for details.'
      );
    }
  }

  return (
    <div className="container">
      <h2>Approvals</h2>

      <p className="small-muted">
        Requests awaiting approval
      </p>

      {error && (
        <p style={{ color: 'red' }}>
          {error}
        </p>
      )}

      {loading ? (
        <p>Loading...</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Purpose</th>
              <th>User</th>
              <th>Role</th>
              <th>Status</th>
              <th></th>
            </tr>
          </thead>

          <tbody>
            {items.map(i => (
              <tr
                key={
                  i.travelRequestId ||
                  i.TravelRequestId ||
                  i.id
                }
              >
                {/* ID */}
                <td>
                  {i.travelRequestId ||
                    i.TravelRequestId ||
                    i.id}
                </td>

                {/* Purpose */}
                <td>
                  {i.purpose ||
                    i.Purpose ||
                    i.destination ||
                    '-'}
                </td>

                {/* User Name */}
                <td>
                  {i.userName ||
                    i.UserName ||
                    i.fullName ||
                    i.FullName ||
                    '-'}
                </td>

                {/* User Role */}
                <td>
                  {i.roleName ||
                    i.RoleName ||
                    i.userRole ||
                    i.UserRole ||
                    i.role ||
                    i.Role ||
                    '-'}
                </td>

                {/* Status */}
                <td>
                  <span
                    className={`badge ${(i.status || '').toLowerCase()}`}
                  >
                    {i.status || 'Draft'}
                  </span>
                </td>

                {/* Actions */}
                <td>
                  {(i.status || '').toLowerCase() !== 'approved' &&
                  (i.status || '').toLowerCase() !== 'rejected' ? (
                    isApproverRole() ? (
                      <>
                        <button
                          className="primary"
                          onClick={() =>
                            handleApprove(
                              i.travelRequestId ||
                              i.TravelRequestId ||
                              i.id
                            )
                          }
                          style={{ marginRight: 8 }}
                        >
                          Approve
                        </button>

                        <button
                          onClick={() =>
                            handleReject(
                              i.travelRequestId ||
                              i.TravelRequestId ||
                              i.id
                            )
                          }
                          style={{
                            background: '#eee',
                            border: '1px solid #ddd',
                            padding: '6px 8px'
                          }}
                        >
                          Reject
                        </button>
                      </>
                    ) : (
                      <span className="small-muted">
                        Pending
                      </span>
                    )
                  ) : (
                    <span className="small-muted">
                      Done
                    </span>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}