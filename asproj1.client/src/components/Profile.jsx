import React, { useEffect, useState } from 'react';
import { FaEdit, FaTrash, FaPlus } from 'react-icons/fa';
import Modal from 'react-modal';
import './css/Profile.css';

const Profile = ({ user }) => {
  const [userData, setUserData] = useState(null);
  const [editedData, setEditedData] = useState({});
  const [isEditing, setIsEditing] = useState(false);

  console.log("USER É: ", user);
  useEffect(() => {
    // Faça uma solicitação HTTP para obter os dados do perfil com base no email
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
      console.log("EDITADO", editedData);

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

      } else {
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


  if (!userData) {
    return <div>Carregando...</div>;
  }

  return (
    <div className="profile-container">
      <div className="spacer" style={{ marginTop: '50px'}} />
      <div className="welcome-container">
        <h1 className="welcome-message">Bem-vindo!</h1>
      </div>
      <div className="spacer" style={{ marginBottom: '20px'}} />
      {userData.userType === 'Client' && (
        <div>
          <button className="add-data-button right" onClick={handleAddData}>
            <FaPlus /> Adicionar Dados
          </button>
          <div className="spacer" style={{ marginBottom: '80px'}} />
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
                    <td>{report.diagnosisDetails}</td>
                    <td>{report.treatmentPlan}</td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      )}
      {userData.userType === 'Helpdesk' && (
        <div>
          <table className='table table-striped custom-table'>
            <thead>
              <tr>
                <th>Name</th>
                <th>Email</th>
                <th>Phone number</th>
                <th>MedicalRecordNumber</th>
                <th>Diagnosis Details</th>
                <th>Treatment Plan</th>
                <th>Options</th>
              </tr>
            </thead>
            <tbody>
              {Array.isArray(userData.clientData) && userData.clientData.length > 0 ? (
                userData.clientData.map(client => (
                <React.Fragment key={`client-${client.clientId}`}>
                  <tr>
                    <td>{client.fullName}</td>
                    <td>{client.emailAddress}</td>
                    <td>{client.phoneNumber}</td>
                    <td>{client.medicalRecordNumber}</td>
                    {Array.isArray(client.medReports) && client.medReports.length > 0 ? (
                      client.medReports.map(report => (
                        <React.Fragment key={`report-${report.medReportId}`}>
                          <td>{report.diagnosisDetails}</td>
                          <td>{report.treatmentPlan}</td>
                        </React.Fragment>
                      ))
                    ) : (
                      // Se não houver dados médicos, exibir células vazias
                      <React.Fragment>
                        <td colSpan="2"></td>

                      </React.Fragment>
                    )}
                    <td>
                      <button onClick={() => handleEdit(client.clientId)}><FaEdit /></button>
                    </td>
                  </tr>
                </React.Fragment>
              ))
            ) : (
            <tr>
              <td colSpan="7">Sem dados de clientes disponíveis</td>
            </tr>
          )}
        </tbody>
      </table>
      </div>
        )}
      </div>
  );
};

export default Profile;
