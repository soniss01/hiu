
CREATE TABLE dbo.m_HIU_PurposeOfUse(
	Id int IDENTITY(1,1) NOT NULL,
	Code varchar(20) NULL,
	Display varchar(50) NULL,
	Active bit NULL,
	CreatedOn datetime NULL,
	UpdatedOn datetime NULL,
)


SET IDENTITY_INSERT [dbo].[m_HIU_PurposeOfUse] ON 
GO
INSERT [dbo].[m_HIU_PurposeOfUse] ([Id], [Code], [Display], [Active], [CreatedOn], [UpdatedOn]) VALUES (1, N'CAREMGT', N'Care Management', 1, CAST(N'2023-06-20T12:31:23.527' AS DateTime), NULL)
GO			  
INSERT [dbo].[m_HIU_PurposeOfUse] ([Id], [Code], [Display], [Active], [CreatedOn], [UpdatedOn]) VALUES (2, N'BTG', N'Break the Glass', 1, CAST(N'2023-06-20T12:31:23.527' AS DateTime), NULL)
GO			
INSERT [dbo].[m_HIU_PurposeOfUse] ([Id], [Code], [Display], [Active], [CreatedOn], [UpdatedOn]) VALUES (3, N'PUBHLTH', N'Public Health', 1, CAST(N'2023-06-20T12:31:23.527' AS DateTime), NULL)
GO			  
INSERT [dbo].[m_HIU_PurposeOfUse] ([Id], [Code], [Display], [Active], [CreatedOn], [UpdatedOn]) VALUES (4, N'HPAYMT', N'Healthcare Payment', 1, CAST(N'2023-06-20T12:31:23.527' AS DateTime), NULL)
GO			  
INSERT [dbo].[m_HIU_PurposeOfUse] ([Id], [Code], [Display], [Active], [CreatedOn], [UpdatedOn]) VALUES (5, N'DSRCH', N'Disease Specific Healthcare Research', 1, CAST(N'2023-06-20T12:31:23.527' AS DateTime), NULL)
GO			
INSERT [dbo].[m_HIU_PurposeOfUse] ([Id], [Code], [Display], [Active], [CreatedOn], [UpdatedOn]) VALUES (6, N'PATRQT', N'Self Requested', 1, CAST(N'2023-06-20T12:31:23.527' AS DateTime), NULL)
GO
SET IDENTITY_INSERT [dbo].[m_HIU_PurposeOfUse] OFF


CREATE PROCEDURE [dbo].[sp_HIU_Select_PurposeOfUse]
AS
BEGIN
  SELECT Code, Display FROM m_HIU_PurposeOfUse where Active=1
END