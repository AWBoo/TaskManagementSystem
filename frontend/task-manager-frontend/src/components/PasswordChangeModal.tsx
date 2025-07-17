import React, { useState, type FormEvent } from 'react';
import type { IChangePasswordRequest, IAdminChangePasswordRequest } from '../features/user/store/profileSlice';
import './Css/PasswordChangeModal.css';

interface PasswordChangeModalProps {
  isOpen: boolean;
  onClose: () => void;
  // Function to save password changes, handling both user and admin requests.
  // Now explicitly stating it can throw an error or return a success.
  onSave: (passwordData: IChangePasswordRequest | IAdminChangePasswordRequest) => Promise<any>;
  // Flag to determine if 'current password' field should be displayed (for user's own profile).
  isMyProfile: boolean;
}

const PasswordChangeModal: React.FC<PasswordChangeModalProps> = ({ isOpen, onClose, onSave, isMyProfile }) => {
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmNewPassword, setConfirmNewPassword] = useState('');
  const [errorMessage, setErrorMessage] = useState('');
  const [successMessage, setSuccessMessage] = useState(''); 
  const [isLoading, setIsLoading] = useState(false); 

  // Handles the form submission for password change.
  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setErrorMessage(''); // Clear previous error messages
    setSuccessMessage(''); // Clear previous success messages
    setIsLoading(true); // Set loading state

    // Validate if new password and confirmation match.
    if (newPassword !== confirmNewPassword) {
      setErrorMessage('New password and confirmation do not match.');
      setIsLoading(false); // Reset loading state
      return;
    }

    // Handle password change for the logged-in user's own profile.
    if (isMyProfile) {
      if (!currentPassword) {
        setErrorMessage('Current password is required.');
        setIsLoading(false); // Reset loading state
        return;
      }
      // Basic client-side validation for new password length.
      if (newPassword.length < 6) {
        setErrorMessage('New password must be at least 6 characters long.');
        setIsLoading(false); // Reset loading state
        return;
      }
      try {
        await onSave({ currentPassword, newPassword, confirmNewPassword });
        setSuccessMessage('Password changed successfully!');
        setTimeout(() => {
          onClose();
          // Reset form fields after successful submission
          setCurrentPassword('');
          setNewPassword('');
          setConfirmNewPassword('');
        }, 2000);
      } catch (error: any) {
        // Assuming your onSave function throws an error object with a 'message' property
        setErrorMessage(error.message || 'Failed to change password. Please try again.');
      } finally {
        setIsLoading(false); // Reset loading state
      }
    } else {
      // Handle password change by an admin for another user.
      // Basic client-side validation for new password length.
      if (newPassword.length < 6) {
        setErrorMessage('New password must be at least 6 characters long.');
        setIsLoading(false); // Reset loading state
        return;
      }
      try {
        await onSave({ newPassword, confirmNewPassword });
        setSuccessMessage('User password changed successfully!');
        // Optionally close modal after a short delay or clear fields
        setTimeout(() => {
          onClose();
          // Reset form fields after successful submission
          setNewPassword('');
          setConfirmNewPassword('');
        }, 2000);
      } catch (error: any) {
        setErrorMessage(error.message || 'Failed to change user password. Please try again.');
      } finally {
        setIsLoading(false); // Reset loading state
      }
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
                disabled={isLoading} // Disable input while loading
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
              disabled={isLoading} // Disable input while loading
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
              disabled={isLoading} // Disable input while loading
            />
          </div>
          {errorMessage && <p className="error-message">{errorMessage}</p>}
          {successMessage && <p className="success-message">{successMessage}</p>} {/* Display success message */}
          <div className="modal-actions">
            <button type="submit" className="button primary-button" disabled={isLoading}>
              {isLoading ? 'Saving...' : 'Save'}
            </button>
            <button type="button" className="button secondary-button" onClick={onClose} disabled={isLoading}>
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default PasswordChangeModal;