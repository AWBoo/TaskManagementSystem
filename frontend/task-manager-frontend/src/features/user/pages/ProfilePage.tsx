import React, { useEffect, useState, type FormEvent } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useSelector, useDispatch } from 'react-redux';
import type { RootState, AppDispatch } from '../../../app/store';
import {
  fetchMyProfile,
  fetchUserProfileById,
  updateMyProfile,
  adminUpdateUserProfile,
  changeMyPassword,
  adminChangeUserPassword,
  clearProfileError,
  type IUpdateProfileRequest,
  type IChangePasswordRequest,
  type IAdminChangePasswordRequest
} from '../store/profileSlice';

import PasswordChangeModal from '../../../components/PasswordChangeModal'; 

import './Css/ProfilePage.css'; // Imports CSS for styling the profile page.

// ProfilePage Functional Component: Manages user profile viewing and editing.
const ProfilePage: React.FC = () => {
  // Extracts userId from URL parameters, if present.
  const { userId: urlUserId } = useParams<{ userId?: string }>();
  const navigate = useNavigate(); // Hook for programmatic navigation.
  const dispatch: AppDispatch = useDispatch(); // Initializes Redux dispatch.

  // Selects profile data, loading state, and error from the Redux store.
  const { profile, loading, error } = useSelector((state: RootState) => state.profile);
  // Selects the current authenticated user to determine roles and ID.
  const { user: currentUser } = useSelector((state: RootState) => state.auth) as { user: any };

  // Determines if the current user has 'Admin' role.
  const isAdmin = currentUser?.roles?.includes('Admin') || false;
  // Gets the ID of the currently logged-in user.
  const currentLoggedInUserId = currentUser?.id as string;

  // Determines if the page is displaying the logged-in user's profile or another user's profile.
  const isMyProfile = !urlUserId || urlUserId === currentLoggedInUserId;
  // Sets the target user ID based on whether it's 'my profile' or an admin viewing another user.
  const targetUserId = isMyProfile ? currentLoggedInUserId : urlUserId;
  // Checks if an admin is currently viewing and editing another user's profile.
  const isEditingAnotherUser = isAdmin && !isMyProfile;

  // States for profile form fields.
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  // State to control the visibility of the password change modal.
  const [showPasswordModal, setShowPasswordModal] = useState(false);
  // State for displaying success or error messages after form submissions.
  const [formMessage, setFormMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  // Effect hook to fetch profile data when component mounts or targetUserId/permissions change.
  useEffect(() => {
    if (!targetUserId) {
      // Redirects to login if no user ID context is available.
      navigate('/login');
      return;
    }

    if (isMyProfile) {
      // Fetches the current user's profile.
      dispatch(fetchMyProfile());
    } else if (isAdmin && targetUserId) {
      // Fetches another user's profile if the current user is an admin.
      dispatch(fetchUserProfileById(targetUserId));
    } else {
      // Redirects if attempting to access another user's profile without admin rights.
      navigate('/unauthorized');
    }

    // Cleanup function: clears errors and messages on component unmount.
    return () => {
      dispatch(clearProfileError());
      setFormMessage(null);
    };
  }, [dispatch, targetUserId, isMyProfile, isAdmin, navigate]);

  // Effect hook to populate form fields when profile data is loaded into Redux state.
  useEffect(() => {
    if (profile) {
      setName(profile.name || ''); // Sets name, defaults to empty string if null.
      setEmail(profile.email || ''); // Sets email, defaults to empty string if null.
    }
  }, [profile]);

  // handleProfileSubmit: Handles the submission of the profile update form.
  const handleProfileSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setFormMessage(null); // Clears any existing messages.

    if (!targetUserId) return; // Exits if no target user ID.

    // Prepares the profile data for the update request.
    const profileData: IUpdateProfileRequest = {
      email,
      name,
    };

    let resultAction;
    if (isMyProfile) {
      // Dispatches action to update current user's profile.
      resultAction = await dispatch(updateMyProfile(profileData));
    } else {
      // Dispatches action for admin to update another user's profile.
      resultAction = await dispatch(adminUpdateUserProfile({ userId: targetUserId, profileData }));
    }

    // Displays success or error message based on the action's outcome.
    if (updateMyProfile.fulfilled.match(resultAction) || adminUpdateUserProfile.fulfilled.match(resultAction)) {
      setFormMessage({ type: 'success', text: 'Profile updated successfully!' });
    } else {
      const errorMessage = resultAction.payload as string || 'Failed to update profile.';
      setFormMessage({ type: 'error', text: errorMessage });
    }
  };

  // handleChangePassword: Handles password change requests.
  const handleChangePassword = async (passwordData: IChangePasswordRequest | IAdminChangePasswordRequest) => {
    setFormMessage(null); // Clears any existing messages.
    if (!targetUserId) return; // Exits if no target user ID.

    let resultAction;
    if (isMyProfile) {
      // Dispatches action for current user to change their password.
      resultAction = await dispatch(changeMyPassword(passwordData as IChangePasswordRequest));
    } else {
      // Dispatches action for admin to change another user's password.
      resultAction = await dispatch(adminChangeUserPassword({ userId: targetUserId, passwordData: passwordData as IAdminChangePasswordRequest }));
    }

    // Displays success or error message and closes modal on success.
    if (changeMyPassword.fulfilled.match(resultAction) || adminChangeUserPassword.fulfilled.match(resultAction)) {
      setFormMessage({ type: 'success', text: 'Password changed successfully!' });
      setShowPasswordModal(false); // Closes the modal upon successful password change.
    } else {
      const errorMessage = resultAction.payload as string || 'Failed to change password.';
      setFormMessage({ type: 'error', text: errorMessage });
    }
  };

  // Renders loading message while profile data is being fetched.
  if (loading) {
    return <div className="profile-message">Loading profile...</div>;
  }

  // Renders error message if fetching profile failed.
  if (error) {
    return <div className="profile-message error-message">Error: {error}</div>;
  }

  // Renders message if no profile data is found after loading.
  if (!profile && !loading) {
    return <div className="profile-message">No profile data found.</div>;
  }

  // JSX structure for the Profile Page component.
  return (
    <div className="profile-page-container">
      <h1 className="profile-page-title">
        {isMyProfile ? 'My Profile' : `Edit User: ${profile?.name} `}
      </h1>
      <h1 className="profile-page-title">
        {`(ID: ${targetUserId})`}
      </h1>

      {formMessage && (
        <div className={`form-message ${formMessage.type === 'success' ? 'success' : 'error'}`}>
          {formMessage.text}
        </div>
      )}

      <form className="profile-form" onSubmit={handleProfileSubmit}>
        <div className="form-group">
          <label htmlFor="firstName">First Name:</label>
          <input
            type="text"
            id="Name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="Enter first name"
          />
        </div>
        <div className="form-group">
          <label htmlFor="email">Email:</label>
          <input
            type="email"
            id="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="Enter email"
            required
          />
        </div>

        <button type="submit" className="button primary-button">Update Profile</button>
        <button type="button" className="button secondary-button" onClick={() => setShowPasswordModal(true)}>
          {isMyProfile ? 'Change My Password' : 'Change User Password'}
        </button>
      </form>

      <PasswordChangeModal
        isOpen={showPasswordModal}
        onClose={() => setShowPasswordModal(false)}
        onSave={handleChangePassword}
        isMyProfile={isMyProfile} // Passes prop to modal to determine its behavior.
      />
    </div>
  );
};

export default ProfilePage;