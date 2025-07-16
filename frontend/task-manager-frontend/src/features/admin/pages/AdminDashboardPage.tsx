import React, { useEffect, useState } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { useNavigate } from 'react-router-dom';
import type { RootState, AppDispatch } from '../../../app/store';
import {
  fetchDashboardStats,
  fetchTaskStatusCounts,
  fetchUserTaskCounts,
  fetchProjectTaskCounts,
  fetchAllRoles,
  fetchLatestAuditEntries,
  type AuditEntry,
} from '../store/adminSlice';
import {
  fetchAllUsers,
  assignRoleToUser,
  removeRoleFromUser,
  deleteUser,
  updateUserStatus,
  type IUserBasic,
} from '../../user/store/usersSlice';
import {
  PieChart, Pie, Cell,
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer
} from 'recharts';
import './Css/AdminDashboardPage.css';

// Colors for the Recharts PieChart.
const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#AF19FF', '#FF19A6'];
// Predefined user statuses for dropdowns.
const USER_STATUSES = ["Active", "Deactivated", "Suspended"];

// Hardcoded data for a humorous "PC Crash" Pie Chart.
const pcCrashData = [
  { name: 'Blue Screens of Death', value: 1 },
  { name: 'Browser Deaths', value: 30 },
  { name: 'Black Screens of Mystery', value: 8 },
  { name: 'Cups of Water drank', value: 5 },
  { name: 'Blinkers Hit', value: 50 },
  { name: 'Times Rebooted', value: 21 },
];

// Custom colors for the PC Crash Pie Chart.
const PC_CRASH_COLORS = [
  '#0509fcff',
  '#f4ff58ff',
  '#020202ff',
  '#A0522D',
  '#696969',
  '#ab1c1cff'
];

