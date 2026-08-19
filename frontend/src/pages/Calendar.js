import React from 'react';
import mockApi from '../services/mockApi';

export default function Calendar(){
  const [trips, setTrips] = React.useState([]);

  React.useEffect(()=>{
    mockApi.getAllRequests().then(all=>{
      const upcoming = all.filter(r=> r.status !== 'Cancelled').map(r=>({
        id: r.id,
        title: r.title || (r.purpose || 'Trip'),
        start: r.startDate,
        end: r.endDate,
        requester: r.requesterName || r.requester
      }));
      setTrips(upcoming);
    })
  },[]);

  return (
    <div className="container">
      <h2>Travel Calendar</h2>
      <p className="small-muted">Upcoming trips (simplified view)</p>
      {trips.length===0 ? <p>No upcoming trips</p> : (
        <table className="table">
          <thead><tr><th>Title</th><th>Requester</th><th>Start</th><th>End</th></tr></thead>
          <tbody>
            {trips.map(t=> (
              <tr key={t.id}>
                <td>{t.title}</td>
                <td>{t.requester}</td>
                <td>{t.start}</td>
                <td>{t.end}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  )
}
