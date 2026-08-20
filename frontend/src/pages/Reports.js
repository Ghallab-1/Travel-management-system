import React from 'react';
import Api from '../services/api';

export default function Reports() {
  const [expenses, setExpenses] = React.useState([]);
  const [requests, setRequests] = React.useState([]);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState(null);

  React.useEffect(() => {
    async function loadReports() {
      setLoading(true);
      setError(null);

      try {
        const [expenseData, requestData] = await Promise.all([
          Api.getExpenses(),
          Api.getTravelRequests(),
        ]);

        setExpenses(expenseData || []);
        setRequests(requestData || []);
      } catch (e) {
        console.error(e);
        setError(
          'Failed to load report data. Ensure the API is running.'
        );
      } finally {
        setLoading(false);
      }
    }

    loadReports();
  }, []);

  const getValue = (obj, camel, pascal) =>
    obj?.[camel] ?? obj?.[pascal];

  const totalSpending = expenses.reduce((sum, expense) => {
    const amount = Number(
      getValue(expense, 'amount', 'Amount') || 0
    );

    return sum + amount;
  }, 0);

  const currentMonth = new Date().getMonth();
  const currentYear = new Date().getFullYear();

  const monthlySpending = expenses
    .filter((expense) => {
      const dateValue = getValue(
        expense,
        'expenseDate',
        'ExpenseDate'
      );

      if (!dateValue) return false;

      const date = new Date(dateValue);

      return (
        date.getMonth() === currentMonth &&
        date.getFullYear() === currentYear
      );
    })
    .reduce((sum, expense) => {
      return (
        sum +
        Number(
          getValue(expense, 'amount', 'Amount') || 0
        )
      );
    }, 0);

  const destinationCounts = {};

  requests.forEach((request) => {
    const destination =
      getValue(
        request,
        'destinationCityName',
        'DestinationCityName'
      ) ||
      getValue(
        request,
        'destination',
        'Destination'
      ) ||
      'Unknown';

    destinationCounts[destination] =
      (destinationCounts[destination] || 0) + 1;
  });

  const topDestinations = Object.entries(destinationCounts)
    .sort((a, b) => b[1] - a[1])
    .slice(0, 5);

  if (loading) {
    return (
      <div className="container">
        <h2>Reports</h2>
        <p>Loading report data...</p>
      </div>
    );
  }

  return (
    <div className="container">
      <h2>Reports</h2>

      {error && (
        <p style={{ color: 'red' }}>
          {error}
        </p>
      )}

      <div
        style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))',
          gap: 12,
          marginBottom: 20,
        }}
      >
        <div className="card">
          <h3>Total Spending</h3>
          <p style={{ fontSize: 24, fontWeight: 700 }}>
            {totalSpending.toFixed(2)}
          </p>
        </div>

        <div className="card">
          <h3>This Month</h3>
          <p style={{ fontSize: 24, fontWeight: 700 }}>
            {monthlySpending.toFixed(2)}
          </p>
        </div>

        <div className="card">
          <h3>Total Expenses</h3>
          <p style={{ fontSize: 24, fontWeight: 700 }}>
            {expenses.length}
          </p>
        </div>

        <div className="card">
          <h3>Total Requests</h3>
          <p style={{ fontSize: 24, fontWeight: 700 }}>
            {requests.length}
          </p>
        </div>
      </div>

      <div
        style={{
          display: 'grid',
          gridTemplateColumns: '1fr 1fr',
          gap: 12,
        }}
      >
        <div className="card">
          <h3>Monthly Spending</h3>

          <p>
            Current month spending:{' '}
            <strong>
              {monthlySpending.toFixed(2)}
            </strong>
          </p>

          <p className="small-muted">
            Calculated from real expense records in the database.
          </p>
        </div>

        <div className="card">
          <h3>Top Destinations</h3>

          {topDestinations.length === 0 ? (
            <p>No destination data available.</p>
          ) : (
            <ol>
              {topDestinations.map(([destination, count]) => (
                <li key={destination}>
                  {destination} — {count} request
                  {count !== 1 ? 's' : ''}
                </li>
              ))}
            </ol>
          )}
        </div>
      </div>
    </div>
  );
}