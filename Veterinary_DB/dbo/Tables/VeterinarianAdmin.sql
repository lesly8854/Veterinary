CREATE TABLE [dbo].[VeterinarianAdmin](
	[VetID] [int] NOT NULL,
	[AdminName] [nvarchar](50) NOT NULL,
	[AdminPhoneNumber] [nvarchar](100) NULL,
	[AdminUsername] [nvarchar](20) NOT NULL,
	[AdminPassword] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_VeterinarianAdmin] PRIMARY KEY CLUSTERED 
(
	[VetID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]