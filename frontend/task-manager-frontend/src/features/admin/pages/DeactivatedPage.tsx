import React from 'react';
import './Css/SuspendedPage.css'; // Imports the CSS for styling the deactivated and suspended pages.

const DeactivatedPage: React.FC = () => {
  return (
    <div className="status-page deactivated-page">
      <h1 className="status-title">🚫 Account Deactivated</h1>
      <p className="status-message">
        We're sorry, but your account has been deactivated. This typically occurs due to a violation of our terms of service or a direct request to close your account.
      </p>
      <p className="status-message">
        If you believe this is an error or need further assistance, please contact our support team.
      </p>
      <a
        href="mailto:DONT@Send.Anything" 
        className="status-button deactivated-button"
      >
        Contact Support
      </a>
      <p className="status-footer-message">
        You have been logged out. Please do not attempt to log in with this account.
      </p>
    </div>
  );
};

export default DeactivatedPage;