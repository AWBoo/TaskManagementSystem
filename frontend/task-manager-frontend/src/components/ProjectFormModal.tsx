import React, { useState, useEffect } from 'react';
import type { IProject, ICreateProjectRequest, IUpdateProjectRequest } from '../features/projects/store/ProjectInterfaces';
import './Css/ProjectFormModal.css';

interface ProjectFormModalProps {
  isOpen: boolean;
  onClose: () => void;
  // Callback function to save project data,
  // accepting either create or update request types.
  onSave: (projectData: ICreateProjectRequest | IUpdateProjectRequest) => Promise<void>;
  // Optional prop: if provided, the modal is in edit mode for this project.
  projectToEdit?: IProject | null;
}

const ProjectFormModal: React.FC<ProjectFormModalProps> = ({
  isOpen,
  onClose,
  onSave,
  projectToEdit,
}) => {
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  // State to hold validation errors, keyed by field name.
  const [errors, setErrors] = useState<{ [key: string]: string }>({});

  // Effect to populate the form fields when a project is selected for editing,
  useEffect(() => {
    if (isOpen) {
      if (projectToEdit) {
        setName(projectToEdit.name);
        setDescription(projectToEdit.description || ''); // Ensure description is a string
      } else {
        // Clear form for a new project entry.
        setName('');
        setDescription('');
      }
      setErrors({}); // Clear any previous validation errors when modal opens.
    }
  }, [isOpen, projectToEdit]);

  // If the modal is not open, render nothing.
  if (!isOpen) return null;

  // Validates the form inputs and updates the errors state.
  const validateForm = () => {
    const newErrors: { [key: string]: string } = {};
    if (!name.trim()) {
      newErrors.name = 'Project name is required.';
    }
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0; // Returns true if no errors.
  };

  // Handles the form submission, performing validation before calling onSave.
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validateForm()) {
      return; // Stop if validation fails.
    }

    const baseProjectData = {
      name,
      // Only include description if it's not an empty string after trimming.
      description: description.trim() === '' ? undefined : description,
    };

    await onSave(baseProjectData);
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <h3 className="modal-title">{projectToEdit ? 'Edit Project' : 'Create New Project'}</h3>
        <form onSubmit={handleSubmit} className="project-form">
          <div className="form-group">
            <label htmlFor="projectName">Project Name:</label>
            <input
              type="text"
              id="projectName"
              name="name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              className={errors.name ? 'input-error' : ''} // Apply error styling if validation fails.
              required
            />
            {errors.name && <span className="error-message">{errors.name}</span>}
          </div>

          <div className="form-group">
            <label htmlFor="projectDescription">Description (Optional):</label>
            <textarea
              id="projectDescription"
              name="description"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              rows={3}
            ></textarea>
          </div>

          <div className="modal-actions">
            <button type="button" className="button cancel-button" onClick={onClose}>
              Cancel
            </button>
            <button type="submit" className="button primary-button">
              {projectToEdit ? 'Save Changes' : 'Create Project'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default ProjectFormModal;