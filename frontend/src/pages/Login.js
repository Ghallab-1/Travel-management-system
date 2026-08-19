import React, { useState } from 'react';
import api from '../services/api';
import { useNavigate } from 'react-router-dom';
import logo from '../assets/logo.svg';

export default function Login({ onLogin }) {
  const [email, setEmail] = useState('demo@company.com');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const navigate = useNavigate();

  async function handleSubmit(e) {
    e.preventDefault();
    setLoading(true);
    setError(null);
    try {
      // For now use backend auth
      const password = window.prompt('Enter password for demo user (try Password123!)') || '';
      const result = await api.login(email.trim(), password);
      // store token and user
      try { window.localStorage.setItem('tms_token', result.token); } catch (e) {}
      const user = { id: result.userId, fullName: result.fullName, email: email.trim(), role: result.roleName };
      onLogin(user);
      try { window.localStorage.setItem('tms_user', JSON.stringify(user)); } catch (e) {}
      navigate('/');
    } catch (err) {
      console.error(err);
      setError('Login failed. Check credentials and that the API is running.');
    }
    setLoading(false);
  }

  return (
    <div className="centered-page">
      <div className="login-card">
        <div className="login-header">
          <img src={logo} alt="logo" />
          <div className="login-title">Travel Management System</div>
        </div>
        <h2>Login (mock)</h2>
        <form onSubmit={handleSubmit}>
          <label>Email (mock):</label>
          <input value={email} onChange={(e) => setEmail(e.target.value)} />
          <div className="login-actions">
            <button type="submit" className="primary" disabled={loading}>Sign in</button>
          </div>
        </form>
        {error && <p style={{ color: 'red' }}>{error}</p>}
      </div>
    </div>
  );
}
