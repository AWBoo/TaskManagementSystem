import React from 'react';
import '../components/Css/DeleteConfirmationModal.css';

interface DeleteConfirmationModalProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  message: string;
}

const DeleteConfirmationModal: React.FC<DeleteConfirmationModalProps> = ({
  isOpen,
  onClose,
  onConfirm,
  message,
}) => {
  // Do not render the modal if it's not open.
  if (!isOpen) return null;

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <h3 className="modal-title">Confirm Action</h3>
        <p>{message}</p>
        <p className="text-red-500">This action cannot be undone.</p>
        <div className="modal-actions">
          <button className="button cancel-button" onClick={onClose}>Cancel</button>
          <button className="button confirm-delete-button" onClick={onConfirm}>Delete</button>
        </div>
      </div>
    </div>
  );
};

export default DeleteConfirmationModal;