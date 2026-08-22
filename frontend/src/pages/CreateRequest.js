import React, { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import api from '../services/api';

function readPdfAsBase64(file) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();

    reader.onload = () => {
      const result = String(reader.result || '');
      resolve(result.includes(',') ? result.split(',')[1] : result);
    };

    reader.onerror = () => reject(reader.error);
    reader.readAsDataURL(file);
  });
}

export default function CreateRequest({ currentUser, onCreated }) {
  const [form, setForm] = useState({
    destinationCityId: '',
    purpose: '',
    project: '',
    travelType: 'Domestic',
    departureDate: '',
    returnDate: '',
    requiredDocumentNotes: '',
    requiredDocumentFileName: '',
    requiredDocumentFileContentType: '',
    requiredDocumentFileBase64: '',
  });

  const [cities, setCities] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [message, setMessage] = useState(null);
  const navigate = useNavigate();

  useEffect(() => {
    api
      .getCities()
      .then((items) => {
        setCities(items || []);
        if (items?.length > 0) {
          updateField('destinationCityId', items[0].id);
        }
      })
      .catch(() => setError('Could not load destination cities. Is the API running?'));
  }, []);

  const selectedCity = useMemo(() => {
    return cities.find((city) => String(city.id) === String(form.destinationCityId));
  }, [cities, form.destinationCityId]);

  const documentGuidance = useMemo(() => {
    if (form.travelType === 'International') {
      return [
        'Valid passport',
        'Visa or entry permit if the destination requires it',
        'Company invitation or assignment letter',
        'Flight and hotel confirmations once booked',
        'Travel insurance and any destination health/customs documents',
      ];
    }

    return [
      'Valid company or government ID',
      'Manager-approved travel request',
      'Flight/hotel confirmations once booked if the trip requires them',
    ];
  }, [form.travelType]);

  function updateField(key, value) {
    setForm((current) => ({ ...current, [key]: value }));
  }

  async function handlePdfChange(e) {
    const file = e.target.files?.[0];

    if (!file) {
      updateField('requiredDocumentFileName', '');
      updateField('requiredDocumentFileContentType', '');
      updateField('requiredDocumentFileBase64', '');
      return;
    }

    if (file.type !== 'application/pdf' && !file.name.toLowerCase().endsWith('.pdf')) {
      setError('Only PDF files can be attached.');
      e.target.value = '';
      return;
    }

    try {
      const base64 = await readPdfAsBase64(file);
      setForm((current) => ({
        ...current,
        requiredDocumentFileName: file.name,
        requiredDocumentFileContentType: file.type || 'application/pdf',
        requiredDocumentFileBase64: base64,
      }));
      setError(null);
    } catch (err) {
      console.error(err);
      setError('Could not read the selected PDF.');
    }
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setLoading(true);
    setMessage(null);
    setError(null);

    const payload = {
      userId: currentUser.id,
      departmentId: currentUser.departmentId,
      travelPolicyId: 1,
      destinationCityId: parseInt(form.destinationCityId, 10),
      purpose: form.purpose,
      project: form.project,
      travelType: form.travelType,
      departureDate: form.departureDate,
      returnDate: form.returnDate,
      estimatedBudget: 0,
      requiredDocumentNotes: form.requiredDocumentNotes,
      requiredDocumentFileName: form.requiredDocumentFileName,
      requiredDocumentFileContentType: form.requiredDocumentFileContentType,
      requiredDocumentFileBase64: form.requiredDocumentFileBase64,
    };

    try {
      const created = await api.createRequest(payload);
      setMessage('Request submitted successfully.');
      if (onCreated) onCreated(created.travelRequestId);
      navigate(`/requests/${created.travelRequestId}`);
    } catch (err) {
      console.error(err);
      setError('Failed to submit request. Check console for details.');
    }

    setLoading(false);
  }

  return (
    <div className="container">
      <h2>Create Travel Request</h2>

      {error && <p style={{ color: 'red' }}>{error}</p>}

      <form onSubmit={handleSubmit} style={{ maxWidth: 720 }}>
        <div>
          <label>Destination</label>
          <select
            value={form.destinationCityId}
            onChange={(e) => updateField('destinationCityId', e.target.value)}
            style={{ width: '100%', padding: 8 }}
          >
            {cities.map((city) => (
              <option key={city.id} value={city.id}>
                {city.name}, {city.country}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label>Purpose</label>
          <input
            value={form.purpose}
            onChange={(e) => updateField('purpose', e.target.value)}
            style={{ width: '100%', padding: 8 }}
            required
          />
        </div>

        <div>
          <label>Project</label>
          <input
            value={form.project}
            onChange={(e) => updateField('project', e.target.value)}
            style={{ width: '100%', padding: 8 }}
          />
        </div>

        <div>
          <label>Travel Type</label>
          <select
            value={form.travelType}
            onChange={(e) => updateField('travelType', e.target.value)}
            style={{ width: '100%', padding: 8 }}
          >
            <option value="Domestic">Domestic</option>
            <option value="International">International</option>
          </select>
        </div>

        <div>
          <label>Departure Date</label>
          <input
            type="date"
            value={form.departureDate}
            onChange={(e) => updateField('departureDate', e.target.value)}
            style={{ padding: 8 }}
            required
          />
        </div>

        <div>
          <label>Return Date</label>
          <input
            type="date"
            value={form.returnDate}
            onChange={(e) => updateField('returnDate', e.target.value)}
            style={{ padding: 8 }}
            required
          />
        </div>

        <div className="card" style={{ marginTop: 12 }}>
          <h3>Travel Documents</h3>
          <p className="small-muted">
            {selectedCity
              ? `Guidance for ${selectedCity.name}, ${selectedCity.country}`
              : 'Guidance for the selected destination'}
          </p>

          <ul>
            {documentGuidance.map((item) => (
              <li key={item}>{item}</li>
            ))}
          </ul>

          <label>Additional document notes</label>
          <textarea
            value={form.requiredDocumentNotes}
            onChange={(e) => updateField('requiredDocumentNotes', e.target.value)}
            rows={4}
            style={{ width: '100%', padding: 8 }}
          />

          <label>Required documents PDF</label>
          <input type="file" accept="application/pdf,.pdf" onChange={handlePdfChange} />

          {form.requiredDocumentFileName && (
            <p className="small-muted">Attached: {form.requiredDocumentFileName}</p>
          )}
        </div>

        <div style={{ marginTop: 12 }}>
          <button type="submit" disabled={loading || cities.length === 0}>
            Submit
          </button>
        </div>
      </form>

      {message && <p style={{ color: 'green' }}>{message}</p>}
    </div>
  );
}
