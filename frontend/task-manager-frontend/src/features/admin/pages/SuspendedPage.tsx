import React from 'react';
import './Css/SuspendedPage.css'; 

const SuspendedPage: React.FC = () => {
  return (
    <div className="status-page suspended-page">
      <h1 className="status-title">⚠️ Account Suspended</h1>
      <p className="status-message">
        Your account has been temporarily suspended. This means you currently cannot access certain features or the full application.
      </p>
      <p className="status-message">
        Please check your email for more details regarding the reason for suspension and any steps you might need to take to reactivate your account.
      </p>
      <a
        href="mailto:DontSendAnything@ItsnotGoingThoughBuddy.com" 
        className="status-button suspended-button"
      >
        Contact Support
      </a>
      <p className="status-footer-message">
        You have been logged out.
      </p>
    </div>
  );
};

export default SuspendedPage;