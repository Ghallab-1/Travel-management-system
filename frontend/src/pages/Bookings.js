import React from 'react';
import Api from '../services/api';

export default function Bookings(){
  const [items, setItems] = React.useState([]);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState(null);

  React.useEffect(() => { load(); }, []);

  async function load(){
    setLoading(true);
    setError(null);
    try {
      const r = await Api.getBookings();
      setItems(r || []);
    } catch (e) {
      console.error(e);
      setError('Failed to load bookings. Ensure the API is running.');
    }
    setLoading(false);
  }

  return (
    <div className="container">
      <h2>Bookings</h2>
      <p className="small-muted">Bookings (from API)</p>
      {error && <p style={{color:'red'}}>{error}</p>}
      {loading ? <p>Loading...</p> : (
      <table>
        <thead><tr><th>ID</th><th>Request</th><th>Type</th><th>Provider</th></tr></thead>
        <tbody>
          {items.map(b => (
            <tr key={b.travelBookingId || b.TravelBookingId || b.id}><td>{b.travelBookingId || b.TravelBookingId || b.id}</td><td>{b.travelRequestId || b.TravelRequestId || b.requestId}</td><td>{b.bookingStatus || b.BookingStatus || '-'}</td><td>{b.notes || b.Notes || '-'}</td></tr>
          ))}
        </tbody>
      </table>)}
    </div>
  )
}
