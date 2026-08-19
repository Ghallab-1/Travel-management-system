import React from 'react';
import Api from '../services/api';

export default function Hotels(){
  const [items, setItems] = React.useState([]);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState(null);

  React.useEffect(() => { load(); }, []);

  async function load(){
    setLoading(true);
    setError(null);
    try {
      const r = await Api.getHotelReservations();
      setItems(r || []);
    } catch (e) {
      console.error(e);
      setError('Failed to load hotels/reservations. Ensure the API is running.');
    }
    setLoading(false);
  }

  return (
    <div className="container">
      <h2>Hotels / Reservations</h2>
      <p className="small-muted">Hotel reservations (from API)</p>
      {error && <p style={{color:'red'}}>{error}</p>}
      {loading ? <p>Loading...</p> : (
        items.length===0 ? <p>No reservations</p> : (
        <table>
          <thead><tr><th>ID</th><th>Booking</th><th>Hotel</th><th>CheckIn</th><th>CheckOut</th></tr></thead>
          <tbody>
            {items.map(h => (
              <tr key={h.hotelReservationId || h.HotelReservationId || h.id}>
                <td>{h.hotelReservationId || h.HotelReservationId || h.id}</td>
                <td>{h.travelBookingId || h.TravelBookingId || '-'}</td>
                <td>{h.hotelId || h.HotelId || '-'}</td>
                <td>{h.checkInDate || h.CheckInDate || '-'}</td>
                <td>{h.checkOutDate || h.CheckOutDate || '-'}</td>
              </tr>
            ))}
          </tbody>
        </table>)
      )}
    </div>
  )
}
