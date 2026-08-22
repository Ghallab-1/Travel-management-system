import React from 'react';
import Api from '../services/api';

function currentRole() {
  try {
    const raw = window.localStorage.getItem('tms_user');
    const user = raw ? JSON.parse(raw) : null;
    return (user?.roleName || user?.role || '').toLowerCase();
  } catch (e) {
    return '';
  }
}

export default function Bookings() {
  const [bookings, setBookings] = React.useState([]);
  const [flights, setFlights] = React.useState([]);
  const [requests, setRequests] = React.useState([]);
  const [airlines, setAirlines] = React.useState([]);
  const [bookingForm, setBookingForm] = React.useState({
    travelRequestId: '',
    bookingStatus: 'Booked',
    notes: '',
  });
  const [flightForm, setFlightForm] = React.useState({
    travelBookingId: '',
    airlineId: '',
    flightNumber: '',
    bookingReference: '',
    departureAirport: '',
    arrivalAirport: '',
    departureDateTime: '',
    arrivalDateTime: '',
  });
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState(null);

  const role = currentRole();
  const canCoordinate = role.includes('travel coordinator') || role.includes('admin');

  React.useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  async function load() {
    setLoading(true);
    setError(null);

    try {
      const [bookingData, flightData, airlineData] = await Promise.all([
        Api.getBookings(),
        Api.getFlights(),
        Api.getAirlines(),
      ]);

      setBookings(bookingData || []);
      setFlights(flightData || []);
      setAirlines(airlineData || []);

      if (canCoordinate) {
        const requestData = await Api.getCoordinatorWork();
        const approved = (requestData || []).filter(
          (request) => String(request.status || '').toLowerCase() === 'approved'
        );
        setRequests(approved);
      }
    } catch (e) {
      console.error(e);
      setError('Failed to load booking data. Ensure the API is running.');
    }

    setLoading(false);
  }

  async function createBooking(e) {
    e.preventDefault();
    setError(null);

    try {
      await Api.createBooking({
        travelRequestId: parseInt(bookingForm.travelRequestId, 10),
        bookingStatus: bookingForm.bookingStatus,
        notes: bookingForm.notes,
      });
      setBookingForm({ travelRequestId: '', bookingStatus: 'Booked', notes: '' });
      await load();
    } catch (e) {
      console.error(e);
      setError(e.message || 'Failed to create booking.');
    }
  }

  async function createFlight(e) {
    e.preventDefault();
    setError(null);

    try {
      await Api.createFlight({
        travelBookingId: parseInt(flightForm.travelBookingId, 10),
        airlineId: parseInt(flightForm.airlineId, 10),
        flightNumber: flightForm.flightNumber,
        bookingReference: flightForm.bookingReference,
        departureAirport: flightForm.departureAirport,
        arrivalAirport: flightForm.arrivalAirport,
        departureDateTime: flightForm.departureDateTime,
        arrivalDateTime: flightForm.arrivalDateTime,
      });
      setFlightForm({
        travelBookingId: '',
        airlineId: '',
        flightNumber: '',
        bookingReference: '',
        departureAirport: '',
        arrivalAirport: '',
        departureDateTime: '',
        arrivalDateTime: '',
      });
      await load();
    } catch (e) {
      console.error(e);
      setError(e.message || 'Failed to create flight.');
    }
  }

  return (
    <div className="container">
      <h2>Flights</h2>
      <p className="small-muted">Coordinator booking and flight records</p>

      {error && <p style={{ color: 'red' }}>{error}</p>}

      {canCoordinate && (
        <div
          style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(260px, 1fr))',
            gap: 12,
            marginBottom: 20,
          }}
        >
          <form className="card" onSubmit={createBooking}>
            <h3>Create Booking</h3>

            <label>Approved Request</label>
            <select
              value={bookingForm.travelRequestId}
              onChange={(e) =>
                setBookingForm((current) => ({ ...current, travelRequestId: e.target.value }))
              }
              required
            >
              <option value="">Select request</option>
              {requests.map((request) => (
                <option key={request.travelRequestId} value={request.travelRequestId}>
                  #{request.travelRequestId} - {request.userName} - {request.destinationCityName}
                </option>
              ))}
            </select>

            <label>Status</label>
            <select
              value={bookingForm.bookingStatus}
              onChange={(e) =>
                setBookingForm((current) => ({ ...current, bookingStatus: e.target.value }))
              }
            >
              <option value="Booked">Booked</option>
              <option value="Pending">Pending</option>
              <option value="Cancelled">Cancelled</option>
            </select>

            <label>Notes</label>
            <textarea
              rows={3}
              value={bookingForm.notes}
              onChange={(e) =>
                setBookingForm((current) => ({ ...current, notes: e.target.value }))
              }
            />

            <button className="primary" type="submit">
              Save Booking
            </button>
          </form>

          <form className="card" onSubmit={createFlight}>
            <h3>Add Flight</h3>

            <label>Booking</label>
            <select
              value={flightForm.travelBookingId}
              onChange={(e) =>
                setFlightForm((current) => ({ ...current, travelBookingId: e.target.value }))
              }
              required
            >
              <option value="">Select booking</option>
              {bookings.map((booking) => (
                <option key={booking.travelBookingId} value={booking.travelBookingId}>
                  Booking #{booking.travelBookingId} - {booking.requesterName}
                </option>
              ))}
            </select>

            <label>Airline</label>
            <select
              value={flightForm.airlineId}
              onChange={(e) =>
                setFlightForm((current) => ({ ...current, airlineId: e.target.value }))
              }
              required
            >
              <option value="">Select airline</option>
              {airlines.map((airline) => (
                <option key={airline.airlineId || airline.AirlineId} value={airline.airlineId || airline.AirlineId}>
                  {airline.airlineName || airline.AirlineName}
                </option>
              ))}
            </select>

            <label>Flight Number</label>
            <input
              value={flightForm.flightNumber}
              onChange={(e) =>
                setFlightForm((current) => ({ ...current, flightNumber: e.target.value }))
              }
              required
            />

            <label>Booking Reference</label>
            <input
              value={flightForm.bookingReference}
              onChange={(e) =>
                setFlightForm((current) => ({ ...current, bookingReference: e.target.value }))
              }
            />

            <label>Departure Airport</label>
            <input
              value={flightForm.departureAirport}
              onChange={(e) =>
                setFlightForm((current) => ({ ...current, departureAirport: e.target.value }))
              }
            />

            <label>Arrival Airport</label>
            <input
              value={flightForm.arrivalAirport}
              onChange={(e) =>
                setFlightForm((current) => ({ ...current, arrivalAirport: e.target.value }))
              }
            />

            <label>Departure</label>
            <input
              type="datetime-local"
              value={flightForm.departureDateTime}
              onChange={(e) =>
                setFlightForm((current) => ({ ...current, departureDateTime: e.target.value }))
              }
              required
            />

            <label>Arrival</label>
            <input
              type="datetime-local"
              value={flightForm.arrivalDateTime}
              onChange={(e) =>
                setFlightForm((current) => ({ ...current, arrivalDateTime: e.target.value }))
              }
              required
            />

            <button className="primary" type="submit">
              Save Flight
            </button>
          </form>
        </div>
      )}

      {loading ? (
        <p>Loading...</p>
      ) : (
        <>
          <h3>Bookings</h3>
          <table>
            <thead>
              <tr>
                <th>ID</th>
                <th>Requester</th>
                <th>Destination</th>
                <th>Status</th>
                <th>Notes</th>
              </tr>
            </thead>
            <tbody>
              {bookings.map((booking) => (
                <tr key={booking.travelBookingId}>
                  <td>{booking.travelBookingId}</td>
                  <td>{booking.requesterName || 'User missing from API'}</td>
                  <td>{booking.destinationCityName || ''}</td>
                  <td>{booking.bookingStatus}</td>
                  <td>{booking.notes || '-'}</td>
                </tr>
              ))}
            </tbody>
          </table>

          <h3 style={{ marginTop: 24 }}>Flights</h3>
          <table>
            <thead>
              <tr>
                <th>ID</th>
                <th>Requester</th>
                <th>Airline</th>
                <th>Flight</th>
                <th>Route</th>
                <th>Departure</th>
              </tr>
            </thead>
            <tbody>
              {flights.map((flight) => (
                <tr key={flight.flightId}>
                  <td>{flight.flightId}</td>
                  <td>{flight.requesterName || 'User missing from API'}</td>
                  <td>{flight.airlineName || ''}</td>
                  <td>{flight.flightNumber}</td>
                  <td>
                    {flight.departureAirport} to {flight.arrivalAirport}
                  </td>
                  <td>
                    {flight.departureDateTime
                      ? new Date(flight.departureDateTime).toLocaleString()
                      : ''}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </>
      )}
    </div>
  );
}
