// src/components/Login.jsx
import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import './css/Login.css';

const Login = ({ onLogin }) => {
  const [Email, setEmail] = useState('');
  const [Password, setPassword] = useState('');
  const navigate = useNavigate();

  const handleLogin = async () => {
    try {
      const response = await fetch('https://localhost:7095/api/Login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ Email, Password }),
      });

      if (response.status === 200) {
        const user = await response.json();
        console.log(user);

        if(user.email){
          console.log(user);
          onLogin(user);
          if (user.userType === 'Client') {
            navigate('/client-profile', { state: { user } });
          } else if (user.userType === 'Helpdesk') {
            navigate('/helpdesk-profile', { state: { user } });
          } else {
            console.error('Tipo de usuário desconhecido:', user.userType);
          }
        }
        else{
          alert('Falha na autenticação. Verifique as suas credenciais.');
        }
      } else {
        console.error('Erro ao fazer login');
      }
    } catch (error) {
      console.error('Erro ao fazer login:', error);
    }
  };

  return (
    <div className="login-container">
      <div className="login-box">
        <h2>Login</h2>
        <form onSubmit={(e) => { e.preventDefault(); handleLogin(); }}>
          <label>Email:</label>
          <input type="email" placeholder="Digite seu email" onChange={(e) => setEmail(e.target.value)} />
          <label>Password:</label>
          <input type="password" placeholder="Digite sua senha" onChange={(e) => setPassword(e.target.value)} />
          <button type="submit">Login</button>
        </form>
        <p className="register-link">Não tem uma conta? <Link to="/register">Registre-se aqui</Link></p>
      </div>
    </div>
  );
};

export default Login;
