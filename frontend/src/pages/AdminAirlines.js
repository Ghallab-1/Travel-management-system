import React, { useEffect, useState } from 'react';
import airlinesData from '../data/airlines.json';

export default function AdminAirlines() {
  const [airlines, setAirlines] = useState([]);

  useEffect(() => {
    setAirlines(airlinesData);
  }, []);

  return (
    <div style={{ padding: 16 }}>
      <h2>Airlines (Admin)</h2>
      <ul>
        {airlines.map(a => <li key={a.id}>{a.name}</li>)}
      </ul>
    </div>
  );
}
