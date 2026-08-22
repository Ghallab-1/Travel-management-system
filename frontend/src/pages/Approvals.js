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
      /*
       * This must use the authenticated
       * pending-for-me endpoint.
       */
      const r = await Api.getPendingForMe();

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
      const raw =
        window.localStorage.getItem('tms_user');

      if (!raw) return null;

      return JSON.parse(raw);
    } catch (e) {
      return null;
    }
  }

  function getApproverId() {
    const u = getCurrentUser();

    if (!u) return null;

    return (
      u.id ||
      u.userId ||
      u.Id ||
      null
    );
  }

  function isApproverRole() {
    const u = getCurrentUser();

    if (!u) return false;

    const role = (
      u.role ||
      u.roleName ||
      ''
    )
      .toString()
      .toLowerCase();

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
      /*
       * Empty comment means the approver
       * did not provide a comment.
       */
      await Api.approveRequest(
        id,
        approverId,
        '',
        ''
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
      /*
       * Empty comment means the rejecter
       * did not provide a comment.
       */
      await Api.rejectRequest(
        id,
        approverId,
        '',
        ''
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
                  Number(
                    a.travelRequestId ||
                      a.TravelRequestId ||
                      a.id
                  ) -
                  Number(
                    b.travelRequestId ||
                      b.TravelRequestId ||
                      b.id
                  )
              )
              .map((i, index) => {
                const requestId =
                  i.travelRequestId ||
                  i.TravelRequestId ||
                  i.id;

                const displayId = index + 1;

                const status =
                  (i.status || 'Draft').toLowerCase();

                const userName =
                  i.userName ||
                  i.UserName ||
                  i.fullName ||
                  i.FullName ||
                  '-';

                const userRole =
                  i.userRole ||
                  i.UserRole ||
                  i.roleName ||
                  i.RoleName ||
                  i.role ||
                  i.Role ||
                  '-';

                return (
                  <tr key={requestId}>
                    <td>{displayId}</td>

                    <td>{userName}</td>

                    <td>{userRole}</td>

                    <td>
                      {i.purpose ||
                        i.Purpose ||
                        i.destination ||
                        '-'}
                    </td>

                    <td>
                      <span
                        className={`badge ${status}`}
                      >
                        {i.status || 'Draft'}
                      </span>
                    </td>

                    <td>
                      {status !== 'approved' &&
                      status !== 'rejected' ? (
                        isApproverRole() ? (
                          <>
                            <button
                              className="primary"
                              onClick={() =>
                                handleApprove(
                                  requestId
                                )
                              }
                              style={{
                                marginRight: 8
                              }}
                            >
                              Approve
                            </button>

                            <button
                              onClick={() =>
                                handleReject(
                                  requestId
                                )
                              }
                              style={{
                                background: '#eee',
                                border:
                                  '1px solid #ddd',
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
                );
              })}
          </tbody>
        </table>
      )}
    </div>
  );
}