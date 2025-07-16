import React, { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import type { RootState, AppDispatch } from '../../../app/store';
import {
  fetchProjects,
  createProject,
  updateProject,
  deleteProject,
} from '../store/projectsSlice';
import type { IProject, ICreateProjectRequest, IUpdateProjectRequest } from '../store/ProjectInterfaces';
import { Link } from 'react-router-dom';

import './Css/ProjectsPage.css'; // Imports CSS for styling the page.
import ProjectFormModal from '../../../components/ProjectFormModal'; // Imports the modal component for project forms.

// ProjectsPage Functional Component: Displays and manages projects.
const ProjectsPage: React.FC = () => {
  // Initializes Redux dispatch for async actions.
  const dispatch: AppDispatch = useDispatch();
  // Selects projects, loading state, and error from Redux store.
  const { projects, loading, error } = useSelector((state: RootState) => state.projects);
  // Selects current user from authentication state to determine admin status.
  const { user: currentUser } = useSelector((state: RootState) => state.auth);

  // Determines if the current user has 'Admin' role for conditional rendering.
  const isAdmin = currentUser?.roles?.includes('Admin') || false;

  // State for new project data, used in the creation form.
  const [newProjectData, setNewProjectData] = useState<ICreateProjectRequest>({ name: '', description: '' });

  // State to control visibility of the project form modal.
  const [showProjectFormModal, setShowProjectFormModal] = useState(false);
  // State to hold project data when editing an existing project.
  const [projectToEdit, setProjectToEdit] = useState<IProject | null>(null);

  // States for filtering projects by date range and name.
  const [filterStartDate, setFilterStartDate] = useState('');
  const [filterEndDate, setFilterEndDate] = useState('');
  const [filterName, setFilterName] = useState('');

  // Effect hook to fetch projects based on date filters.
  // Triggers whenever dispatch, filterStartDate, or filterEndDate changes.
  useEffect(() => {
    try {
      dispatch(fetchProjects({
        startDate: filterStartDate || undefined,
        endDate: filterEndDate || undefined,
      }));
    } catch (e) {
      console.error("ProjectsPage: Error caught when dispatching fetchProjects:", e);
    }
  }, [dispatch, filterStartDate, filterEndDate]);

  // handleOpenCreateModal: Prepares and opens the modal for creating a new project.
  const handleOpenCreateModal = () => {
    setProjectToEdit(null); // Clears any project data from a previous edit.
    setShowProjectFormModal(true);
  };

  // handleOpenEditModal: Sets the project to be edited and opens the modal.
  const handleOpenEditModal = (project: IProject) => {
    setProjectToEdit(project);
    setShowProjectFormModal(true);
  };

  // handleCloseProjectFormModal: Closes the modal and resets the project to edit state.
  const handleCloseProjectFormModal = () => {
    setShowProjectFormModal(false);
    setProjectToEdit(null);
  };

  // handleSaveProject: Dispatches create or update actions based on modal context.
  // Shows an alert based on the success or failure of the operation.
  const handleSaveProject = async (projectData: ICreateProjectRequest | IUpdateProjectRequest) => {
    let resultAction;
    if (projectToEdit) {
      resultAction = await dispatch(updateProject({ id: projectToEdit.id, data: projectData as IUpdateProjectRequest }));
      if (updateProject.fulfilled.match(resultAction)) {
        alert(`Project "${projectData.name}" updated successfully!`);
      } else {
        alert(`Failed to update project: ${resultAction.payload || 'Unknown error'}`);
      }
    } else {
      resultAction = await dispatch(createProject(projectData as ICreateProjectRequest));
      if (createProject.fulfilled.match(resultAction)) {
        alert(`Project "${projectData.name}" created successfully!`);
      } else {
        alert(`Failed to create project: ${resultAction.payload || 'Unknown error'}`);
      }
    }
    handleCloseProjectFormModal();
  };

  // handleDeleteProject: Handles project deletion with a confirmation prompt.
  // Displays an alert based on the outcome of the deletion.
  const handleDeleteProject = async (id: string) => {
    if (window.confirm('Are you sure you want to delete this project? This cannot be undone.')) {
      const resultAction = await dispatch(deleteProject(id));
      if (deleteProject.rejected.match(resultAction)) {
        alert(`Failed to delete project: ${resultAction.payload || 'Unknown error'}`);
      } else {
        alert('Project deleted successfully!');
      }
    }
  };

  // handleFilterStartDateChange: Updates the start date filter state.
  const handleFilterStartDateChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFilterStartDate(e.target.value);
  };

  // handleFilterEndDateChange: Updates the end date filter state.
  const handleFilterEndDateChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFilterEndDate(e.target.value);
  };

  // filteredProjects: Filters the projects array based on the name filter.
  // Date filtering is handled by the `fetchProjects` thunk.
  const filteredProjects = projects.filter(project => {
    const nameMatches = filterName
      ? project.name.toLowerCase().includes(filterName.toLowerCase())
      : true; // If filterName is empty, all names match.
    return nameMatches;
  });

  return (
    <div className="projects-container">
      <h1 className="projects-page-title">All Projects</h1>
      <div className="projects-controls-container">
        <div className="projects-filter-section">
          <h3>Filter Projects</h3>
          <div className="filter-controls">
            <div className="form-group">
              <label htmlFor="filterName">Name:</label>
              <input
                type="text"
                id="filterName"
                name="filterName"
                value={filterName}
                onChange={(e) => setFilterName(e.target.value)}
                placeholder="Filter by project name"
                disabled={loading}
              />
            </div>
            <div className="form-group">
              <label htmlFor="filterStartDate">Created After:</label>
              <input
                type="date"
                id="filterStartDate"
                name="filterStartDate"
                value={filterStartDate}
                onChange={handleFilterStartDateChange}
                disabled={loading}
              />
            </div>
            <div className="form-group">
              <label htmlFor="filterEndDate">Created Before:</label>
              <input
                type="date"
                id="filterEndDate"
                name="filterEndDate"
                value={filterEndDate}
                onChange={handleFilterEndDateChange}
                disabled={loading}
              />
            </div>
          </div>
        </div>

        {isAdmin && (
          <div className="create-project-section">
            <h2>Create New Project</h2>
            <button onClick={handleOpenCreateModal} disabled={loading} className="button primary-button">
              Add New Project
            </button>
          </div>
        )}
      </div>
      {/* Section to display available projects */}
      <div className="view-projects-section">
        <h2>Available Projects</h2>
        {loading && filteredProjects.length === 0 && <p className="projects-message">Loading projects...</p>}
        {error && <p className="projects-message error-message">Error: {error}</p>}
        {!loading && filteredProjects.length === 0 && !error ? (
          <p className="projects-message no-projects-found">
            No projects found matching your criteria. {isAdmin ? "Click 'Add New Project' to create one!" : ""}
          </p>
        ) : (
          <ul className="project-list">
            {filteredProjects.map((project) => ( // Renders each filtered project in a list.
              <li key={project.id} className="project-item">
                <h3>{project.name}</h3>
                <p>{project.description || 'No description provided.'}</p>
                <small>Created: {new Date(project.createdAt).toLocaleDateString()}</small>
                <div className="project-actions">
                  {/* Edit/Delete buttons are only visible to admins */}
                  {isAdmin && (
                    <>
                      <button onClick={() => handleOpenEditModal(project)} disabled={loading} className="button action-button edit-button">Edit</button>
                      <button onClick={() => handleDeleteProject(project.id)} disabled={loading} className="button action-button delete-button">Delete</button>
                    </>
                  )}
                  {/* Link to view tasks within a project, accessible to all users */}
                  <Link to={`/projects/${project.id}/tasks`} className="button action-button view-tasks-button">View Tasks</Link>
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>

      {/* Project Form Modal, displayed when needed for create/edit operations */}
      {showProjectFormModal && isAdmin && (
        <ProjectFormModal
          isOpen={showProjectFormModal}
          onClose={handleCloseProjectFormModal}
          onSave={handleSaveProject}
          projectToEdit={projectToEdit}
        />
      )}
    </div>
  );
};

export default ProjectsPage;