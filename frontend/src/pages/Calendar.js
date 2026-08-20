import React from 'react';
import Api from '../services/api';

export default function Calendar() {
  const [trips, setTrips] = React.useState([]);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState(null);

  React.useEffect(() => {
    async function loadTrips() {
      setLoading(true);
      setError(null);

      try {
        const all = await Api.getTravelRequests();

        const upcoming = (all || [])
          .filter((r) => {
            const status = String(r.status || '').toLowerCase();
            return status !== 'cancelled' && status !== 'rejected';
          })
          .map((r) => ({
            id: r.travelRequestId ?? r.TravelRequestId,
            title:
              r.purpose ||
              r.project ||
              r.destinationCityName ||
              'Trip',
            start: r.departureDate ?? r.DepartureDate,
            end: r.returnDate ?? r.ReturnDate,
            requester:
              r.requesterName ||
              r.userName ||
              r.userId ||
              '-',
            destination:
              r.destinationCityName ||
              r.destination ||
              '-',
            status: r.status || '-',
          }))
          .sort((a, b) => {
            const dateA = a.start ? new Date(a.start).getTime() : 0;
            const dateB = b.start ? new Date(b.start).getTime() : 0;
            return dateA - dateB;
          });

        setTrips(upcoming);
      } catch (e) {
        console.error(e);
        setError(
          'Failed to load travel calendar. Ensure the API is running.'
        );
      } finally {
        setLoading(false);
      }
    }

    loadTrips();
  }, []);

  return (
    <div className="container">
      <h2>Travel Calendar</h2>
      <p className="small-muted">
        Travel requests from the backend
      </p>

      {error && (
        <p style={{ color: 'red' }}>
          {error}
        </p>
      )}

      {loading ? (
        <p>Loading...</p>
      ) : trips.length === 0 ? (
        <p>No travel requests found.</p>
      ) : (
        <table className="table">
          <thead>
            <tr>
              <th>Trip</th>
              <th>Destination</th>
              <th>Requester</th>
              <th>Start</th>
              <th>End</th>
              <th>Status</th>
            </tr>
          </thead>

          <tbody>
            {trips.map((trip) => (
              <tr key={trip.id}>
                <td>{trip.title}</td>
                <td>{trip.destination}</td>
                <td>{trip.requester}</td>
                <td>{trip.start || '-'}</td>
                <td>{trip.end || '-'}</td>
                <td>{trip.status}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}