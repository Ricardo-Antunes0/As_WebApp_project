// src/components/Register.jsx
import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import './css/Register.css';

const Register = ({ onRegister }) => {
  const [Name, setNome] = useState('');
  const [MedicalRecordNumber, setMedicalRecordNumber] = useState('');
  const [PhoneNumber, setPhoneNumber] = useState('');
  const [Email, setEmail] = useState('');
  const [Password, setPassword] = useState('');
  const [AcessCode, setAcessCode] = useState('');
  const navigate = useNavigate();
  const userType = "Client"

  const handleRegister = async () => {
    try {
      const response = await fetch('https://localhost:7095/api/Register', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ Name, MedicalRecordNumber, PhoneNumber, Email, Password, userType, AcessCode}),
      });

      if (response.status === 200) {
        const user = await response.json();
        if(user.email){
          console.log(user);
          onRegister(user);
          if (user.userType === 'Client') {
            navigate('/client-profile', { state: { user } });
          } else if (user.userType === 'Helpdesk') {
            navigate('/helpdesk-profile', { state: { user } });
          } else {
            console.error('Tipo de usuário desconhecido:', user.userType);
          }
        } else {
          alert('E-mail já está em uso. Por favor, use outro e-mail.');
        }
      } else {
        console.error('Erro ao fazer o registro:', response.statusText);
      }
    } catch (error) {
      console.error('Erro ao fazer o registro:', error);
    }
  };

  return (
    <div className="register-container">
    <div className="register-box">
      <h2>Registo</h2>
      <form>
        <label>Nome completo:</label>
        <input type="nome" placeholder="Digite o seu nome" onChange={(e) => setNome(e.target.value)} />
        <label>Medical Record Number:</label>
        <input type="MRN" placeholder="Digite o seu Medical Record Number: " onChange={(e) => setMedicalRecordNumber(e.target.value)} />
        <label>Telefone:</label>
        <input type="PhoneNumber" placeholder="Digite o seu número de telefone: " onChange={(e) => setPhoneNumber(e.target.value)} />
        <label>Código de acesso:</label>
        <input type="AcessCode" placeholder="Digite o seu código de acesso: " onChange={(e) => setAcessCode(e.target.value)} />
        <label>Email:</label>
        <input type="email" placeholder="Digite o seu email" onChange={(e) => setEmail(e.target.value)} />
        <label>Password:</label>
        <input type="password" placeholder="Digite sua senha" onChange={(e) => setPassword(e.target.value)} />
        <button type="button" onClick={handleRegister}>Registrar</button>
      </form>
      <p className="login-link">Já tem conta?<Link to="/login"> Clique aqui </Link></p>
    </div>
    </div>
  );
};

export default Register;



