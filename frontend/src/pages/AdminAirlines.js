import React from 'react';
import Api from '../services/api';

export default function AdminAirlines() {
  const [airlines, setAirlines] = React.useState([]);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState(null);

  const [name, setName] = React.useState('');
  const [code, setCode] = React.useState('');
  const [isActive, setIsActive] = React.useState(true);

  const [editingId, setEditingId] = React.useState(null);

  async function loadAirlines() {
    setLoading(true);
    setError(null);

    try {
      const data = await Api.getAirlines();
      setAirlines(data || []);
    } catch (e) {
      console.error(e);
      setError(
        'Failed to load airlines. Ensure the API is running.'
      );
    } finally {
      setLoading(false);
    }
  }

  React.useEffect(() => {
    loadAirlines();
  }, []);

  function resetForm() {
    setName('');
    setCode('');
    setIsActive(true);
    setEditingId(null);
  }

  function startEdit(airline) {
    setEditingId(
      airline.airlineId ?? airline.AirlineId
    );

    setName(
      airline.airlineName ??
      airline.AirlineName ??
      ''
    );

    setCode(
      airline.airlineCode ??
      airline.AirlineCode ??
      ''
    );

    setIsActive(
      airline.isActive ??
      airline.IsActive ??
      true
    );

    setError(null);
  }

  async function handleSubmit(e) {
    e.preventDefault();

    setError(null);

    if (!name.trim()) {
      setError('Airline name is required.');
      return;
    }

    if (!code.trim()) {
      setError('Airline code is required.');
      return;
    }

    const payload = {
      airlineName: name.trim(),
      airlineCode: code.trim().toUpperCase(),
      isActive,
    };

    try {
      if (editingId !== null) {
        await Api.updateAirline(
          editingId,
          payload
        );
      } else {
        await Api.createAirline(payload);
      }

      resetForm();
      await loadAirlines();
    } catch (e) {
      console.error(e);

      setError(
        e?.message ||
        'Failed to save airline.'
      );
    }
  }

  async function handleDelete(id) {
    const confirmed = window.confirm(
      'Are you sure you want to delete this airline?'
    );

    if (!confirmed) return;

    setError(null);

    try {
      await Api.deleteAirline(id);
      await loadAirlines();
    } catch (e) {
      console.error(e);

      setError(
        e?.message ||
        'Failed to delete airline.'
      );
    }
  }

  return (
    <div className="container">
      <h2>Airlines (Admin)</h2>

      {error && (
        <p style={{ color: 'red' }}>
          {error}
        </p>
      )}

      <div
        className="card"
        style={{ marginBottom: 20 }}
      >
        <h3>
          {editingId !== null
            ? 'Edit Airline'
            : 'Add Airline'}
        </h3>

        <form onSubmit={handleSubmit}>
          <div
            style={{
              display: 'grid',
              gridTemplateColumns:
                '1fr 1fr auto',
              gap: 12,
              alignItems: 'end',
            }}
          >
            <div>
              <label>
                Airline Name
              </label>

              <input
                value={name}
                onChange={(e) =>
                  setName(e.target.value)
                }
                placeholder="EgyptAir"
              />
            </div>

            <div>
              <label>
                Airline Code
              </label>

              <input
                value={code}
                onChange={(e) =>
                  setCode(e.target.value)
                }
                placeholder="MS"
                maxLength={10}
              />
            </div>

            <label
              style={{
                display: 'flex',
                gap: 8,
                alignItems: 'center',
                paddingBottom: 8,
              }}
            >
              <input
                type="checkbox"
                checked={isActive}
                onChange={(e) =>
                  setIsActive(
                    e.target.checked
                  )
                }
              />

              Active
            </label>
          </div>

          <div
            style={{
              display: 'flex',
              gap: 8,
              marginTop: 12,
            }}
          >
            <button type="submit">
              {editingId !== null
                ? 'Update Airline'
                : 'Add Airline'}
            </button>

            {editingId !== null && (
              <button
                type="button"
                onClick={resetForm}
              >
                Cancel
              </button>
            )}
          </div>
        </form>
      </div>

      {loading ? (
        <p>Loading airlines...</p>
      ) : airlines.length === 0 ? (
        <p>No airlines found.</p>
      ) : (
        <table className="table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Name</th>
              <th>Code</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>

          <tbody>
            {airlines.map((airline) => {
              const id =
                airline.airlineId ??
                airline.AirlineId;

              const airlineName =
                airline.airlineName ??
                airline.AirlineName;

              const airlineCode =
                airline.airlineCode ??
                airline.AirlineCode;

              const active =
                airline.isActive ??
                airline.IsActive;

              return (
                <tr key={id}>
                  <td>{id}</td>

                  <td>{airlineName}</td>

                  <td>{airlineCode}</td>

                  <td>
                    {active
                      ? 'Active'
                      : 'Inactive'}
                  </td>

                  <td>
                    <div
                      style={{
                        display: 'flex',
                        gap: 8,
                      }}
                    >
                      <button
                        type="button"
                        onClick={() =>
                          startEdit(airline)
                        }
                      >
                        Edit
                      </button>

                      <button
                        type="button"
                        onClick={() =>
                          handleDelete(id)
                        }
                      >
                        Delete
                      </button>
                    </div>
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