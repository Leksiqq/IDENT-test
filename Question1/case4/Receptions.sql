USE [IDENTClinic4]
GO

/****** Object:  Table [dbo].[Receptions]    Script Date: 30.07.2024 12:26:02 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Receptions](
	[ID] [int] NOT NULL,
	[ID_Patients] [int] NOT NULL,
	[ID_Doctors] [int] NOT NULL,
	[StartDateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_Receptions] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Receptions]  WITH CHECK ADD  CONSTRAINT [FK_Receptions_Doctors] FOREIGN KEY([ID_Doctors])
REFERENCES [dbo].[Doctors] ([ID])
GO

ALTER TABLE [dbo].[Receptions] CHECK CONSTRAINT [FK_Receptions_Doctors]
GO

ALTER TABLE [dbo].[Receptions]  WITH CHECK ADD  CONSTRAINT [FK_Receptions_Patients] FOREIGN KEY([ID_Patients])
REFERENCES [dbo].[Patients] ([ID])
GO

ALTER TABLE [dbo].[Receptions] CHECK CONSTRAINT [FK_Receptions_Patients]
GO

