CREATE TABLE [dbo].[Billing](
	[BillingID] [int] NOT NULL,
	[AppointmentID] [int] NULL,
	[OwnerID] [int] NULL,
	[PaymentStatus] [nvarchar](50) NOT NULL,
	[DateIssued] [datetime] NULL,
 CONSTRAINT [PK_Billing] PRIMARY KEY CLUSTERED 
(
	[BillingID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Billing]  WITH CHECK ADD  CONSTRAINT [FK_Billing_Appointment] FOREIGN KEY([AppointmentID])
REFERENCES [dbo].[Appointment] ([AppointmentID])
GO

ALTER TABLE [dbo].[Billing] CHECK CONSTRAINT [FK_Billing_Appointment]
GO


GO
ALTER TABLE [dbo].[Billing]  WITH CHECK ADD  CONSTRAINT [FK_Billing_PetOwner] FOREIGN KEY([OwnerID])
REFERENCES [dbo].[PetOwner] ([OwnerID])
ON UPDATE CASCADE
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[Billing] CHECK CONSTRAINT [FK_Billing_PetOwner]
GO

