import React from 'react';

export default function Reports(){
  return (
    <div className="container">
      <h2>Reports</h2>
      
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
        <div className="card"><h3>Monthly Spending</h3></div>
        <div className="card"><h3>Top Destinations</h3></div>
      </div>
    </div>
  )
}
