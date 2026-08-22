import React from 'react';
import Api from '../services/api';

function currentUser() {
  try {
    const raw = window.localStorage.getItem('tms_user');
    return raw ? JSON.parse(raw) : null;
  } catch (e) {
    return null;
  }
}

function canCoordinate(user) {
  const role = (user?.roleName || user?.role || '').toLowerCase();
  return role.includes('travel coordinator') || role.includes('admin');
}

export default function Expenses() {
  const user = currentUser();
  const showForm = canCoordinate(user);

  const [items, setItems] = React.useState([]);
  const [requests, setRequests] = React.useState([]);
  const [categories, setCategories] = React.useState([]);
  const [currencies, setCurrencies] = React.useState([]);
  const [form, setForm] = React.useState({
    travelRequestId: '',
    expenseCategoryId: '',
    currencyId: '',
    amount: '',
    description: '',
    expenseDate: '',
  });
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState(null);

  React.useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  async function load() {
    setLoading(true);
    setError(null);

    try {
      const [expenseData, categoryData, currencyData] = await Promise.all([
        Api.getExpenses(),
        Api.getExpenseCategories(),
        Api.getCurrencies(),
      ]);

      setItems(expenseData || []);
      setCategories(categoryData || []);
      setCurrencies(currencyData || []);

      if (showForm) {
        const requestData = await Api.getCoordinatorWork();
        setRequests(
          (requestData || []).filter(
            (request) => String(request.status || '').toLowerCase() === 'approved'
          )
        );
      }
    } catch (e) {
      console.error(e);
      setError('Failed to load expenses. Ensure the API is running.');
    }

    setLoading(false);
  }

  async function createExpense(e) {
    e.preventDefault();
    setError(null);

    const selectedRequest = requests.find(
      (request) => String(request.travelRequestId) === String(form.travelRequestId)
    );

    if (!selectedRequest) {
      setError('Select an approved request first.');
      return;
    }

    try {
      await Api.createExpense({
        travelRequestId: parseInt(form.travelRequestId, 10),
        userId: selectedRequest.userId,
        expenseCategoryId: parseInt(form.expenseCategoryId, 10),
        currencyId: parseInt(form.currencyId, 10),
        amount: parseFloat(form.amount) || 0,
        description: form.description,
        expenseDate: form.expenseDate,
      });

      setForm({
        travelRequestId: '',
        expenseCategoryId: '',
        currencyId: '',
        amount: '',
        description: '',
        expenseDate: '',
      });

      await load();
    } catch (e) {
      console.error(e);
      setError(e.message || 'Failed to save expense.');
    }
  }

  return (
    <div className="container">
      <h2>Expenses</h2>
      <p className="small-muted">Expense records from the travel workflow</p>

      {error && <p style={{ color: 'red' }}>{error}</p>}

      {showForm && (
        <form className="card" onSubmit={createExpense} style={{ marginBottom: 20 }}>
          <h3>Add Expense</h3>

          <label>Approved Request</label>
          <select
            value={form.travelRequestId}
            onChange={(e) => setForm((current) => ({ ...current, travelRequestId: e.target.value }))}
            required
          >
            <option value="">Select request</option>
            {requests.map((request) => (
              <option key={request.travelRequestId} value={request.travelRequestId}>
                #{request.travelRequestId} - {request.userName} - {request.destinationCityName}
              </option>
            ))}
          </select>

          <label>Category</label>
          <select
            value={form.expenseCategoryId}
            onChange={(e) => setForm((current) => ({ ...current, expenseCategoryId: e.target.value }))}
            required
          >
            <option value="">Select category</option>
            {categories.map((category) => (
              <option key={category.expenseCategoryId} value={category.expenseCategoryId}>
                {category.categoryName}
              </option>
            ))}
          </select>

          <label>Currency</label>
          <select
            value={form.currencyId}
            onChange={(e) => setForm((current) => ({ ...current, currencyId: e.target.value }))}
            required
          >
            <option value="">Select currency</option>
            {currencies.map((currency) => (
              <option key={currency.currencyId} value={currency.currencyId}>
                {currency.currencyCode} - {currency.currencyName}
              </option>
            ))}
          </select>

          <label>Amount</label>
          <input
            type="number"
            min="0"
            step="0.01"
            value={form.amount}
            onChange={(e) => setForm((current) => ({ ...current, amount: e.target.value }))}
            required
          />

          <label>Expense Date</label>
          <input
            type="date"
            value={form.expenseDate}
            onChange={(e) => setForm((current) => ({ ...current, expenseDate: e.target.value }))}
            required
          />

          <label>Description</label>
          <textarea
            rows={3}
            value={form.description}
            onChange={(e) => setForm((current) => ({ ...current, description: e.target.value }))}
          />

          <button className="primary" type="submit">
            Save Expense
          </button>
        </form>
      )}

      {loading ? (
        <p>Loading...</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>User</th>
              <th>Role</th>
              <th>Amount</th>
              <th>Category</th>
              <th>Date</th>
            </tr>
          </thead>
          <tbody>
            {items.map((expense) => (
              <tr key={expense.expenseId}>
                <td>{expense.expenseId}</td>
                <td>{expense.userName || 'User missing from API'}</td>
                <td>{expense.userRole || 'Role missing from API'}</td>
                <td>
                  {Number(expense.amount || 0).toFixed(2)} {expense.currencyCode || ''}
                </td>
                <td>{expense.categoryName || 'Category missing from API'}</td>
                <td>{expense.expenseDate || ''}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
