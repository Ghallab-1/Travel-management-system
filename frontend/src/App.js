import React, { useState } from 'react';
import './App.css';
import NavBar from './components/NavBar';
import Sidebar from './components/Sidebar';
import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import CreateRequest from './pages/CreateRequest';
import MyRequests from './pages/MyRequests';
import RequestDetail from './pages/RequestDetail';
import AdminDepartments from './pages/AdminDepartments';
import AdminAirlines from './pages/AdminAirlines';
import Approvals from './pages/Approvals';
import Bookings from './pages/Bookings';
import Expenses from './pages/Expenses';
import Reports from './pages/Reports';
import Hotels from './pages/Hotels';
import Notifications from './pages/Notifications';
import Calendar from './pages/Calendar';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';

function App() {
  const [currentUser, setCurrentUser] = useState(() => {
    try { const raw = window.localStorage.getItem('tms_user'); return raw ? JSON.parse(raw) : null } catch (e) { return null }
  });

  function handleLogin(user) {
    setCurrentUser(user);
    try { window.localStorage.setItem('tms_user', JSON.stringify(user)); } catch (e) {}
  }

  function handleLogout(){
    setCurrentUser(null);
    try { window.localStorage.removeItem('tms_user'); } catch(e){}
  }

  // When not authenticated show only the Login page (no NavBar / Sidebar)
  if (!currentUser) {
    return (
      <>
        <div className="bg-blob-1" />
        <div className="bg-blob-2" />
        <BrowserRouter>
          <Routes>
            <Route path="/login" element={<Login onLogin={handleLogin} />} />
            <Route path="*" element={<Navigate to="/login" />} />
          </Routes>
        </BrowserRouter>
      </>
    );
  }

  // Authenticated layout
  return (
    <>
      <div className="bg-blob-1" />
      <div className="bg-blob-2" />
      <BrowserRouter>
        <div style={{ display: 'flex', minHeight: '100vh' }}>
          <Sidebar currentUser={currentUser} />
          <div style={{ flex: 1 }}>
            <NavBar currentUser={currentUser} onLogout={handleLogout} />
            <main>
              <Routes>
                <Route path="/" element={<Dashboard currentUser={currentUser} />} />
                <Route path="/create" element={<CreateRequest currentUser={currentUser} />} />
                <Route path="/myrequests" element={<MyRequests currentUser={currentUser} />} />
                <Route path="/requests/:id" element={<RequestDetail />} />
                <Route path="/admin/departments" element={<AdminDepartments />} />
                <Route path="/admin/airlines" element={<AdminAirlines />} />
                <Route path="/approvals" element={<Approvals />} />
                <Route path="/bookings" element={<Bookings />} />
                <Route path="/hotels" element={<Hotels />} />
                <Route path="/expenses" element={<Expenses />} />
                <Route path="/reports" element={<Reports />} />
                <Route path="/calendar" element={<Calendar currentUser={currentUser} />} />
                <Route path="/notifications" element={<Notifications currentUser={currentUser} />} />
                <Route path="*" element={<Navigate to="/" />} />
              </Routes>
            </main>
          </div>
        </div>
      </BrowserRouter>
    </>
  );
}

export default App;
