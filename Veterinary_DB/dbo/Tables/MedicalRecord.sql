CREATE TABLE [dbo].[MedicalRecord](
	[RecordID] [int] NOT NULL,
	[PetID] [int] NULL,
	[VetID] [int] NULL,
	[AppointmentID] [int] NULL,
	[Diagnosis] [nvarchar](100) NOT NULL,
	[Treatment] [nvarchar](100) NOT NULL,
	[Prescription] [nvarchar](100) NOT NULL,
	[Vaccination] [nvarchar](100) NOT NULL,
	[Notes] [nvarchar](500) NOT NULL,
	[DateRecorded] [datetime] NULL,
 CONSTRAINT [PK_MedicalRecord] PRIMARY KEY CLUSTERED 
(
	[RecordID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[MedicalRecord]  WITH CHECK ADD  CONSTRAINT [FK_MedicalRecord_Appointment] FOREIGN KEY([AppointmentID])
REFERENCES [dbo].[Appointment] ([AppointmentID])
GO

ALTER TABLE [dbo].[MedicalRecord] CHECK CONSTRAINT [FK_MedicalRecord_Appointment]
GO


GO
ALTER TABLE [dbo].[MedicalRecord]  WITH CHECK ADD  CONSTRAINT [FK_MedicalRecord_Pet] FOREIGN KEY([PetID])
REFERENCES [dbo].[Pet] ([PetID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[MedicalRecord] CHECK CONSTRAINT [FK_MedicalRecord_Pet]
GO


GO
ALTER TABLE [dbo].[MedicalRecord]  WITH CHECK ADD  CONSTRAINT [FK_MedicalRecord_VeterinarianAdmin] FOREIGN KEY([VetID])
REFERENCES [dbo].[VeterinarianAdmin] ([VetID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[MedicalRecord] CHECK CONSTRAINT [FK_MedicalRecord_VeterinarianAdmin]
GO

