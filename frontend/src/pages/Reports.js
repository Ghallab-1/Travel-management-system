import React from 'react';

export default function Reports(){
  return (
    <div className="container">
      <h2>Reports</h2>
      <p className="small-muted">Mock reports and charts (placeholders)</p>
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
        <div className="card"><h3>Monthly Spending</h3><div className="small-muted">$12,430</div></div>
        <div className="card"><h3>Top Destinations</h3><ul><li>Cairo</li><li>Dubai</li></ul></div>
      </div>
    </div>
  )
}
