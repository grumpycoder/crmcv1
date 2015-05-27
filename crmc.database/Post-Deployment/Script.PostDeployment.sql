/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

USE [CRMC]
GO
INSERT [dbo].[AspNetUsers] ([Id], [Email], [EmailConfirmed], [PasswordHash], [SecurityStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEndDateUtc], [LockoutEnabled], [AccessFailedCount], [UserName]) VALUES (N'de400966-db12-4f76-9d9c-e954d5f22ae8', N'lecia.brooks@splcenter.org', 0, N'AAOOmsyVbkVOpHCQhG1qarze26J18zPodY76BFYM28ZiZaZpPKxs3U5bRNrkJZcGJg==', N'd2a9a312-19aa-46fa-ae27-7a4c5a0cfc56', NULL, 0, 0, NULL, 0, 0, N'lecia.brooks')
GO
INSERT [dbo].[AspNetUsers] ([Id], [Email], [EmailConfirmed], [PasswordHash], [SecurityStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEndDateUtc], [LockoutEnabled], [AccessFailedCount], [UserName]) VALUES (N'ffca8fc1-a067-4636-8d62-8ff71fffd155', N'mark.lawrence@splcenter.org', 0, N'ACOAK4eTYgGQ2IySK400RJLwlk+ZE8hNIkJ32EwBDVPkubXjbvy0TYLhnbGmePOw3g==', N'5bec5bf9-f489-4ca1-a244-66356f39dba6', NULL, 0, 0, NULL, 0, 0, N'mark.lawrence')
GO
INSERT [dbo].[AspNetRoles] ([Id], [Name]) VALUES (N'1', N'admin')
GO
INSERT [dbo].[AspNetRoles] ([Id], [Name]) VALUES (N'2', N'user')
GO
INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'ffca8fc1-a067-4636-8d62-8ff71fffd155', N'1')
GO
INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'ffca8fc1-a067-4636-8d62-8ff71fffd155', N'2')
GO
INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'de400966-db12-4f76-9d9c-e954d5f22ae8', N'1')
GO															  
INSERT [dbo].[AspNetUserRoles] ([UserId], [RoleId]) VALUES (N'de400966-db12-4f76-9d9c-e954d5f22ae8', N'2')
GO


USE [CRMC]
GO
SET IDENTITY_INSERT [dbo].[AppConfigs] ON 

GO
INSERT [dbo].[AppConfigs] ([Id], [HubName], [WebServerURL], [Volume], [ScrollSpeed], [AddNewItemSpeed], [MinFontSize], [MaxFontSize], [MinFontSizeVIP], [MaxFontSizeVIP], [FontFamily], [AudioFilePath], [UseLocalDataSource]) VALUES (1, N'CRMCHub', N'http://crmc', 0.69893355209187868, 10, 0.51, 10, 30, 30, 40, N'Optima LT Std', N'c:\audio', 0)
GO
SET IDENTITY_INSERT [dbo].[AppConfigs] OFF
GO

