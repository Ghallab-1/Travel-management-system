import React, { useEffect, useState } from 'react';
import Api from '../services/api';

export default function AdminDepartments() {
  const [departments, setDepartments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  async function load() {
    setLoading(true);
    setError(null);
    try {
      const data = await Api.getDepartments();
      // Normalize backend shape to { id, name, description, isActive }
      setDepartments(data.map(d => ({ id: d.id, name: d.name, description: d.description || '', isActive: !!d.isActive })));
    } catch (e) {
      console.error(e);
      setError('Failed to load departments.');
    }
    setLoading(false);
  }

  useEffect(() => { load(); }, []);

  async function handleAdd() {
    const name = window.prompt('Department name');
    if (!name) return;
    const description = window.prompt('Description (optional)') || '';
    try {
      const created = await Api.createDepartment({ name, description, isActive: true });
      // created returns the created entity
      setDepartments(prev => [...prev, { id: created.id, name: created.name, description: created.description || '', isActive: created.isActive }]);
    } catch (e) { console.error(e); alert('Failed to create department'); }
  }

  async function handleEdit(id) {
    const dept = departments.find(d => d.id === id);
    if (!dept) return;
    const name = window.prompt('Department name', dept.name) || dept.name;
    const description = window.prompt('Description (optional)', dept.description) ?? dept.description;
    const isActive = window.confirm('Mark as active? Cancel = Inactive') ? true : false;
    try {
      await Api.updateDepartment(id, { name, description, isActive });
      setDepartments(prev => prev.map(d => d.id === id ? { ...d, name, description, isActive } : d));
    } catch (e) { console.error(e); alert('Failed to update department'); }
  }

  async function handleDelete(id) {
    if (!window.confirm('Delete this department?')) return;
    try {
      await Api.deleteDepartment(id);
      setDepartments(prev => prev.filter(d => d.id !== id));
    } catch (e) { console.error(e); alert('Failed to delete department'); }
  }

  return (
    <div style={{ padding: 16 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h2>Departments (Admin)</h2>
        <div>
          <button onClick={handleAdd} style={{ padding: '6px 12px', marginLeft: 8 }}>+ Add Department</button>
        </div>
      </div>

      {loading && <p>Loading departments...</p>}
      {error && <p style={{ color: 'red' }}>{error}</p>}

      {!loading && (
        <table style={{ width: '100%', borderCollapse: 'collapse', marginTop: 12 }}>
          <thead>
            <tr style={{ textAlign: 'left', borderBottom: '1px solid #ddd' }}>
              <th style={{ padding: 8 }}>Name</th>
              <th style={{ padding: 8 }}>Description</th>
              <th style={{ padding: 8, width: 120 }}>Active</th>
              <th style={{ padding: 8, width: 200 }}>Actions</th>
            </tr>
          </thead>
          <tbody>
            {departments.map(d => (
              <tr key={d.id} style={{ borderBottom: '1px solid #f0f0f0' }}>
                <td style={{ padding: 8 }}>{d.name}</td>
                <td style={{ padding: 8 }}>{d.description}</td>
                <td style={{ padding: 8 }}>{d.isActive ? 'Yes' : 'No'}</td>
                <td style={{ padding: 8 }}>
                  <button onClick={() => handleEdit(d.id)} style={{ marginRight: 8 }}>Edit</button>
                  <button onClick={() => handleDelete(d.id)}>Delete</button>
                </td>
              </tr>
            ))}
            {departments.length === 0 && (
              <tr>
                <td colSpan={4} style={{ padding: 12 }}>No departments found.</td>
              </tr>
            )}
          </tbody>
        </table>
      )}

      <p style={{ marginTop: 12, color: '#666' }}>Changes are persisted to the backend via API in this Tasks copy.</p>
    </div>
  );
}
