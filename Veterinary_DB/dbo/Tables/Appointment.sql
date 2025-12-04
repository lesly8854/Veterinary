CREATE TABLE [dbo].[Appointment](
	[AppointmentID] [int] NOT NULL,
	[PetID] [int] NULL,
	[VetID] [int] NULL,
	[OwnerID] [int] NULL,
	[Date] [datetime] NULL,
	[Reason] [nvarchar](500) NOT NULL,
	[Status] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_Appointment] PRIMARY KEY CLUSTERED 
(
	[AppointmentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Appointment]  WITH CHECK ADD  CONSTRAINT [FK_Appointment_Pet] FOREIGN KEY([PetID])
REFERENCES [dbo].[Pet] ([PetID])
GO

ALTER TABLE [dbo].[Appointment] CHECK CONSTRAINT [FK_Appointment_Pet]
GO


GO
ALTER TABLE [dbo].[Appointment]  WITH CHECK ADD  CONSTRAINT [FK_Appointment_PetOwner] FOREIGN KEY([OwnerID])
REFERENCES [dbo].[PetOwner] ([OwnerID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Appointment] CHECK CONSTRAINT [FK_Appointment_PetOwner]
GO


GO
ALTER TABLE [dbo].[Appointment]  WITH CHECK ADD  CONSTRAINT [FK_Appointment_VeterinarianAdmin] FOREIGN KEY([VetID])
REFERENCES [dbo].[VeterinarianAdmin] ([VetID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Appointment] CHECK CONSTRAINT [FK_Appointment_VeterinarianAdmin]
GO

