import React, { useEffect } from 'react';
import { useSelector, useDispatch } from 'react-redux';
import type { RootState, AppDispatch } from '../app/store';
import { fetchUserDashboardData } from '../features/user/store/usersSlice';
import './css/Home.css';
import { motion } from 'framer-motion';
import { Link } from 'react-router-dom';
import {
  PieChart, Pie, Cell,
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer
} from 'recharts';
import AskAIBot from '../components/AskAIBot';
import type { ITaskStatusCount } from '../features/user/store/DashboardIterface';

// Define a mapping from task status to a specific color.
const TASK_STATUS_COLOR_MAP: { [key: string]: string } = {
  'Not Started': '#63b3ed', // Blue
  'In Progress': '#f6e05e', // Yellow
  'Completed': '#48bb78',    // Green
  'On Hold': '#ed8936',      // Orange
  'Blocked': '#e53e3e',      // Red
};

// Home Functional Component: Displays a personalized dashboard for authenticated users.
const Home: React.FC = () => {
  const dispatch: AppDispatch = useDispatch();
  const isAuthenticated = useSelector((state: RootState) => state.auth.isAuthenticated);
  const user = useSelector((state: RootState) => state.auth.user);

  const userDashboardData = useSelector((state: RootState) => state.users.userDashboardData);
  const dashboardLoading = useSelector((state: RootState) => state.users.dashboardLoading);
  const dashboardError = useSelector((state: RootState) => state.users.dashboardError);

  // Effect hook to fetch user dashboard data when authenticated.
  useEffect(() => {
    if (isAuthenticated && user?.id) {
      dispatch(fetchUserDashboardData());
    }
  }, [isAuthenticated, user?.id, dispatch]);

  // Renders a loading message while dashboard data is being fetched.
  if (dashboardLoading && isAuthenticated) {
    return (
      <div className="home-page-container">
        <div className="home-page-message">
          <p>Loading your personalized dashboard...</p>
        </div>
      </div>
    );
  }

  // Renders an error message if dashboard data fetching fails.
  if (dashboardError && isAuthenticated) {
    return (
      <div className="home-page-container">
        <div className="home-page-message error-message">
          <p>Error loading dashboard: {dashboardError}</p>
        </div>
      </div>
    );
  }

  // Prepares data for the task status pie chart.
  const myTaskStatusPieChartData = userDashboardData?.myTaskStatusCounts || [];
  // Prepares data for the project task bar chart.
  const myProjectTaskBarChartData = userDashboardData?.myProjectTaskCounts || [];

  // Checks if there's any dashboard data to display.
  const hasDashboardData = userDashboardData && (
    userDashboardData.myTotalTasks > 0 ||
    myTaskStatusPieChartData.length > 0 ||
    myProjectTaskBarChartData.length > 0
  );
  // If no dashboard data, show onboarding message for new users.
  return (
    <div className="home-page-container">
      {isAuthenticated ? (
        // Content displayed for authenticated users.
        <div className="home-authenticated-content">
          <motion.h1
            initial={{ opacity: 0, y: -30 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.6 }}
            className="home-title"
          >
            {/* Updated line to show user's name, then email, then 'Project Lead' */}
            Welcome back, {user?.name || user?.email || 'Project Lead'}!
          </motion.h1>
          <motion.p
            initial={{ opacity: 0, y: -20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.2, duration: 0.6 }}
            className="home-welcome-message"
          >
            Ready to drive your tasks forward?
          </motion.p>

          {hasDashboardData ? (
            // Displays dashboard statistics and charts if data is available.
            <>
              {/* Personalized Task Stats Overview */}
              <motion.div
                initial={{ opacity: 0, scale: 0.9 }}
                animate={{ opacity: 1, scale: 1 }}
                transition={{ delay: 0.6, duration: 0.8, type: "spring", stiffness: 100 }}
                className="home-tasks-stats"
              >
                <div className="stat-card">
                  <h3>{userDashboardData?.myTotalTasks ?? '0'}</h3>
                  <p>My Total Tasks</p>
                </div>
                <div className="stat-card">
                  <h3>{userDashboardData?.myTasksDueSoon ?? '0'}</h3>
                  <p>Tasks Due Soon</p>
                </div>
                <div className="stat-card">
                  <h3>{userDashboardData?.myOverdueTasks ?? '0'}</h3>
                  <p>Overdue Tasks</p>
                </div>
              </motion.div>

              {/* User-Specific Charts Section */}
              <section className="dashboard-section">
                <h2 className="section-title">Your Task Insights</h2>
                <div className="charts-grid">
                  {/* My Task Status Pie Chart */}
                  <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: 0.8, duration: 0.7 }}
                    className="chart-card"
                  >
                    <h3 className="chart-title">My Tasks by Status</h3>
                    {myTaskStatusPieChartData.length > 0 && myTaskStatusPieChartData.some(d => d.count > 0) ? (
                      <ResponsiveContainer width="100%" height={300}>
                        <PieChart>
                          <Pie
                            data={myTaskStatusPieChartData}
                            cx="50%"
                            cy="50%"
                            labelLine={false}
                            outerRadius={100}
                            fill="#8884d8"
                            dataKey="count"
                            label={({ payload, percent }) => {
                              const dataEntry = payload as ITaskStatusCount;
                              return `${dataEntry.status} (${dataEntry.count}) ${((percent ?? 0) * 100).toFixed(0)}%`;
                            }}
                          >
                            {myTaskStatusPieChartData.map((entry, index) => (
                              <Cell key={`cell-${index}`} fill={TASK_STATUS_COLOR_MAP[entry.status] || '#cccccc'} />
                            ))}
                          </Pie>
                          <Tooltip />
                          <Legend
                            formatter={(value, entry) => {
                              const dataEntry = entry.payload as ITaskStatusCount;
                              return dataEntry.status; // Displays the status name in the legend.
                            }}
                          />
                        </PieChart>
                      </ResponsiveContainer>
                    ) : (
                      <p className="no-data-message">No task status data available for you.</p>
                    )}
                  </motion.div>

                  {/* My Projects Task Count Bar Chart */}
                  <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ delay: 0.9, duration: 0.7 }}
                    className="chart-card"
                  >
                    <h3 className="chart-title">My Tasks per Project</h3>
                    {myProjectTaskBarChartData.length > 0 && myProjectTaskBarChartData.some(d => d.taskCount > 0) ? (
                      <ResponsiveContainer width="100%" height={300}>
                        <BarChart
                          data={myProjectTaskBarChartData}
                          margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
                        >
                          <CartesianGrid strokeDasharray="3 3" stroke="#4a5568" />
                          <XAxis dataKey="projectName" stroke="#cbd5e0" />
                          <YAxis allowDecimals={false} stroke="#cbd5e0" />
                          <Tooltip contentStyle={{ backgroundColor: '#2d3748', border: 'none', color: '#e2e8f0' }} />
                          <Legend />
                          <Bar dataKey="taskCount" fill="#63b3ed" />
                        </BarChart>
                      </ResponsiveContainer>
                    ) : (
                      <p className="no-data-message">No project task data available for your projects.</p>
                    )}
                  </motion.div>
                </div>
              </section>

              <motion.div
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 1.2, duration: 0.6 }}
                className="home-cta-buttons"
              >
                <Link to="/tasks" className="primary-button-styled">Go to My Tasks</Link>
              </motion.div>
            </>
          ) : (
            // Onboarding message for new users with no data.
            <motion.div
              initial={{ opacity: 0, y: 30 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.6, duration: 0.8 }}
              className="home-onboarding-section"
            >
              <h2 className="onboarding-title">Welcome to Your Project Hub!</h2>
              <p className="onboarding-text">It looks like you haven't been assigned any tasks yet.</p>
              <p className="onboarding-text-small">
                Head over to the Tasks page to view all available tasks or create new ones if you have the permissions.
              </p>
              <motion.div
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ delay: 0.9, duration: 0.7 }}
                className="hero-buttons"
              >
                <Link to="/tasks" className="primary-button-styled">Go to Tasks</Link>
              </motion.div>
            </motion.div>
          )}

          {/* Ask the AI Bot Component */}
          <AskAIBot />

        </div>
      ) : (
        // Content for Unauthenticated Users (Marketing/Landing Page Focus).
        <div className="home-unauthenticated-content">
          <div className="hero-section">
            <motion.h1
              initial={{ opacity: 0, y: -30 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.8 }}
              className="hero-title"
            >
              Master Your Projects. Deliver Success.
            </motion.h1>
            <motion.p
              initial={{ opacity: 0, y: -20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.3, duration: 0.8 }}
              className="home-prompt-message"
            >
              From concept to completion, empower your team and streamline every project phase.
            </motion.p>
            <motion.span
              initial={{ opacity: 0, scale: 0.8 }}
              animate={{ opacity: 1, scale: 1 }}
              transition={{ delay: 0.6, duration: 0.8, type: "spring", stiffness: 100 }}
              className="benefit-icon"
              role="img"
              aria-label="Man and Woman Technologist"
            >
              👩‍💻👨‍💻
            </motion.span>
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.9, duration: 0.7 }}
              className="hero-buttons"
            >
              <Link to="/login" className="primary-button-styled">Get Started</Link>
            </motion.div>
          </div>

          {/* Features Showcase */}
          <div className="features-showcase">
            <h2 className="features-title">How Our Platform Transforms Project Delivery</h2>
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ delay: 0.3, duration: 0.8, staggerChildren: 0.2 }}
              className="feature-cards-grid"
            >
              <motion.div
                className="feature-card"
                initial={{ opacity: 0, scale: 0.8 }}
                animate={{ opacity: 1, scale: 1 }}
                transition={{ duration: 0.6 }}
              >
                <span className="benefit-icon" role="img" aria-label="Clipboard with Checkmark">📋✅</span>
                <h3>Streamlined Project Planning</h3>
                <p>Break down complex projects into manageable tasks, assign roles, and set clear timelines.</p>
              </motion.div>
              <motion.div
                className="feature-card"
                initial={{ opacity: 0, scale: 0.8 }}
                animate={{ opacity: 1, scale: 1 }}
                transition={{ delay: 0.1, duration: 0.6 }}
              >
                <span className="benefit-icon" role="img" aria-label="Handshake">🤝</span>
                <h3>Seamless Team Collaboration</h3>
                <p>Track progress together in a centralized workspace, fostering transparency and accountability.</p>
              </motion.div>
              <motion.div
                className="feature-card"
                initial={{ opacity: 0, scale: 0.8 }}
                animate={{ opacity: 1, scale: 1 }}
                transition={{ delay: 0.2, duration: 0.6 }}
              >
                <span className="benefit-icon" role="img" aria-label="Chart Increasing">📈</span>
                <h3>Intuitive Progress Monitoring</h3>
                <p>Visualize project health with dashboards and other things</p>
              </motion.div>
            </motion.div>
          </div>

          {/* Core Benefits Section */}
          <motion.div
            initial={{ opacity: 0, y: 30 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.5, duration: 0.8 }}
            className="benefits-section-container"
          >
            <h2 className="benefits-title">Achieve Task Excellence</h2>
            <div className="benefit-items-grid">
              <div className="benefit-item">
                <span className="benefit-icon">✨</span>
                <p>
                  Get a clear overview of all your assigned tasks and their statuses.
                </p>
              </div>
              <div className="benefit-item">
                <span className="benefit-icon">🔗</span>
                <p>
                  Keep track of task deadlines and priorities effortlessly.
                </p>
              </div>
              <div className="benefit-item">
                <span className="benefit-icon">🚀</span>
                <p>
                  Focus on what matters most with clear insights into your workload.
                </p>
              </div>
            </div>
          </motion.div>
        </div>
      )}
    </div>
  );
};

export default Home;