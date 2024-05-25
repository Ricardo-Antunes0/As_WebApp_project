import React, { useEffect, useState } from 'react';
import { FaEdit, FaTrash, FaPlus } from 'react-icons/fa';
import Modal from 'react-modal';
import './css/Profile.css';

const ClientProfile = ({ user, onLogout }) => {
  const [userData, setUserData] = useState(null);
  const [editedData, setEditedData] = useState({});
  const [isEditing, setIsEditing] = useState(false);

  useEffect(() => {
    fetch(`https://localhost:7095/api/Profile?email=${user.email}`)
      .then(response => response.json())
      .then(data => {
        console.log("DATA: ", data);
        setUserData(data);
      })
      .catch(error => console.error('Erro ao obter dados do perfil:', error));
  }, [user.email]);

  console.log("userData: ", userData);

  const handleEdit = (client) => {
    setEditedData({ ...client });
    setIsEditing(true);
  };

  const handleSave = async () => {
    try {
      console.log("Dados do cliente editados: ", editedData);

      const response = await fetch('https://localhost:7095/api/Profile', {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(editedData),
      });

  
      if (response.ok) {
        setUserData((prevUserData) => {
          const updatedClientData = prevUserData.clientData.map((client) =>
          client.emailAddress === editedData.emailAddress ? editedData : client
        );

          console.log("Dados antigos: ", prevUserData);
          console.log("Dados recentes: ", updatedClientData);
          return {
            ...prevUserData,
            clientData: updatedClientData,
          };
        });
        setIsEditing(false);
        alert("Pedido realizado com sucesso!!")
      } else {
        alert('Erro ao editar dados.');
        console.error('Erro ao salvar os dados.');
      }
    } catch (error) {
      console.error('Erro na solicitação HTTP:', error);
    }
  };

  const handleCancel = () => {
    setEditedData({});
    setIsEditing(false);
  };

 
  const handleAddData = () => {
    console.log('Adicionar dados');
  };


  const handleLogout = () => {
    onLogout();
  };


  if (!userData) {
    return <div>Carregando...</div>;
  }

  //<button onClick={handleLogout}>Logout</button>
  return (
    <div className="profile-container">
      <div className="spacer" style={{ marginTop: '180px'}} />
      <div className="welcome-container">
        <h1 className="welcome-message">Bem-vindo!</h1>
      </div>
      {userData.userType === 'Client' && (
        <div>
         <div className="button-container">
            <button className="add-data-button" onClick={handleAddData}>
              <FaPlus /> Adicionar Dados
            </button>
            <button className="logout-button" onClick={handleLogout}>Logout</button>
          </div>
          <div className='Dados'>
          {isEditing ? (
            <div>
               <div>
                <label>Name:</label>
                <input
                  type="text"
                  value={editedData.fullName}
                  onChange={(e) => setEditedData({ ...editedData, fullName: e.target.value })}
                />
              </div>
              <div>
                <label>Phone Number:</label>
                <input
                  type="text"
                  value={editedData.phoneNumber}
                  onChange={(e) => setEditedData({ ...editedData, phoneNumber: e.target.value })}
                />
              </div>
              <div>
                <button onClick={handleSave}>Save</button>
                <button onClick={handleCancel}>Cancel</button>
              </div>
            </div>
          ) : (
          <div>
            <p><strong>Name:</strong> {userData.clientData[0].fullName}</p>
            <p><strong>Email:</strong> {userData.clientData[0].emailAddress}</p>
            <p><strong>Phone Number:</strong> {userData.clientData[0].phoneNumber}</p>
            <p><strong>Medical Record Number:</strong> {userData.clientData[0].medicalRecordNumber}</p>
            <button onClick={() => handleEdit(userData.clientData[0])}><FaEdit /></button>   
          </div>
          )}
          </div>
          <table className='table table-striped client-table'>
            <thead>
              <tr>
                <th>Diagnosis Details</th>
                <th>Treatment Plan</th>
              </tr>
            </thead>
            <tbody>
            {userData.clientData[0].medReports.length === 0 ? (
                <tr>
                  <td colSpan="3">Não há registros médicos disponíveis.</td>
                </tr>
              ) : (
                userData.clientData[0].medReports.map(report => (
                  <tr key={report.medReportId}>
                    <td>{report.diagnosisDetails || 'N/A'}</td>
                    <td>{report.treatmentPlan || 'N/A'}</td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      )}
      </div>
    )};
     

export default ClientProfile;
