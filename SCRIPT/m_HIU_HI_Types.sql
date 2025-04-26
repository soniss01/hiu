
CREATE TABLE dbo.m_HIU_HI_Types(
	Id int IDENTITY(1,1) NOT NULL,
	Code varchar(20) NULL,
	Display varchar(50) NULL,
	Active bit NULL,
	CreatedOn datetime NULL,
	UpdatedOn datetime NULL,
)


--SET IDENTITY_INSERT [dbo].[m_HIU_HI_Types] ON 
--GO
--INSERT [dbo].[m_HIU_HI_Types] ([Id], [Code], [Display], [Active], [CreatedOn], [UpdatedOn]) VALUES (1, N'Prescription', N'Prescription', 1, CAST(N'2023-06-20T12:31:44.740' AS DateTime), NULL)
--GO			  
--INSERT [dbo].[m_HIU_HI_Types] ([Id], [Code], [Display], [Active], [CreatedOn], [UpdatedOn]) VALUES (2, N'DiagnosticReport', N'Diagnostic Report', 1, CAST(N'2023-06-20T12:31:44.740' AS DateTime), NULL)
--GO			
--INSERT [dbo].[m_HIU_HI_Types] ([Id], [Code], [Display], [Active], [CreatedOn], [UpdatedOn]) VALUES (3, N'OPConsultation', N'OP Consultation', 1, CAST(N'2023-06-20T12:31:44.740' AS DateTime), NULL)
--GO			
--INSERT [dbo].[m_HIU_HI_Types] ([Id], [Code], [Display], [Active], [CreatedOn], [UpdatedOn]) VALUES (4, N'DischargeSummary', N'Discharge Summary', 1, CAST(N'2023-06-20T12:31:44.740' AS DateTime), NULL)
--GO			
--INSERT [dbo].[m_HIU_HI_Types] ([Id], [Code], [Display], [Active], [CreatedOn], [UpdatedOn]) VALUES (5, N'ImmunizationRecord', N'Immunization Record', 1, CAST(N'2023-06-20T12:31:44.740' AS DateTime), NULL)
--GO		
--INSERT [dbo].[m_HIU_HI_Types] ([Id], [Code], [Display], [Active], [CreatedOn], [UpdatedOn]) VALUES (6, N'HealthDocumentRecord', N'Record artifact', 1, CAST(N'2023-06-20T12:31:44.740' AS DateTime), NULL)
--GO			
--INSERT [dbo].[m_HIU_HI_Types] ([Id], [Code], [Display], [Active], [CreatedOn], [UpdatedOn]) VALUES (7, N'WellnessRecord', N'Wellness Record', 1, CAST(N'2023-06-20T12:31:44.740' AS DateTime), NULL)
--GO
--SET IDENTITY_INSERT [dbo].[m_HIU_HI_Types] OFF



CREATE PROCEDURE [dbo].[sp_HIU_Select_HI_Types]
AS
BEGIN
  SELECT Code, Display FROM m_HIU_HI_Types where Active=1
END
GO