const AdminDashboardPage: React.FC = () => {
  const dispatch: AppDispatch = useDispatch();
  const navigate = useNavigate();

  // Select relevant state from the Redux store for admin data and loading status.
  const {
    dashboardStats,
    taskStatusCounts,
    userTaskCounts,
    projectTaskCounts,
    roles,
    loading,
    error,
    latestAuditEntries,
    auditLoading,
    auditError,
  } = useSelector((state: RootState) => state.admin);

  // Select user data and their loading/error states.
  const { users, loading: usersLoading, error: usersError } = useSelector((state: RootState) => state.users);

  // State for controlling the delete user confirmation modal.
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [userToDelete, setUserToDelete] = useState<IUserBasic | null>(null);

  // State for controlling the role management modal.
  const [showRoleModal, setShowRoleModal] = useState(false);
  const [userToManageRoles, setUserToManageRoles] = useState<IUserBasic | null>(null);
  const [selectedRole, setSelectedRole] = useState('');
  const [roleActionType, setRoleActionType] = useState<'assign' | 'remove' | null>(null);

  // Effect to fetch all necessary dashboard data on component mount.
  useEffect(() => {
    dispatch(fetchDashboardStats());
    dispatch(fetchTaskStatusCounts());
    dispatch(fetchUserTaskCounts());
    dispatch(fetchProjectTaskCounts());
    dispatch(fetchAllUsers());
    dispatch(fetchAllRoles());
    dispatch(fetchLatestAuditEntries(10)); // Fetch the last 10 audit entries.
  }, [dispatch]);

  // Handler to open the delete confirmation modal for a specific user.
  const handleDeleteClick = (user: IUserBasic) => {
    setUserToDelete(user);
    setShowDeleteModal(true);
  };

  // Confirms and dispatches the action to delete a user.
  const confirmDeleteUser = async () => {
    if (userToDelete) {
      const resultAction = await dispatch(deleteUser(userToDelete.id));
      if (deleteUser.fulfilled.match(resultAction)) {
        alert(`User ${userToDelete.email} deleted successfully.`);
      } else {
        alert(`Failed to delete user: ${resultAction.payload || 'Unknown error'}`);
      }
      setShowDeleteModal(false);
      setUserToDelete(null);
    }
  };

  // Handler to open the role management modal for a specific user.
  const handleManageRolesClick = (user: IUserBasic) => {
    setUserToManageRoles(user);
    setSelectedRole(''); // Reset selected role when opening.
    setShowRoleModal(true);
  };

  // Handles assigning or removing a role for a user.
  const handleRoleAction = async () => {
    if (userToManageRoles && selectedRole && roleActionType) {
      let resultAction;
      if (roleActionType === 'assign') {
        resultAction = await dispatch(assignRoleToUser({ userId: userToManageRoles.id, roleName: selectedRole }));
        if (assignRoleToUser.fulfilled.match(resultAction)) {
          alert(`Role '${selectedRole}' assigned to ${userToManageRoles.email}.`);
        } else {
          alert(`Failed to assign role: ${resultAction.payload || 'Unknown error'}`);
        }
      } else if (roleActionType === 'remove') {
        resultAction = await dispatch(removeRoleFromUser({ userId: userToManageRoles.id, roleName: selectedRole }));
        if (removeRoleFromUser.fulfilled.match(resultAction)) {
          alert(`Role '${selectedRole}' removed from ${userToManageRoles.email}.`);
        } else {
          alert(`Failed to remove role: ${resultAction.payload || 'Unknown error'}`);
        }
      }
      setShowRoleModal(false);
      setUserToManageRoles(null);
      setSelectedRole('');
      setRoleActionType(null);
    }
  };

  // Handles changing a user's status.
  const handleStatusChange = async (userId: string, newStatus: string) => {
    // Using window.confirm as a temporary placeholder for a custom modal.
    if (window.confirm(`Are you sure you want to change status to '${newStatus}' for this user?`)) {
      const resultAction = await dispatch(updateUserStatus({ userId, newStatus }));
      if (updateUserStatus.fulfilled.match(resultAction)) {
        alert(`User status updated to '${newStatus}'.`);
      } else {
        alert(`Failed to update status: ${resultAction.payload || 'Unknown error'}`);
      }
    }
  };

  // Navigates to a user's tasks page.
  const handleViewTasksClick = (user: IUserBasic) => {
    navigate(`/admin/users/${user.id}/tasks`);
  };

  // Navigates to a user's profile page.
  const handleViewProfileClick = (userId: string) => {
    navigate(`/admin/users/${userId}/profile`);
  };

  // Navigates to a user's specific audit log page.
  const handleViewUserAuditLogClick = (userId: string) => {
    navigate(`/admin/users/${userId}/audit`);
  };

  // Display loading message if any data is still being fetched.
  if (loading || usersLoading || auditLoading) {
    return <div className="admin-dashboard-message">Loading admin dashboard data...</div>;
  }

  // Display error message if any data fetching failed.
  if (error || usersError || auditError) {
    return <div className="admin-dashboard-message error-message">Error: {error || usersError || auditError}</div>;
  }

  // Prepare data for the Task Status Pie Chart.
  const pieChartData = taskStatusCounts.map(item => ({
    name: item.status,
    value: item.count,
  }));

  // Prepare data for the Tasks per User Bar Chart.
  const userBarChartData = userTaskCounts.map(item => ({
    name: item.userEmail.split('@')[0], // Use part of email for shorter labels.
    tasks: item.taskCount,
  }));

  // Prepare data for the Tasks per Project Bar Chart.
  const projectBarChartData = projectTaskCounts.map(item => ({
    name: item.projectName,
    tasks: item.taskCount,
  }));

  // Helper function to format timestamps for display.
  const formatTimestamp = (timestamp: string) => {
    return new Date(timestamp).toLocaleString();
  };

  return (
    <div className="admin-dashboard-container">
      <h1 className="admin-dashboard-title">Admin Dashboard</h1>

      {/* Overview Statistics Section */}
      <section className="dashboard-section">
        <h2 className="section-title">Overview Statistics</h2>
        <div className="stats-grid">
          <div className="stat-card stat-card-users">
            <h3>Total Users</h3>
            <p>{dashboardStats?.totalUsers ?? '0'}</p>
          </div>
          <div className="stat-card stat-card-projects">
            <h3>Total Projects</h3>
            <p>{dashboardStats?.totalProjects ?? '0'}</p>
          </div>
          <div className="stat-card stat-card-tasks">
            <h3>Total Tasks</h3>
            <p>{dashboardStats?.totalTasks ?? '0'}</p>
          </div>
        </div>
      </section>

      {/* Charts Section */}
      <section className="dashboard-section">
        <h2 className="section-title">Data Visualizations</h2>
        <div className="charts-grid">
          {/* Task Status Pie Chart */}
          <div className="chart-card">
            <h3 className="chart-title">Tasks by Status</h3>
            {pieChartData.length > 0 ? (
              <ResponsiveContainer width="100%" height={300}>
                <PieChart>
                  <Pie
                    data={pieChartData}
                    cx="50%"
                    cy="50%"
                    labelLine={false}
                    outerRadius={100}
                    fill="#8884d8"
                    dataKey="value"
                    label={({ name, percent }) => `${name} ${((percent ?? 0) * 100).toFixed(0)}%`}
                  >
                    {pieChartData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Pie>
                  <Tooltip />
                  <Legend />
                </PieChart>
              </ResponsiveContainer>
            ) : (
              <p className="no-data-message">No task status data available.</p>
            )}
          </div>

          {/* Users Task Count Bar Chart */}
          <div className="chart-card">
            <h3 className="chart-title">Tasks per User</h3>
            {userBarChartData.length > 0 ? (
              <ResponsiveContainer width="100%" height={300}>
                <BarChart
                  data={userBarChartData}
                  margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
                >
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="name" />
                  <YAxis />
                  <Tooltip />
                  <Legend />
                  <Bar dataKey="tasks" fill="#82ca9d" />
                </BarChart>
              </ResponsiveContainer>
            ) : (
              <p className="no-data-message">No user task data available.</p>
            )}
          </div>

          {/* Projects Task Count Bar Chart */}
          <div className="chart-card">
            <h3 className="chart-title">Tasks per Project</h3>
            {projectBarChartData.length > 0 ? (
              <ResponsiveContainer width="100%" height={300}>
                <BarChart
                  data={projectBarChartData}
                  margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
                >
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="name" />
                  <YAxis />
                  <Tooltip />
                  <Legend />
                  <Bar dataKey="tasks" fill="#8884d8" />
                </BarChart>
              </ResponsiveContainer>
            ) : (
              <p className="no-data-message">No project task data available.</p>
            )}
          </div>
          {/* Humorous PC Crash Statistics Pie Chart */}
          <div className="chart-card">
            <h3 className="chart-title">PC Crash Statistics (My Dev Journey)</h3>
            {pcCrashData.length > 0 ? (
              <ResponsiveContainer width="100%" height={300}>
                <PieChart>
                  <Pie
                    data={pcCrashData}
                    cx="50%"
                    cy="50%"
                    labelLine={false}
                    outerRadius={100}
                    fill="#8884d8"
                    dataKey="value"
                    label={({ name, percent }) => `${name} ${((percent ?? 0) * 100).toFixed(0)}%`}
                  >
                    {pcCrashData.map((entry, index) => (
                      <Cell key={`cell-pc-crash-${index}`} fill={PC_CRASH_COLORS[index % PC_CRASH_COLORS.length]} />
                    ))}
                  </Pie>
                  <Tooltip />
                  <Legend />
                </PieChart>
              </ResponsiveContainer>
            ) : (
              <p className="no-data-message">Not enough crash data for a meaningful chart... yet.</p>
            )}
            <p className="small-text" style={{ textAlign: 'center', marginTop: '10px' }}>
              *All counts are approximate and highly correlated with complex debugging sessions.
              <br />
              (No actual user data was harmed in the making of this joke.)
            </p>
          </div>

        </div>
      </section>

      {/* All Users Table Section */}
      <section className="dashboard-section">
        <h2 className="section-title">All Users</h2>
        {users.length > 0 ? (
          <div className="table-container">
            <table className="users-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Status</th>
                  <th>Email</th>
                  <th>Roles</th>
                  <th>Projects</th>
                  <th>Tasks</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {users.map((user) => (
                  <tr key={user.id}>
                    <td>{user.name || 'N/A'}</td>
                    <td className="user-status-cell">
                      <span className={`status-badge status-${user.status.toLowerCase().replace(/\s/g, '-')}`}>
                        {user.status}
                      </span>
                      <select
                        className="status-dropdown"
                        value={user.status}
                        onChange={(e) => handleStatusChange(user.id, e.target.value)}
                      >
                        {USER_STATUSES.map(status => (
                          <option key={status} value={status}>{status}</option>
                        ))}
                      </select>
                    </td>
                    <td>{user.email}</td>
                    <td>{user.roles && user.roles.length > 0 ? user.roles.join(', ') : 'No Roles'}</td>
                    <td>{user.projectCount}</td>
                    <td>{user.taskCount}</td>
                    <td className="user-actions">
                      <button className="action-button assign-role-button" onClick={() => handleManageRolesClick(user)}>Manage Roles</button>
                      <button className="action-button view-tasks-button" onClick={() => handleViewTasksClick(user)}>View Tasks</button>
                      <button className="action-button view-profile-button" onClick={() => handleViewProfileClick(user.id)}>View Profile</button>
                      <button
                        className="action-button view-audit-log-button"
                        onClick={() => handleViewUserAuditLogClick(user.id)}
                      >
                        View Audit Log
                      </button>
                      <button className="action-button delete-user-button" onClick={() => handleDeleteClick(user)}>Delete</button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <p className="no-data-message">No users found.</p>
        )}
      </section>

      {/* Recent System Changes (Audit Log) Section */}
      <section className="dashboard-section">
        <h2 className="section-title">Recent System Changes (Last 10 Entries)</h2>
        {latestAuditEntries.length > 0 ? (
          <div className="table-container">
            <table className="audit-table">
              <thead>
                <tr>
                  <th>Timestamp</th>
                  <th>User (ID)</th>
                  <th>Type</th>
                  <th>ID</th>
                  <th>Change</th>
                  <th>Old Value</th>
                  <th>New Value</th>
                </tr>
              </thead>
              <tbody>
                {latestAuditEntries.map((entry) => (
                  <tr key={entry.id}>
                    <td>{formatTimestamp(entry.changeTimestamp)}</td>
                    <td>{entry.changedByUserEmail || entry.changedByUserId || 'N/A'} ({entry.changedByUserId?.substring(0, 8)}...)</td>
                    <td>{entry.entityType}</td>
                    <td>{entry.entityId.substring(0, 8)}...</td>
                    <td>{entry.changeType} ({entry.propertyName || 'N/A'})</td>
                    <td>{entry.oldValue || 'N/A'}</td>
                    <td>{entry.newValue || 'N/A'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <p className="no-data-message">No recent audit entries found.</p>
        )}
      </section>

      {/* Delete User Confirmation Modal */}
      {showDeleteModal && userToDelete && (
        <div className="modal-overlay">
          <div className="modal-content">
            <h3 className="modal-title">Confirm Delete User</h3>
            <p>Are you sure you want to delete user: <strong>{userToDelete.email}</strong>?</p>
            <p className="text-red-500">This action cannot be undone and will delete all associated tasks assigned to this user.</p>
            <div className="modal-actions">
              <button className="button cancel-button" onClick={() => setShowDeleteModal(false)}>Cancel</button>
              <button className="button confirm-delete-button" onClick={confirmDeleteUser}>Delete User</button>
            </div>
          </div>
        </div>
      )}

      {/* Manage Roles Modal */}
      {showRoleModal && userToManageRoles && (
        <div className="modal-overlay">
          <div className="modal-content">
            <h3 className="modal-title">Manage Roles for {userToManageRoles.email}</h3>
            <div className="form-group">
              <label htmlFor="selectRole">Select Role:</label>
              <select
                id="selectRole"
                value={selectedRole}
                onChange={(e) => setSelectedRole(e.target.value)}
              >
                <option value="">-- Choose Role --</option>
                {roles.map((role) => (
                  <option key={role.id} value={role.name}>
                    {role.name}
                  </option>
                ))}
              </select>
            </div>
            <div className="modal-actions role-actions">
              <button
                className="button assign-button"
                onClick={() => { setRoleActionType('assign'); handleRoleAction(); }}
                disabled={!selectedRole || userToManageRoles.roles.includes(selectedRole)}
              >
                Assign Role
              </button>
              <button
                className="button remove-button"
                onClick={() => { setRoleActionType('remove'); handleRoleAction(); }}
                disabled={!selectedRole || !userToManageRoles.roles.includes(selectedRole)}
              >
                Remove Role
              </button>
              <button className="button cancel-button" onClick={() => setShowRoleModal(false)}>Cancel</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default AdminDashboardPage;