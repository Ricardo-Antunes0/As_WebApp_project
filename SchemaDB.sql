
	USE ASproj1;

	-- Users.sql
	CREATE TABLE dbo.Users (
		UserId INT PRIMARY KEY IDENTITY(1, 1),
		UserMail NVARCHAR(500) NOT NULL,
		PasswordHash NVARCHAR(256) NOT NULL,
		UserType NVARCHAR(20) NOT NULL -- 'Client' ou 'Helpdesk'
	);

	-- Clients.sql
	CREATE TABLE dbo.Clients (
		ClientId INT PRIMARY KEY identity(1,1),
		FullName NVARCHAR(500),
		EmailAddress NVARCHAR(500),
		PhoneNumber NVARCHAR(9) MASKED WITH (FUNCTION = 'default()'),
		MedicalRecordNumber NVARCHAR(255),
		CONSTRAINT UC_MedicalRecordNumber UNIQUE (MedicalRecordNumber),
		AccessCodeHash NVARCHAR(128)
	);

	CREATE TABLE dbo.MedReport(
		MedReportId INT PRIMARY KEY identity(1,1),
		MedicalRecordNumber NVARCHAR(255),
		 FOREIGN KEY (MedicalRecordNumber)
		REFERENCES dbo.Clients(MedicalRecordNumber)
		ON UPDATE CASCADE,
		DiagnosisDetails NVARCHAR(MAX) MASKED WITH (FUNCTION = 'default()') NULL, -- Mascarado para helpdesk
		TreatmentPlan NVARCHAR(MAX) MASKED WITH (FUNCTION = 'default()') NULL, -- Mascarado para helpdesk
		);	


-- CRIAR SERVICES ACCOUNTS
-- Atribuir algumas permissoes

	USE ASproj1;
	CREATE USER Clientes WITHOUT LOGIN;
	CREATE USER Helpdesks WITHOUT LOGIN;
	GO

	GRANT SELECT ON dbo.Clients TO Clientes;
	GRANT SELECT ON dbo.MedReport TO Clientes;
	GRANT SELECT ON dbo.Clients TO Helpdesks;
	GRANT SELECT ON dbo.MedReport TO Helpdesks;

	GRANT UNMASK TO Clientes;




-------------------- STORED PROCEDURES -----------------------------

	CREATE PROCEDURE GetClientData
	@UserMail NVARCHAR(500),
	@UserType NVARCHAR(500),
	@ClientId INT
	AS
	BEGIN
		DECLARE @SqlStatement NVARCHAR(MAX);

		IF @UserType = 'Client'
		BEGIN
			-- Para Cliente
			EXECUTE AS USER = 'Clientes';
			SELECT C.*, MR.* 
			FROM dbo.Clients C 
			LEFT JOIN dbo.MedReport MR ON C.MedicalRecordNumber = MR.MedicalRecordNumber 
			WHERE C.EmailAddress = @UserMail;
			REVERT;		
		END
		ELSE IF @UserType = 'Helpdesk'
		BEGIN
			IF @ClientId <> 0
			BEGIN
				GRANT UNMASK TO Helpdesks;
				EXECUTE AS USER = 'Helpdesks';
				SELECT C.*, MR.* 
				FROM dbo.Clients C LEFT JOIN dbo.MedReport MR ON C.MedicalRecordNumber = MR.MedicalRecordNumber
				WHERE C.ClientId = @ClientId;
				REVERT;
				REVOKE UNMASK TO Helpdesks;
			END
			ELSE
			BEGIN
				SET @SqlStatement = 'EXECUTE AS USER = ''Helpdesks''; SELECT C.*, MR.* FROM dbo.Clients C LEFT JOIN dbo.MedReport MR ON C.MedicalRecordNumber = MR.MedicalRecordNumber;';
			END
		END
   
		EXEC sp_executesql @SqlStatement, N'@UserMail NVARCHAR(500)', @UserMail;

		REVERT;
	END;



	CREATE PROCEDURE UpdateClientData
		@UserType NVARCHAR(500),
		@UserMail NVARCHAR(500) = NULL,
		@FullName NVARCHAR(255),
		@PhoneNumber NVARCHAR(20) = NULL,
		@ClientId INT = NULL,
		@MedReportId INT = NULL,
		@MedRecordNumber NVARCHAR(255) = NULL,
		@DiagnosisDetails NVARCHAR(MAX) = NULL,
		@TreatmentPlan NVARCHAR(MAX) = NULL, 
		@KnowCode BIT = NULL
	AS
	BEGIN
		IF @UserType = 'Client'
		BEGIN
			EXECUTE AS USER = 'Clientes'
			UPDATE dbo.Clients
			SET
				FullName = @FullName,
				PhoneNumber = ISNULL(@PhoneNumber, PhoneNumber)
			WHERE
				EmailAddress = @UserMail;
			REVERT;
		END
		  ELSE IF @UserType = 'Helpdesk' AND @KnowCode = 1
			BEGIN
				GRANT UPDATE ON dbo.Clients TO Helpdesks;
				GRANT UPDATE ON dbo.MedReport TO Helpdesks;
				EXECUTE AS USER = 'Helpdesks';
				-- Atualizar os dados do Cliente
				UPDATE dbo.Clients 
				SET
					EmailAddress = @UserMail,
					FullName = @FullName,
					PhoneNumber = @PhoneNumber,
					MedicalRecordNumber = @MedRecordNumber
				WHERE ClientId = @ClientId;
				-- Atualiza os MedReports
				UPDATE dbo.MedReport
				SET
					DiagnosisDetails = @DiagnosisDetails,
					TreatmentPlan =  @TreatmentPlan
				WHERE MedReportId = @MedReportId;
				
				
				REVERT;
				REVOKE UPDATE ON dbo.Clients FROM Helpdesks;
				REVOKE UPDATE ON dbo.MedReport FROM Helpdesks;
			END
		ELSE IF @UserType = 'Helpdesk' AND @KnowCode = 0
		BEGIN
			-- EDITAR APENAS O NOME, campo nao mascarado
			GRANT UPDATE ON dbo.Clients TO Helpdesks;
			GRANT UPDATE ON dbo.MedReport TO Helpdesks;
			EXECUTE AS USER = 'Helpdesks'
			UPDATE dbo.Clients
				SET 
					FullName = @FullName,
					MedicalRecordNumber = @MedRecordNumber
				WHERE ClientId = @ClientId;
			REVERT;
			REVOKE UPDATE ON dbo.Clients FROM Helpdesks;
			REVOKE UPDATE ON dbo.MedReport TO Helpdesks;
		END
	END;




	-----------------------------------------
	DROP TABLE dbo.Users;
	DROP TABLE dbo.Clients;
	DROP TABLE dbo.MedReport;
 

	USE ASproj1;
	SELECT * FROM dbo.Users;
	SELECT * FROM dbo.Clients;
	SELECT * FROM dbo.MedReport;

