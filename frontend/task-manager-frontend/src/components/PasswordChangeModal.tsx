import React, { useState, type FormEvent } from 'react';
import type { IChangePasswordRequest, IAdminChangePasswordRequest } from '../features/user/store/profileSlice';
import './Css/PasswordChangeModal.css';

interface PasswordChangeModalProps {
  isOpen: boolean;
  onClose: () => void;
  // Function to save password changes, handling both user and admin requests.
  onSave: (passwordData: IChangePasswordRequest | IAdminChangePasswordRequest) => Promise<any>;
  // Flag to determine if 'current password' field should be displayed (for user's own profile).
  isMyProfile: boolean;
}

const PasswordChangeModal: React.FC<PasswordChangeModalProps> = ({ isOpen, onClose, onSave, isMyProfile }) => {
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmNewPassword, setConfirmNewPassword] = useState('');
  const [errorMessage, setErrorMessage] = useState('');

  // Handles the form submission for password change.
  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setErrorMessage('');

    // Validate if new password and confirmation match.
    if (newPassword !== confirmNewPassword) {
      setErrorMessage('New password and confirmation do not match.');
      return;
    }

    // Handle password change for the logged-in user's own profile.
    if (isMyProfile) {
      if (!currentPassword) {
        setErrorMessage('Current password is required.');
        return;
      }
      // Basic client-side validation for new password length.
      if (newPassword.length < 6) {
        setErrorMessage('New password must be at least 6 characters long.');
        return;
      }
      await onSave({ currentPassword, newPassword, confirmNewPassword });
    } else {
      // Handle password change by an admin for another user.
      // Basic client-side validation for new password length.
      if (newPassword.length < 6) {
        setErrorMessage('New password must be at least 6 characters long.');
        return;
      }
      await onSave({ newPassword, confirmNewPassword });
    }
  };

  // Do not render the modal if it's not open.
  if (!isOpen) return null;

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <h2>{isMyProfile ? 'Change Your Password' : 'Change User Password'}</h2>
        <form onSubmit={handleSubmit}>
          {isMyProfile && (
            <div className="form-group">
              <label htmlFor="currentPassword">Current Password:</label>
              <input
                type="password"
                id="currentPassword"
                value={currentPassword}
                onChange={(e) => setCurrentPassword(e.target.value)}
                required={isMyProfile}
              />
            </div>
          )}
          <div className="form-group">
            <label htmlFor="newPassword">New Password:</label>
            <input
              type="password"
              id="newPassword"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              required
            />
          </div>
          <div className="form-group">
            <label htmlFor="confirmNewPassword">Confirm New Password:</label>
            <input
              type="password"
              id="confirmNewPassword"
              value={confirmNewPassword}
              onChange={(e) => setConfirmNewPassword(e.target.value)}
              required
            />
          </div>
          {errorMessage && <p className="error-message">{errorMessage}</p>}
          <div className="modal-actions">
            <button type="submit" className="button primary-button">Save</button>
            <button type="button" className="button secondary-button" onClick={onClose}>Cancel</button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default PasswordChangeModal;