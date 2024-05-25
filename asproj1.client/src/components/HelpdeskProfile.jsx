import React, { useEffect, useState } from 'react';
import { FaEdit, FaTrash, FaPlus, FaEye } from 'react-icons/fa';
import Modal from 'react-modal';
import './css/Profile.css';

Modal.setAppElement('#root');


const AccessCodeModal = ({ isOpen, onClose, onSubmit, clientId}) => {
    const [accessCode, setAccessCode] = useState('');
  
    const handleSubmit = (e) => {
      e.preventDefault();
      onSubmit(accessCode);
    };
  
    return (
    <Modal isOpen={isOpen} onRequestClose={onClose} contentLabel="Código de Acesso" className="custom-modal">
    <div aria-hidden="true">
        <form onSubmit={handleSubmit}>
        <button className="close-button" onClick={onClose}>
            X
        </button>
        <div className='Label-modal'>
            <label>Código de Acesso:</label>
        </div>
        <div>
            <input type="text" value={accessCode} onChange={(e) => setAccessCode(e.target.value)} />
        </div>
        <div>
            <button type="submit">Enviar</button>
        </div>
        </form>
    </div>
    </Modal>
    );
};


const EditModal = ({ editedData, isOpen, onSave, onCancel, onChange, knowCode, selectedClientId }) => {
  const isFullForm = knowCode && selectedClientId === editedData.clientId;
  
  const renderForm = () => {
    if (isFullForm) {
      return (
        <form>
          <button className="close-button" onClick={onCancel}>
            X
          </button>
          <div className="form-row">
            <label>Name:</label>
            <input
              type="text"
              value={editedData.fullName || ''}
              onChange={(e) => onChange({ ...editedData, fullName: e.target.value })}
            />
          </div>
          <div className="form-row">
            <label>Email:</label>
            <input
              type="text"
              value={editedData.emailAddress || ''}
              onChange={(e) => onChange({ ...editedData, emailAddress: e.target.value })}
            />
          </div>
          <div className="form-row">
            <label>Phone number:</label>
            <input
              type="text"
              value={editedData.phoneNumber || ''}
              onChange={(e) => onChange({ ...editedData, phoneNumber: e.target.value })}
            />
          </div>
          <div className="form-row">
            <label>Medical Record Number:</label>
            <input
              type="text"
              value={editedData.medicalRecordNumber || ''}
              onChange={(e) => onChange({ ...editedData, medicalRecordNumber: e.target.value })}
            />
          </div>
          <div className="form-row">
            <label>Diagnosis Details:</label>
            <input
              type="text"
              value={editedData.medReports[0]?.diagnosisDetails || ''}
              onChange={(e) => {
                const updatedMedReports = [
                  {
                    ...editedData.medReports[0],
                    diagnosisDetails: e.target.value,
                  },
                ];
                onChange({ ...editedData, medReports: updatedMedReports });
              }}
            />
          </div>
          <div className="form-row">
            <label>Treatment plan:</label>
            <input
              type="text"
              value={editedData.medReports[0]?.treatmentPlan || ''}
              onChange={(e) => {
                const updatedMedReports = [
                  {
                    ...editedData.medReports[0],
                    treatmentPlan: e.target.value,
                  },
                ];
                onChange({ ...editedData, medReports: updatedMedReports });
              }}
            />
          </div>
          <div className="form-row">
            <button type="button" onClick={onSave}>
              Salvar
            </button>
          </div>
        </form>
      );
    } else {
      return (
        <form>
          <button className="close-button" onClick={onCancel}>
            X
          </button>
          <div className="form-row">
            <label>Name:</label>
            <input
              type="text"
              value={editedData.fullName || ''}
              onChange={(e) => onChange({ ...editedData, fullName: e.target.value })}
            />
          </div>
          <div className="form-row">
            <label>Medical record number:</label>
            <input
              type="text"
              value={editedData.medicalRecordNumber || ''}
              onChange={(e) => onChange({ ...editedData, medicalRecordNumber: e.target.value })}
            />
          </div>
          <div className="form-row">
            <button type="button" onClick={onSave}>
              Salvar
            </button>
          </div>
        </form>
      );
    }
  };

  return (
    <Modal isOpen={isOpen} onRequestClose={onCancel} contentLabel="Editar Cliente" className="custom-modal">
      <div aria-hidden="true">{renderForm()}</div>
    </Modal>
  );
};



  
  
