import React, { useEffect } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { useParams, useNavigate } from 'react-router-dom';
import type { RootState, AppDispatch } from '../../../app/store';
import {
  fetchUserAuditHistory,
  type AuditEntry,
} from '../../admin/store/adminSlice';

import './Css/UserAuditHistoryPage.css';

const UserAuditHistoryPage: React.FC = () => {
  // Extract userId from the URL parameters.
  const { userId } = useParams<{ userId: string }>();
  const navigate = useNavigate();
  const dispatch: AppDispatch = useDispatch();

  // Select audit history data and its loading/error states from the Redux store.
  const {
    userAuditHistory,
    auditLoading,
    auditError,
  } = useSelector((state: RootState) => state.admin);

  // Fetch user-specific audit history when the component mounts or userId changes.
  useEffect(() => {
    if (userId) {
      dispatch(fetchUserAuditHistory(userId));
    }
  }, [dispatch, userId]);

  // Formats a timestamp string into a localized date and time string.
  const formatTimestamp = (timestamp: string) => {
    return new Date(timestamp).toLocaleString();
  };

  // Display an error message if no user ID is provided in the URL.
  if (!userId) {
    return <div className="user-audit-container">Invalid User ID provided.</div>;
  }

  // Display a loading message while audit history data is being fetched.
  if (auditLoading) {
    return <div className="user-audit-container">Loading user audit history...</div>;
  }

  // Display an error message if fetching audit history failed.
  if (auditError) {
    return <div className="user-audit-container error-message">Error: {auditError}</div>;
  }

  return (
    <div className="user-audit-container">
      {/* Display a truncated user ID in the title. */}
      <h1 className="user-audit-title">Audit History for User ID: {userId.substring(0, 8)}...</h1>

      {/* Button to navigate back to the admin dashboard. */}
      <button className="back-button" onClick={() => navigate('/admin-dashboard')}>
        &larr; Back to Admin Dashboard
      </button>

      {/* Conditionally render the audit history table or a no-data message. */}
      {userAuditHistory.length > 0 ? (
        <div className="table-container">
          <table className="audit-table">
            <thead>
              <tr>
                <th>Timestamp</th>
                <th>Entity Type</th>
                <th>Entity ID</th>
                <th>Change Type</th>
                <th>Property</th>
                <th>Old Value</th>
                <th>New Value</th>
                <th>Changed By</th>
              </tr>
            </thead>
            <tbody>
              {userAuditHistory.map((entry: AuditEntry) => (
                <tr key={entry.id}>
                  <td>{formatTimestamp(entry.changeTimestamp)}</td>
                  <td>{entry.entityType}</td>
                  <td>{entry.entityId.substring(0, 8)}...</td>
                  <td>{entry.changeType}</td>
                  <td>{entry.propertyName || 'N/A'}</td>
                  <td>{entry.oldValue || 'N/A'}</td>
                  <td>{entry.newValue || 'N/A'}</td>
                  <td>{entry.changedByUserEmail || entry.changedByUserId?.substring(0, 8) || 'N/A'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : (
        <p className="no-data-message">No audit history found for this user.</p>
      )}
    </div>
  );
};

export default UserAuditHistoryPage;