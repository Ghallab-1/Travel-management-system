import React from 'react';
import Api from '../services/api';

function canCoordinate() {
  try {
    const raw = window.localStorage.getItem('tms_user');
    const user = raw ? JSON.parse(raw) : null;
    const role = (user?.roleName || user?.role || '').toLowerCase();
    return role.includes('travel coordinator') || role.includes('admin');
  } catch (e) {
    return false;
  }
}

export default function Hotels() {
  const [reservations, setReservations] = React.useState([]);
  const [bookings, setBookings] = React.useState([]);
  const [hotels, setHotels] = React.useState([]);
  const [form, setForm] = React.useState({
    travelBookingId: '',
    hotelId: '',
    checkInDate: '',
    checkOutDate: '',
    numberOfNights: '',
    bookingReference: '',
  });
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState(null);
  const showForm = canCoordinate();

  React.useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  async function load() {
    setLoading(true);
    setError(null);

    try {
      const [reservationData, bookingData, hotelData] = await Promise.all([
        Api.getHotelReservations(),
        Api.getBookings(),
        Api.getHotels(),
      ]);

      setReservations(reservationData || []);
      setBookings(bookingData || []);
      setHotels(hotelData || []);
    } catch (e) {
      console.error(e);
      setError('Failed to load hotels/reservations. Ensure the API is running.');
    }

    setLoading(false);
  }

  async function createReservation(e) {
    e.preventDefault();
    setError(null);

    try {
      await Api.createHotelReservation({
        travelBookingId: parseInt(form.travelBookingId, 10),
        hotelId: parseInt(form.hotelId, 10),
        checkInDate: form.checkInDate,
        checkOutDate: form.checkOutDate,
        numberOfNights: parseInt(form.numberOfNights, 10) || 1,
        bookingReference: form.bookingReference,
      });

      setForm({
        travelBookingId: '',
        hotelId: '',
        checkInDate: '',
        checkOutDate: '',
        numberOfNights: '',
        bookingReference: '',
      });

      await load();
    } catch (e) {
      console.error(e);
      setError(e.message || 'Failed to save hotel reservation.');
    }
  }

  return (
    <div className="container">
      <h2>Hotels</h2>
      <p className="small-muted">Coordinator hotel reservations</p>

      {error && <p style={{ color: 'red' }}>{error}</p>}

      {showForm && (
        <form className="card" onSubmit={createReservation} style={{ marginBottom: 20 }}>
          <h3>Create Hotel Reservation</h3>

          <label>Booking</label>
          <select
            value={form.travelBookingId}
            onChange={(e) => setForm((current) => ({ ...current, travelBookingId: e.target.value }))}
            required
          >
            <option value="">Select booking</option>
            {bookings.map((booking) => (
              <option key={booking.travelBookingId} value={booking.travelBookingId}>
                Booking #{booking.travelBookingId} - {booking.requesterName}
              </option>
            ))}
          </select>

          <label>Hotel</label>
          <select
            value={form.hotelId}
            onChange={(e) => setForm((current) => ({ ...current, hotelId: e.target.value }))}
            required
          >
            <option value="">Select hotel</option>
            {hotels.map((hotel) => (
              <option key={hotel.hotelId} value={hotel.hotelId}>
                {hotel.hotelName} - {hotel.cityName}
              </option>
            ))}
          </select>

          <label>Check In</label>
          <input
            type="date"
            value={form.checkInDate}
            onChange={(e) => setForm((current) => ({ ...current, checkInDate: e.target.value }))}
            required
          />

          <label>Check Out</label>
          <input
            type="date"
            value={form.checkOutDate}
            onChange={(e) => setForm((current) => ({ ...current, checkOutDate: e.target.value }))}
            required
          />

          <label>Nights</label>
          <input
            type="number"
            min="1"
            value={form.numberOfNights}
            onChange={(e) => setForm((current) => ({ ...current, numberOfNights: e.target.value }))}
            required
          />

          <label>Booking Reference</label>
          <input
            value={form.bookingReference}
            onChange={(e) => setForm((current) => ({ ...current, bookingReference: e.target.value }))}
          />

          <button className="primary" type="submit">
            Save Hotel Reservation
          </button>
        </form>
      )}

      {loading ? (
        <p>Loading...</p>
      ) : reservations.length === 0 ? (
        <p>No reservations</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Requester</th>
              <th>Hotel</th>
              <th>City</th>
              <th>Check In</th>
              <th>Check Out</th>
            </tr>
          </thead>
          <tbody>
            {reservations.map((reservation) => (
              <tr key={reservation.hotelReservationId}>
                <td>{reservation.hotelReservationId}</td>
                <td>{reservation.requesterName || 'User missing from API'}</td>
                <td>{reservation.hotelName || ''}</td>
                <td>{reservation.cityName || ''}</td>
                <td>{reservation.checkInDate || ''}</td>
                <td>{reservation.checkOutDate || ''}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
