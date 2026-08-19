import React from 'react';
import Api from '../services/api';

export default function Expenses(){
  const [items, setItems] = React.useState([]);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState(null);

  React.useEffect(() => { load(); }, []);

  async function load(){
    setLoading(true);
    setError(null);
    try {
      const r = await Api.getExpenses();
      setItems(r || []);
    } catch (e) {
      console.error(e);
      setError('Failed to load expenses. Ensure the API is running.');
    }
    setLoading(false);
  }

  return (
    <div className="container">
      <h2>Expenses</h2>
      <p className="small-muted">Expense claims (from API)</p>
      {error && <p style={{color:'red'}}>{error}</p>}
      {loading ? <p>Loading...</p> : (
      <table>
        <thead><tr><th>ID</th><th>User</th><th>Amount</th><th>Category</th></tr></thead>
        <tbody>
          {items.map(x => (
            <tr key={x.expenseId || x.ExpenseId || x.id}><td>{x.expenseId || x.ExpenseId || x.id}</td><td>{x.userId || x.UserId || '-'}</td><td>{x.amount} {x.currencyId || x.CurrencyId || ''}</td><td>{x.expenseCategoryId || x.ExpenseCategoryId || '-'}</td></tr>
          ))}
        </tbody>
      </table>)}
    </div>
  )
}