const HelpdeskProfile = ({ user, onLogout }) => {
  const [userData, setUserData] = useState(null);
  const [editedData, setEditedData] = useState({}); // Dados do cliente que se quer editar
  const [isEditing, setIsEditing] = useState(false);
  const [updatedClientData, setUpdatedClientData] = useState(null);
  const [showAccessCodeModal, setShowAccessCodeModal] = useState(false);
  const [selectedClientId, setSelectedClientId] = useState(null); // Para o botao de ver
  const [knowCode, setKnowCode] = useState(false); // SABER SE O helpdesk sabe o codigo ou nao
  const [accessCodeClientId, setAccessCodeClientId] = useState(null); // ID do cliente que o helpdesk inseriou o codigo


  useEffect(() => {
    fetch(`https://localhost:7095/api/Profile?email=${user.email}`)
      .then(response => response.json())
      .then(data => {
        console.log("DATA: ", data);
        setUserData(data);
      })
      .catch(error => console.error('Erro ao obter dados do perfil:', error));
  }, [user.email]);


  // Funcoes DO MODAL DE EDIÇÃO

  const openEditModal = (client) => {
    // CASO o CODIGO DE ACESSO TENHA SIDO INSERIDO E SEJA CORRETO 
    // E o client que queremos editar seja esse que inserimos o codigo entao
    // o editedData (cliente que é para editar) vai conter os dados nao mascarados
    if (knowCode && client.clientId === updatedClientData.clientId) {
      setEditedData({ ...updatedClientData });
      console.log(editedData);
    } else {
      console.log(editedData);
      // Caso contrario, apresento os dados mascarados
      setEditedData({ ...client });
    }
    setIsEditing(true);
  };
  
  const closeEditModal = () => {
    setEditedData({});
    setIsEditing(false);
  };

  const saveEditedData = async () => {
    // AQUI É PARA AATUALIZAR A BASE DE DADOS
    // QUANDO RECEBER UMA 200 OK ENTAO ATUALIZAR O ESTADO DA UPDATEDcLIENTdATA
    // PARA ASSIM A MINHA TABELA TER OS DADOS DO CLIENTE QUE FOI EDITADO ATUALIZADOS
    try {
        console.log("Dados do cliente editados: ", editedData);
        
        const response = await fetch('https://localhost:7095/api/CheckAccessCode', {
          method: 'PUT',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({userEmail: user.email, editedData: editedData, knowCode: knowCode}),
        });
  
        if (response.ok) {
          setUpdatedClientData(editedData);
          setIsEditing(false);
          alert('Pedido realizado com sucesso!');
        } else {
          alert('Não foi possivel realizar o seu pedido.');
          console.error('Não foi possivel realizar o seu pedido.');
        }
      } catch (error) {
        console.error('Erro na solicitação HTTP:', error);
      }
    closeEditModal();
  };


  // FUNCOES PARA INSERIR O CODIGO DE ACESSO

  const openAccessCodeModal = (clientId) => {
    setKnowCode(false);
    setSelectedClientId(clientId);
    setShowAccessCodeModal(true);
    console.log("setShow: ", showAccessCodeModal);
    console.log("SLEECTED CLIENT ID : ", selectedClientId);
  };

  const closeAccessCodeModal = () => {
    setShowAccessCodeModal(false);
  };
  
  const submitAccessCode = async (code, clientId) => {
    try {
      const response = await fetch(`https://localhost:7095/api/CheckAccessCode`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ userType: user.userType, code: code, clientId: clientId}),
      });
  
      if (response.ok) {
        const clientData = await response.json();
        console.log('Dados do cliente:', clientData);

        setUpdatedClientData(clientData); //ATUALIZAR A VARIAVEL DE ESTADO QUE GUARDA OS DADOS NAO MASCARADOS DO UTILIZADOR QUE O HELPDESK INSERIOU O CODIGO
        setKnowCode(true); // O HELPDESK SABE O CODIGO DO CLIENTE
        setAccessCodeClientId(clientId); // DESTE CLIENTE 
        alert('Código de acesso correto.');
      } else {
        const errorMessage = await response.text();
        console.error(`Erro ao verificar o código de acesso: ${errorMessage}`);
        alert('Código de acesso incorreto!!');
      }
    } catch (error) {
      console.error('Erro na solicitação HTTP:', error);
      alert('Erro ao processar a solicitação. Por favor, tente novamente mais tarde.');
    }
    setShowAccessCodeModal(false);
  };

  const handleLogout = () => {
    onLogout();
  };


  if (!userData) {
    return <div>Carregando...</div>;
  }

  return (
    <div id ="root" className="profile-container">
      <div className="spacer" style={{ marginTop: '50px'}} />
      <div className="welcome-container">
        <h1 className="welcome-message">Bem-vindo!</h1>
      </div>
      <button className='Logout-helpdesk' onClick={handleLogout}>Logout</button>
      <div className="spacer" style={{ marginBottom: '5px'}} />
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
                    <td> {updatedClientData && updatedClientData.clientId === client.clientId
                    ? updatedClientData.fullName : client.fullName}</td>
                    <td>{updatedClientData && updatedClientData.clientId === client.clientId ?
                    updatedClientData.emailAddress : client.emailAddress}</td>
                    <td>{updatedClientData && updatedClientData.clientId === client.clientId ? 
                     updatedClientData.phoneNumber : client.phoneNumber}</td>
                    <td>{updatedClientData && updatedClientData.clientId === client.clientId ?
                    updatedClientData.medicalRecordNumber : client.medicalRecordNumber}</td>
                     {updatedClientData && updatedClientData.clientId === client.clientId && 
                     Array.isArray(updatedClientData.medReports) && updatedClientData.medReports.length > 0 ? (
                      updatedClientData.medReports.map(report => (
                      <React.Fragment key={`report-${report.medReportId}`}>
                        <td>{report.diagnosisDetails || 'N/A'}</td>
                        <td>{report.treatmentPlan || 'N/A'}</td>
                      </React.Fragment>
                    ))
                  ) : (
                    Array.isArray(client.medReports) && client.medReports.length > 0 ? (
                      client.medReports.map(report => (
                        <React.Fragment key={`report-${report.medReportId}`}>
                        <td>{report.diagnosisDetails || 'xxxx'}</td>
                        <td>{report.treatmentPlan || 'xxxx'}</td>
                        </React.Fragment>
                      ))
                    ) : (
                      <React.Fragment>
                        <td colSpan="2"></td>

                      </React.Fragment>
                    )
                    )}
                    <td>
                      <button onClick={() => openEditModal(client)}><FaEdit /></button>
                      <button onClick={() => openAccessCodeModal(client.clientId)} ><FaEye /></button>
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
      <EditModal
        editedData={editedData}
        isOpen={isEditing}
        onSave={saveEditedData}
        onCancel={closeEditModal}
        onChange={setEditedData}
        knowCode={knowCode}
        selectedClientId = {selectedClientId}
      />
      <AccessCodeModal
        isOpen={showAccessCodeModal}
        onClose={closeAccessCodeModal}
        onSubmit={(code) => submitAccessCode(code, selectedClientId)}
      />
    </div>
  );
};

export default HelpdeskProfile;
