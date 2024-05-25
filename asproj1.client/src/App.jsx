// src/App.jsx
import React, { useState } from 'react';
import { BrowserRouter as Router, Route, Routes, Navigate } from 'react-router-dom';
import Login from './components/Login.jsx';
import Register from './components/Register.jsx';
import ClientProfile from './components/ClientProfile.jsx';
import HelpdeskProfile from './components/HelpdeskProfile.jsx';


const App = () => {
  const [loggedInUser, setLoggedInUser] = useState(null);

  const handleLogin = (user) => {
    setLoggedInUser(user);
  };

  const handleRegister = (user) => {
    setLoggedInUser(user);
  };

  const handleLogout = () => {
    setLoggedInUser(null);
  };

  return (
    <Router>
      <Routes>
        <Route
          path="/*"
          element={<Navigate to="/login" />}
        />
        <Route
          path="/login"
          element={<Login onLogin={handleLogin} />}
        />
        <Route 
          path="/register"
          element={<Register onRegister={handleRegister} />} />
        <Route
          path="/client-profile"
          element={ loggedInUser ? (<ClientProfile user={loggedInUser} onLogout={handleLogout} />
            ) : ( <Navigate to="/login" /> )}
        />

        <Route
          path="/helpdesk-profile"
          element={ loggedInUser ? (<HelpdeskProfile user={loggedInUser} onLogout={handleLogout} />
            ) : ( <Navigate to="/login" /> )}
        />

      </Routes>
    </Router>
  );
};

export default App;
