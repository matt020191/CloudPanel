/****** Object:  Table [dbo].[UserPlans]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[UserPlans](
	[UserGuid] [uniqueidentifier] NOT NULL,
	[ProductID] [int] NOT NULL,
	[PlanID] [int] NOT NULL,
	[CompanyCode] [varchar](255) NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[UserPermissions]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserPermissions](
	[UserID] [int] NOT NULL,
	[EnableExchange] [bit] NOT NULL,
	[DisableExchange] [bit] NOT NULL,
	[AddDomain] [bit] NOT NULL,
	[DeleteDomain] [bit] NOT NULL,
	[ModifyAcceptedDomain] [bit] NOT NULL,
	[ImportUsers] [bit] NOT NULL,
 CONSTRAINT [PK_UserPermissions] PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Prices]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Prices](
	[PriceID] [int] IDENTITY(1,1) NOT NULL,
	[ProductID] [int] NOT NULL,
	[PlanID] [int] NOT NULL,
	[CompanyCode] [varchar](255) NOT NULL,
	[Price] [money] NOT NULL,
 CONSTRAINT [PK_Prices] PRIMARY KEY CLUSTERED 
(
	[PriceID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[PriceOverride]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[PriceOverride](
	[CompanyCode] [varchar](255) NOT NULL,
	[Price] [nvarchar](25) NULL,
	[PlanID] [int] NULL,
	[Product] [varchar](25) NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Plans_TerminalServices]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Plans_TerminalServices](
	[TSPlanID] [int] NOT NULL,
	[TSPlanName] [nvarchar](50) NOT NULL,
	[ResellerCode] [varchar](255) NOT NULL,
	[ProductID] [int] NOT NULL,
	[MaxUserSpaceMB] [int] NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Plans_Organization]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Plans_Organization](
	[OrgPlanID] [int] IDENTITY(1,1) NOT NULL,
	[OrgPlanName] [nvarchar](50) NOT NULL,
	[ProductID] [int] NOT NULL,
	[ResellerCode] [varchar](255) NULL,
	[MaxUsers] [int] NOT NULL,
	[MaxDomains] [int] NOT NULL,
	[MaxExchangeMailboxes] [int] NOT NULL,
	[MaxExchangeContacts] [int] NOT NULL,
	[MaxExchangeDistLists] [int] NOT NULL,
	[MaxExchangePublicFolders] [int] NOT NULL,
	[MaxExchangeMailPublicFolders] [int] NOT NULL,
	[MaxExchangeKeepDeletedItems] [int] NOT NULL,
	[MaxTerminalServerUsers] [int] NOT NULL,
	[MaxExchangeResourceMailboxes] [int] NULL,
 CONSTRAINT [PK_Plans_Organization] PRIMARY KEY CLUSTERED 
(
	[OrgPlanID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Plans_ExchangeMailbox]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Plans_ExchangeMailbox](
	[MailboxPlanID] [int] IDENTITY(1,1) NOT NULL,
	[MailboxPlanName] [nvarchar](50) NOT NULL,
	[ProductID] [int] NULL,
	[ResellerCode] [varchar](255) NULL,
	[CompanyCode] [varchar](255) NULL,
	[MailboxSizeMB] [int] NOT NULL,
	[MaxSendKB] [int] NOT NULL,
	[MaxReceiveKB] [int] NOT NULL,
	[MaxRecipients] [int] NOT NULL,
	[EnablePOP3] [bit] NOT NULL,
	[EnableIMAP] [bit] NOT NULL,
	[EnableOWA] [bit] NOT NULL,
	[EnableMAPI] [bit] NOT NULL,
	[EnableAS] [bit] NOT NULL,
	[EnableECP] [bit] NOT NULL,
	[MaxKeepDeletedItems] [int] NOT NULL,
	[MailboxPlanDesc] [ntext] NULL,
	[Price] [nvarchar](7) NULL,
	[Cost] [nvarchar](7) NULL,
	[MaxMailboxSizeMB] [int] NULL,
	[AdditionalGBPrice] [nvarchar](20) NULL,
 CONSTRAINT [PK_Plans_ExchangeMailbox] PRIMARY KEY CLUSTERED 
(
	[MailboxPlanID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Plans_ExchangeActiveSync]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Plans_ExchangeActiveSync](
	[ASID] [int] IDENTITY(1,1) NOT NULL,
	[CompanyCode] [varchar](255) NOT NULL,
	[DisplayName] [varchar](150) NOT NULL,
	[Description] [ntext] NULL,
	[ExchangeName] [varchar](75) NULL,
	[AllowBluetooth] [varchar](10) NULL,
	[AllowBrowser] [bit] NULL,
	[AllowCamera] [bit] NULL,
	[AllowConsumerMail] [bit] NULL,
	[AllowDesktopSync] [bit] NULL,
	[AllowInternetSharing] [bit] NULL,
	[AllowSimplePassword] [bit] NULL,
	[AllowTextMessaging] [bit] NULL,
	[AllowWifi] [bit] NULL,
	[AllowPOPIMAP] [bit] NULL,
	[AllowHTMLEmail] [bit] NULL,
	[AllowInfraredConnections] [bit] NULL,
	[AllowRemoteDesktop] [bit] NULL,
	[AllowStorageCard] [bit] NULL,
	[AllowNonProvisionableDevices] [bit] NULL,
	[PasswordEnabled] [bit] NULL,
	[AlphanumericPasswordRequired] [bit] NULL,
	[MaxFailedPasswordAttempts] [varchar](9) NULL,
	[MinPasswordLength] [int] NULL,
	[MinDevicePasswordComplexChar] [int] NULL,
	[RefreshIntervalInHours] [int] NULL,
	[RequirePassword] [bit] NULL,
	[RequireAlphanumericPassword] [bit] NULL,
	[EnablePasswordRecovery] [bit] NULL,
	[RequireEncryptionOnDevice] [bit] NULL,
	[RequireEncryptionOnStorageCard] [bit] NULL,
	[NumberOfFailedAttempted] [int] NULL,
	[MinimumPasswordLength] [int] NULL,
	[InactivityTimeoutInMinutes] [int] NULL,
	[PasswordExpirationInDays] [int] NULL,
	[EnforcePasswordHistory] [int] NULL,
	[LimitEmailSizeInKB] [int] NULL,
	[IncludePastCalendarItems] [varchar](20) NULL,
	[IncludePastEmailItems] [varchar](20) NULL,
	[AllowDirectPushWhenRoaming] [bit] NULL,
	[AllowAttachmentsDownload] [bit] NULL,
	[MaximumAttachmentSizeInKB] [int] NULL,
	[AllowRemovableStorage] [bit] NULL,
	[AllowInfrared] [bit] NULL,
	[IsEnterpriseCAL] [bit] NULL,
	[AllowUnsignedApplications] [bit] NULL,
	[AllowUnsignedInstallationPackages] [bit] NULL,
 CONSTRAINT [PK_Plans_ExchangeActiveSync] PRIMARY KEY CLUSTERED 
(
	[ASID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Plans_Citrix]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Plans_Citrix](
	[CitrixPlanID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](56) NOT NULL,
	[GroupName] [varchar](64) NOT NULL,
	[Description] [ntext] NULL,
	[IsServer] [bit] NOT NULL,
	[CompanyCode] [varchar](255) NULL,
	[Price] [nvarchar](6) NULL,
	[Cost] [nvarchar](20) NULL,
	[PictureURL] [varchar](255) NULL,
 CONSTRAINT [PK_Plans_Citrix] PRIMARY KEY CLUSTERED 
(
	[CitrixPlanID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[LoginStatus]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[LoginStatus](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Username] [varchar](100) NOT NULL,
	[IPAddress] [varchar](48) NOT NULL,
	[DateTime] [datetime] NOT NULL,
	[LoginStatus] [bit] NOT NULL,
 CONSTRAINT [PK_LoginStatus] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[SysMessages]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SysMessages](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Message] [ntext] NOT NULL,
	[DateSubmit] [datetime] NULL,
 CONSTRAINT [PK_SysMessages] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SvcTask]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SvcTask](
	[SvcTaskID] [int] IDENTITY(1,1) NOT NULL,
	[TaskType] [int] NOT NULL,
	[LastRun] [datetime] NOT NULL,
	[NextRun] [datetime] NULL,
	[TaskOutput] [ntext] NULL,
	[TaskDelayInMinutes] [int] NOT NULL,
	[TaskCreated] [datetime] NULL,
	[Reoccurring] [bit] NOT NULL,
 CONSTRAINT [PK_SvcTask] PRIMARY KEY CLUSTERED 
(
	[SvcTaskID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SvcQueue]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SvcQueue](
	[SvcQueueID] [int] IDENTITY(1,1) NOT NULL,
	[TaskID] [int] NOT NULL,
	[UserPrincipalName] [varchar](255) NULL,
	[CompanyCode] [varchar](255) NULL,
	[TaskOutput] [ntext] NULL,
	[TaskCreated] [datetime] NOT NULL,
	[TaskCompleted] [datetime] NULL,
	[TaskDelayInMinutes] [int] NOT NULL,
	[TaskSuccess] [int] NOT NULL,
 CONSTRAINT [PK_SvcQueue] PRIMARY KEY CLUSTERED 
(
	[SvcQueueID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[SvcMailboxSizes]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[SvcMailboxSizes](
	[UserPrincipalName] [varchar](64) NOT NULL,
	[MailboxDatabase] [varchar](255) NOT NULL,
	[TotalItemSizeInKB] [varchar](255) NOT NULL,
	[TotalDeletedItemSizeInKB] [varchar](255) NOT NULL,
	[ItemCount] [int] NOT NULL,
	[DeletedItemCount] [int] NOT NULL,
	[Retrieved] [datetime] NOT NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[SvcMailboxDatabaseSizes]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SvcMailboxDatabaseSizes](
	[DatabaseName] [nvarchar](64) NOT NULL,
	[Server] [nvarchar](64) NOT NULL,
	[DatabaseSize] [nvarchar](255) NOT NULL,
	[Retrieved] [date] NOT NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Stats_UserCount]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Stats_UserCount](
	[StatDate] [date] NOT NULL,
	[UserCount] [int] NOT NULL,
 CONSTRAINT [PK_Stats_UserCount] PRIMARY KEY CLUSTERED 
(
	[StatDate] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Stats_ExchCount]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Stats_ExchCount](
	[StatDate] [date] NOT NULL,
	[UserCount] [int] NOT NULL,
 CONSTRAINT [PK_Stats_ExchCount] PRIMARY KEY CLUSTERED 
(
	[StatDate] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Stats_CitrixCount]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Stats_CitrixCount](
	[StatDate] [date] NOT NULL,
	[UserCount] [int] NOT NULL,
 CONSTRAINT [PK_Stats_CitrixCount] PRIMARY KEY CLUSTERED 
(
	[StatDate] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Settings]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Settings](
	[BaseOU] [ntext] NOT NULL,
	[PrimaryDC] [varchar](50) NOT NULL,
	[Username] [varchar](50) NOT NULL,
	[Password] [ntext] NOT NULL,
	[SuperAdmins] [ntext] NOT NULL,
	[BillingAdmins] [ntext] NOT NULL,
	[ExchangeFqdn] [varchar](50) NOT NULL,
	[ExchangePFServer] [varchar](50) NOT NULL,
	[ExchangeVersion] [int] NOT NULL,
	[ExchangeSSLEnabled] [bit] NOT NULL,
	[ExchangeConnectionType] [varchar](10) NOT NULL,
	[PasswordMinLength] [int] NOT NULL,
	[PasswordComplexityType] [int] NOT NULL,
	[CitrixEnabled] [bit] NOT NULL,
	[PublicFolderEnabled] [bit] NOT NULL,
	[LyncEnabled] [bit] NOT NULL,
	[WebsiteEnabled] [bit] NOT NULL,
	[SQLEnabled] [bit] NOT NULL,
	[CurrencySymbol] [nvarchar](10) NULL,
	[CurrencyEnglishName] [nvarchar](200) NULL,
	[ResellersEnabled] [bit] NULL,
	[CompanysName] [varchar](255) NULL,
	[CompanysLogo] [varchar](255) NULL,
	[AllowCustomNameAttrib] [bit] NULL,
	[ExchStats] [bit] NULL,
	[IPBlockingEnabled] [bit] NULL,
	[IPBlockingFailedCount] [int] NULL,
	[IPBlockingLockedMinutes] [int] NULL,
	[ExchDatabases] [ntext] NULL,
	[UsersOU] [varchar](255) NULL,
	[BrandingLoginLogo] [varchar](255) NULL,
	[BrandingCornerLogo] [varchar](255) NULL,
	[LockdownEnabled] [bit] NULL,
	[LyncFrontEnd] [varchar](255) NULL,
	[LyncUserPool] [varchar](255) NULL,
	[LyncMeetingUrl] [varchar](255) NULL,
	[LyncDialinUrl] [varchar](255) NULL,
	[SupportMailEnabled] [bit] NULL,
	[SupportMailAddress] [varchar](255) NULL,
	[SupportMailServer] [varchar](255) NULL,
	[SupportMailUsername] [varchar](255) NULL,
	[SupportMailPassword] [varchar](255) NULL,
	[SupportMailFrom] [varchar](255) NULL,
	[SupportMailPort] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ResourceMailboxes]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ResourceMailboxes](
	[ResourceID] [int] IDENTITY(1,1) NOT NULL,
	[DisplayName] [nvarchar](255) NOT NULL,
	[CompanyCode] [varchar](255) NOT NULL,
	[UserPrincipalName] [nvarchar](255) NOT NULL,
	[PrimarySmtpAddress] [nvarchar](255) NOT NULL,
	[ResourceType] [varchar](10) NOT NULL,
	[MailboxPlan] [int] NOT NULL,
	[AdditionalMB] [int] NOT NULL,
 CONSTRAINT [PK_ResourceMailboxes] PRIMARY KEY CLUSTERED 
(
	[ResourceID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  StoredProcedure [dbo].[GetProducts]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 12/3/2012
-- Description:	Gets the products from the database
-- =============================================
CREATE PROCEDURE [dbo].[GetProducts]
AS
	SELECT ProductID,
		   ProductName,
		   ProductDesc,
		   Created
	FROM
		   dbo.Products
RETURN
GO
/****** Object:  Table [dbo].[Companies]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Companies](
	[CompanyId] [int] IDENTITY(1,1) NOT NULL,
	[IsReseller] [bit] NOT NULL,
	[ResellerCode] [nvarchar](255) NULL,
	[OrgPlanID] [int] NULL,
	[CompanyName] [varchar](100) NOT NULL,
	[CompanyCode] [nvarchar](255) NOT NULL,
	[Street] [nvarchar](255) NOT NULL,
	[City] [nvarchar](100) NOT NULL,
	[State] [nvarchar](100) NOT NULL,
	[ZipCode] [nvarchar](50) NOT NULL,
	[PhoneNumber] [nvarchar](50) NOT NULL,
	[Website] [nvarchar](255) NULL,
	[Description] [ntext] NULL,
	[AdminName] [nvarchar](100) NOT NULL,
	[AdminEmail] [nvarchar](255) NOT NULL,
	[DistinguishedName] [nvarchar](255) NOT NULL,
	[Created] [datetime] NOT NULL,
	[ExchEnabled] [bit] NOT NULL,
	[LyncEnabled] [bit] NULL,
	[CitrixEnabled] [bit] NULL,
	[ExchPFPlan] [int] NULL,
	[Country] [varchar](50) NULL,
	[ExchPermFixed] [bit] NULL,
 CONSTRAINT [PK_Companies] PRIMARY KEY CLUSTERED 
(
	[CompanyId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[AuditLogin]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[AuditLogin](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[IPAddress] [varchar](128) NOT NULL,
	[Username] [varchar](255) NOT NULL,
	[LoginStatus] [bit] NOT NULL,
	[AuditTimeStamp] [datetime] NOT NULL,
 CONSTRAINT [PK_AuditLogin] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Audit]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Audit](
	[AuditID] [int] IDENTITY(1,1) NOT NULL,
	[Username] [nvarchar](50) NOT NULL,
	[Date] [datetime] NOT NULL,
	[SeverityID] [int] NULL,
	[MethodName] [varchar](100) NULL,
	[Parameters] [ntext] NULL,
	[Message] [ntext] NOT NULL,
 CONSTRAINT [PK_Audit] PRIMARY KEY CLUSTERED 
(
	[AuditID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  StoredProcedure [dbo].[AddProduct]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 12/3/2012
-- Description:	Adds a product to the database
-- =============================================
CREATE PROCEDURE [dbo].[AddProduct]
	@ProductID	 int OUTPUT,
	@ProductName nvarchar(100),
	@ProductDesc ntext
AS
	INSERT INTO Products
	(
		ProductName,
		ProductDesc,
		Created
	)
	VALUES
	(
		@ProductName,
		@ProductDesc,
		CURRENT_TIMESTAMP
	)
	
	SET @ProductID = SCOPE_IDENTITY()
RETURN
GO
/****** Object:  Table [dbo].[Contacts]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Contacts](
	[DistinguishedName] [varchar](255) NOT NULL,
	[CompanyCode] [varchar](255) NULL,
	[DisplayName] [varchar](255) NOT NULL,
	[Email] [varchar](255) NOT NULL,
	[Hidden] [bit] NOT NULL,
 CONSTRAINT [PK_Contacts] PRIMARY KEY CLUSTERED 
(
	[DistinguishedName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[CompanyStats]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[CompanyStats](
	[CompanyID] [int] NOT NULL,
	[CompanyCode] [varchar](255) NULL,
	[UserCount] [int] NOT NULL,
	[DomainCount] [int] NOT NULL,
	[SubDomainCount] [int] NOT NULL,
	[ExchMailboxCount] [int] NOT NULL,
	[ExchContactsCount] [int] NOT NULL,
	[ExchDistListsCount] [int] NOT NULL,
	[ExchPublicFolderCount] [int] NOT NULL,
	[ExchMailPublicFolderCount] [int] NOT NULL,
	[ExchKeepDeletedItems] [int] NOT NULL,
	[RDPUserCount] [int] NOT NULL,
 CONSTRAINT [PK_CompanyStats] PRIMARY KEY CLUSTERED 
(
	[CompanyID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[ApiAccess]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[ApiAccess](
	[CustomerKey] [varchar](255) NOT NULL,
	[CustomerSecret] [varchar](255) NOT NULL,
	[CompanyCode] [varchar](255) NOT NULL,
 CONSTRAINT [PK_ApiAccess] PRIMARY KEY CLUSTERED 
(
	[CustomerKey] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[DistributionGroups]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[DistributionGroups](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[DistinguishedName] [varchar](255) NOT NULL,
	[CompanyCode] [varchar](255) NULL,
	[DisplayName] [varchar](255) NOT NULL,
	[Email] [varchar](255) NOT NULL,
	[Hidden] [bit] NOT NULL,
 CONSTRAINT [PK_DistributionGroups] PRIMARY KEY CLUSTERED 
(
	[DistinguishedName] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[Domains]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Domains](
	[DomainID] [int] IDENTITY(1,1) NOT NULL,
	[CompanyCode] [varchar](255) NULL,
	[Domain] [nvarchar](255) NOT NULL,
	[IsSubDomain] [bit] NULL,
	[IsDefault] [bit] NOT NULL,
	[IsAcceptedDomain] [bit] NOT NULL,
	[IsLyncDomain] [bit] NULL,
 CONSTRAINT [PK_Domains] PRIMARY KEY CLUSTERED 
(
	[DomainID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  StoredProcedure [dbo].[DomainExists]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 12/7/2012
-- Description:	Gets domain information based on the domain name
-- =============================================
CREATE PROCEDURE [dbo].[DomainExists]
	@DomainName nvarchar(255)
AS
BEGIN
	SET NOCOUNT ON;

    SELECT COUNT(Domain) AS Results FROM Domains WHERE Domain=@DomainName
END
GO
/****** Object:  StoredProcedure [dbo].[CountDomains]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 12/15/2012
-- Description:	Count the domains for a specific company
-- =============================================
CREATE PROCEDURE [dbo].[CountDomains]
	@CompanyCode varchar(10)
AS
	SELECT COUNT(Domain) FROM Domains
	  WHERE
		   CompanyCode=@CompanyCode
RETURN
GO
/****** Object:  StoredProcedure [dbo].[GetCompanyDomains]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 12/4/2012
-- Description:	Gets a list of domains that are assigned to a company
-- =============================================
CREATE PROCEDURE [dbo].[GetCompanyDomains]
	@CompanyCode varchar(10)
AS
	SELECT	DomainID,
			Domain,
			IsSubDomain,
			IsDefault,
			IsAcceptedDomain
	FROM Domains
	WHERE	
			CompanyCode=@CompanyCode
RETURN
GO
/****** Object:  StoredProcedure [dbo].[GetCompany]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon	
-- Create date: 12/7/2012
-- Description:	Gets more detailed information about a company
-- =============================================
CREATE PROCEDURE [dbo].[GetCompany]
	@CompanyCode varchar(10)
AS
	SELECT		ResellerCode,
				OrgPlanID,
				CompanyName,
				CompanyCode,
				Street,
				City,
				State,
				ZipCode,
				PhoneNumber,
				Website,
				Description,
				AdminName,
				AdminEmail,
				DistinguishedName,
				Created,
				ExchEnabled,
				ExchPFPlan,
				LyncEnabled,
				CitrixEnabled
		FROM
				Companies
		WHERE
				CompanyCode=@CompanyCode
		AND
				IsReseller=0
GO
/****** Object:  StoredProcedure [dbo].[GetCompanies]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 12/3/2012
-- Description:	Gets a list of companies based on a reseller
-- =============================================
CREATE PROCEDURE [dbo].[GetCompanies]
	@ResellerCode varchar(10)
AS
	SELECT  ResellerCode,
			OrgPlanID,
			CompanyName,
			CompanyCode,
			Street,
			City,
			State,
			ZipCode,
			PhoneNumber,
			Website,
			Description,
			AdminName,
			AdminEmail,
			DistinguishedName,
			Created
	FROM
			Companies
	WHERE
			ResellerCode=@ResellerCode
	AND
			IsReseller=0
	ORDER BY
			CompanyName
RETURN
GO
/****** Object:  UserDefinedFunction [dbo].[GetExchangePublicFolderPrice]    Script Date: 01/15/2015 19:56:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetExchangePublicFolderPrice]
                (
                                @CompanyCode varchar(255),
                                @ExchPFPlan int
                )
                RETURNS varchar(25)
                AS
                BEGIN
                               
                                DECLARE @price VARCHAR(25);
                                SET @price = (SELECT Price FROM PriceOverride WHERE CompanyCode=@CompanyCode AND PlanID=@ExchPFPlan AND Product='Public Folder');
 
                                IF @price IS NULL
                                                SET @price = (SELECT Price FROM Plans_PublicFolders WHERE PFID=@ExchPFPlan);
 
                                RETURN @price;
 
                END
GO
/****** Object:  StoredProcedure [dbo].[GetExchangeMailboxPlans]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 11/30/2012
-- Description:	Retrieves all the Exchange plans from the database
-- =============================================
CREATE PROCEDURE [dbo].[GetExchangeMailboxPlans]
AS
	Select MailboxPlanID,
	       CompanyCode,
		   MailboxPlanName, 
		   MailboxSizeMB, 
		   MaxSendKB, 
		   MaxReceiveKB, 
		   MaxRecipients,
		   EnablePOP3,
		   EnableIMAP,
		   EnableOWA,
		   EnableMAPI,
		   EnableAS,
		   EnableECP,
		   MaxKeepDeletedItems,
		   MailboxPlanDesc,
		   Price,
		   Cost
	FROM
		   dbo.Plans_ExchangeMailbox
RETURN
GO
/****** Object:  UserDefinedFunction [dbo].[GetExchangeMailboxPlanPrice]    Script Date: 01/15/2015 19:56:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetExchangeMailboxPlanPrice]
                (
                                @CompanyCode varchar(255),
                                @MailboxPlanID int
                )
                RETURNS varchar(25)
                AS
                BEGIN
                               
                                DECLARE @price VARCHAR(25);
                                SET @price = (SELECT Price FROM PriceOverride WHERE CompanyCode=@CompanyCode AND PlanID=@MailboxPlanID AND Product='Exchange');
 
                                IF @price IS NULL
                                                SET @price = (SELECT Price FROM Plans_ExchangeMailbox WHERE MailboxPlanID=@MailboxPlanID);
 
                                RETURN @price;
 
 
                END
GO
/****** Object:  StoredProcedure [dbo].[GetExchangeMailboxPlan]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 11/30/2012
-- Description: Gets a specific exchange mailbox plan from the database
-- =============================================
CREATE PROCEDURE [dbo].[GetExchangeMailboxPlan]
	@MailboxPlanID int
AS
	Select MailboxPlanID, 
		   MailboxPlanName, 
		   MailboxSizeMB, 
		   MaxSendKB, 
		   MaxReceiveKB, 
		   MaxRecipients,
		   EnablePOP3,
		   EnableIMAP,
		   EnableOWA,
		   EnableMAPI,
		   EnableAS,
		   EnableECP,
		   MaxKeepDeletedItems,
		   MailboxPlanDesc,
		   CompanyCode,
		   Price,
		   Cost
	FROM
		   dbo.Plans_ExchangeMailbox
	WHERE
			MailboxPlanID=@MailboxPlanID
RETURN
GO
/****** Object:  StoredProcedure [dbo].[GetDistributionGroups]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 1/18/2013
-- Description:	Gets a list of distribution groups for a company
-- =============================================
CREATE PROCEDURE [dbo].[GetDistributionGroups]
	@CompanyCode varchar(10)
AS
	SELECT  ID,
			CompanyCode,
			DistinguishedName,
			DisplayName, 
			Email,
			Hidden
	FROM
			DistributionGroups
	WHERE
			CompanyCode=@CompanyCode
RETURN
GO
/****** Object:  StoredProcedure [dbo].[GetDistributionGroupById]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 1/25/2013
-- Description:	Gets a distribution group by ID
-- =============================================
CREATE PROCEDURE [dbo].[GetDistributionGroupById]
	@ID int
AS
	SELECT 
			ID,
			DistinguishedName,
			CompanyCode, 
			DisplayName, 
			Email,
			Hidden
	FROM
			DistributionGroups
	WHERE
			ID=@ID
RETURN
GO
/****** Object:  StoredProcedure [dbo].[GetContacts]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 1/15/2013
-- Description:	Gets contacts for a specific company
-- =============================================
CREATE PROCEDURE [dbo].[GetContacts]
	@CompanyCode varchar(10)
AS
	SELECT 
			DistinguishedName,
			CompanyCode, 
			DisplayName, 
			Email,
			Hidden
	FROM
			Contacts
	WHERE
			CompanyCode=@CompanyCode
RETURN
GO
/****** Object:  StoredProcedure [dbo].[CompanyCodeExists]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[CompanyCodeExists] 
	@CompanyCode varchar(10)
AS
BEGIN
	SET NOCOUNT ON;

    SELECT COUNT(CompanyCode) AS Results FROM Companies 
		WHERE CompanyCode=@CompanyCode
END
GO
/****** Object:  StoredProcedure [dbo].[AddOrganizationPlan]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 12/9/2012
-- Description:	Adds a new organization plan
-- =============================================
CREATE PROCEDURE [dbo].[AddOrganizationPlan] 
	@OrgPlanName nvarchar(50),
	@MaxUsers int,
	@MaxDomains int,
	@MaxExchangeMailboxes int,
	@MaxExchangeContacts int,
	@MaxExchangeDistLists int,
	@MaxExchangePublicFolders int,
	@MaxExchangeMailPublicFolders int,
	@MaxExchangeKeepDeletedItems int,
	@MaxTerminalServerUsers int
AS
	INSERT INTO Plans_Organization
				(
					ProductID,
					OrgPlanName,
					MaxUsers,
					MaxDomains,
					MaxExchangeMailboxes,
					MaxExchangeContacts,
					MaxExchangeDistLists,
					MaxExchangePublicFolders,
					MaxExchangeMailPublicFolders,
					MaxExchangeKeepDeletedItems,
					MaxTerminalServerUsers
				)
				VALUES
				(
					2,
					@OrgPlanName,
					@MaxUsers,
					@MaxDomains,
					@MaxExchangeMailboxes,
					@MaxExchangeContacts,
					@MaxExchangeDistLists,
					@MaxExchangePublicFolders,
					@MaxExchangeMailPublicFolders,
					@MaxExchangeKeepDeletedItems,
					@MaxTerminalServerUsers
				)
RETURN
GO
/****** Object:  StoredProcedure [dbo].[AddExchangeMailboxPlan]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 11/30/2012
-- Description:	Adds a new Exchange Mailbox Plan
-- =============================================
CREATE PROCEDURE [dbo].[AddExchangeMailboxPlan]
	@MailboxPlanName nvarchar(50),
	@MailboxSizeMB int,
	@MaxSendKB int,
	@MaxReceiveKB int,
	@MaxRecipients int,
	@EnablePOP3 bit,
	@EnableIMAP bit,
	@EnableOWA bit,
	@EnableMAPI bit,
	@EnableAS bit,
	@EnableECP bit,
	@MaxKeepDeletedItems int,
	@MailboxPlanDesc ntext,
	@CompanyCode varchar(10),
	@Price nvarchar(7),
	@Cost nvarchar(7)
AS
	INSERT INTO Plans_ExchangeMailbox
	(
		MailboxPlanName,
		MailboxSizeMB,
		MaxSendKB,
		MaxReceiveKB,
		MaxRecipients,
		EnablePOP3,
		EnableIMAP,
		EnableOWA,
		EnableMAPI,
		EnableAS,
		EnableECP,
		MaxKeepDeletedItems,
		MailboxPlanDesc,
		CompanyCode,
		Price,
		Cost
	)
	VALUES
	(
		@MailboxPlanName,
		@MailboxSizeMB,
		@MaxSendKB,
		@MaxReceiveKB,
		@MaxRecipients, 
		@EnablePOP3,
		@EnableIMAP,
		@EnableOWA,
		@EnableMAPI,
		@EnableAS,
		@EnableECP,
		@MaxKeepDeletedItems,
		@MailboxPlanDesc,
		@CompanyCode,
		@Price,
		@Cost
	)
	
	RETURN
GO
/****** Object:  StoredProcedure [dbo].[AddDomain]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 12/15/2012
-- Description:	Adds a domain to the database
-- =============================================
CREATE PROCEDURE [dbo].[AddDomain]
	@CompanyCode varchar(10),
	@DomainName nvarchar(255),
	@IsSubDomain bit,
	@IsDefault bit
AS
	IF NOT EXISTS (SELECT Domain FROM Domains WHERE Domain=@DomainName)
		INSERT INTO Domains
					(
						CompanyCode,
						Domain,
						IsSubDomain,
						IsDefault,
						IsAcceptedDomain
					)
					VALUES
					(
						@CompanyCode,
						@DomainName,
						@IsSubDomain,
						@IsDefault,
						0
					)
RETURN
GO
/****** Object:  StoredProcedure [dbo].[AddDistributionGroup]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 1/23/2012
-- Description: Adds a distribution group
-- =============================================
CREATE PROCEDURE [dbo].[AddDistributionGroup] 
	@CompanyCode varchar(10),
	@DistinguishedName varchar(255),
	@DisplayName varchar(255),
	@Email varchar(255),
	@Hidden bit
AS

	INSERT INTO DistributionGroups
				(
			     CompanyCode,
				 DistinguishedName,
				 DisplayName,
				 Email,
				 Hidden
				)
				VALUES
				(
				 @CompanyCode,
				 @DistinguishedName,
				 @DisplayName,
				 @Email,
				 @Hidden
				 )
RETURN
GO
/****** Object:  StoredProcedure [dbo].[AddContact]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 1/15/2013
-- Description:	Adds a new Exchange Contact
-- =============================================
CREATE PROCEDURE [dbo].[AddContact]
	@Companycode varchar(10),
	@DistinguishedName ntext,
	@DisplayName varchar(255),
	@Email varchar(255),
	@Hidden bit
AS

	INSERT INTO Contacts
				(
			     CompanyCode,
				 DistinguishedName,
				 DisplayName,
				 Email,
				 Hidden
				)
				VALUES
				(
				 @CompanyCode,
				 @DistinguishedName,
				 @DisplayName,
				 @Email,
				 @Hidden
				 )
RETURN
GO
/****** Object:  StoredProcedure [dbo].[AddCompany]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 12/3/2012
-- Description:	Inserts a new company into the database
-- =============================================
CREATE PROCEDURE [dbo].[AddCompany]
	@ResellerCode varchar(10),
	@CompanyName nvarchar(100),
	@CompanyCode nvarchar(10),
	@Street nvarchar(255),
	@City nvarchar(100),
	@State nvarchar(100),
	@ZipCode int,
	@PhoneNumber nvarchar(50),
	@Website nvarchar(255),
	@Description ntext,
	@AdminName nvarchar(100),
	@AdminEmail nvarchar(255),
	@DistinguishedName nvarchar(255),
	@DomainName nvarchar(255)
AS
BEGIN
	INSERT INTO Companies
				(
					IsReseller,
					ResellerCode,
					CompanyName,
					CompanyCode,
					Street,
					City,
					State,
					ZipCode,
					PhoneNumber,
					Website,
					Description,
					AdminName,
					AdminEmail,
					DistinguishedName,
					Created,
					ExchEnabled,
					ExchPFPlan
				)
				VALUES
				(
					0,
					@ResellerCode,
					@CompanyName,
					@CompanyCode,
					@Street,
					@City,
					@State,
					@ZipCode,
					@PhoneNumber,
					@Website,
					@Description,
					@AdminName,
					@AdminEmail,
					@DistinguishedName,
					CURRENT_TIMESTAMP,
					0,
					0
				)
	
	DECLARE @CompanyId int
	SET @CompanyId = SCOPE_IDENTITY()
				
	IF NOT EXISTS (SELECT Domain FROM Domains WHERE Domain=@DomainName)
		INSERT INTO Domains
				(
					CompanyCode,
					Domain,
					IsSubDomain,
					IsDefault,
					IsAcceptedDomain
				)
				VALUES
				(
					@CompanyCode,
					@DomainName,
					0,
					1,
					0
				)
END
GO
/****** Object:  StoredProcedure [dbo].[AddCitrixApp]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 4/11/2013
-- Description:	Adds a new Citrix application
-- =============================================
CREATE PROCEDURE [dbo].[AddCitrixApp]
	@Name varchar(20),
	@GroupName varchar(75),
	@Description ntext,
	@CompanyCode varchar(3),
	@IsServer bit,
	@Price nvarchar(8)
AS
BEGIN
	SET NOCOUNT ON;

    INSERT INTO Plans_Citrix
			(
				Name,
				GroupName,
				Description,
				CompanyCode,
				IsServer,
				Price
			)
			VALUES
			(
				@Name,
				@GroupName,
				@Description,
				@CompanyCode,
				@IsServer,
				@Price
			)

END
GO
/****** Object:  StoredProcedure [dbo].[AddAudit]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
			-- Author:		Jacob Dixon
			-- Create date: 12/5/2012
			-- Description:	Adds audit information to the database
			-- =============================================
			CREATE PROCEDURE [dbo].[AddAudit]
				@Username nvarchar(50),
				@Date datetime,
				@SeverityID int,
				@MethodName nvarchar(100),
				@Parameters ntext,
				@Message ntext	
			AS
				INSERT INTO Audit
					   (
							Username,
							Date,
							SeverityID,
							MethodName,
							Parameters,
							Message
					   )
					   VALUES
					   (
							@Username,
							@Date,
							@SeverityID,
							@MethodName,
							@Parameters,
							@Message
					   )
			RETURN
GO
/****** Object:  StoredProcedure [dbo].[GetResellers]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 12/3/2012
-- Description:	Gets a list of resellers
-- =============================================
CREATE PROCEDURE [dbo].[GetResellers]
AS
	SELECT	CompanyCode,
			CompanyName,
			ResellerCode,
			Street,
			City,
			State,
			ZipCode,
			PhoneNumber,
			Website,
			Description,
			AdminName,
			AdminEmail,
			DistinguishedName,
			Created
	FROM 
			Companies
	WHERE 
			IsReseller=1
GO
/****** Object:  StoredProcedure [dbo].[GetReseller]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 12/5/2012
-- Description:	Get information about a specific reseller
-- =============================================
CREATE PROCEDURE [dbo].[GetReseller]
	@ResellerCode varchar(10)
AS
	SELECT		CompanyName,
				CompanyCode,
				Street,
				City,
				State,
				ZipCode,
				PhoneNumber,
				Website,
				Description,
				AdminName,
				AdminEmail,
				DistinguishedName,
				Created
		FROM 
				Companies
		WHERE 
				IsReseller=1 AND CompanyCode=@ResellerCode
GO
/****** Object:  UserDefinedFunction [dbo].[GetCitrixPlanPrice]    Script Date: 01/15/2015 19:56:49 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetCitrixPlanPrice]
(
	@CompanyCode varchar(255),
    @CitrixPlanID int
)
RETURNS varchar(25)
AS
BEGIN
	DECLARE @price VARCHAR(25);
    
	SET @price = (SELECT Price FROM PriceOverride WHERE CompanyCode=@CompanyCode AND PlanID=@CitrixPlanID AND Product='Citrix');
 
    IF @price IS NULL
       SET @price = (SELECT Price FROM Plans_Citrix WHERE CitrixPlanID=@CitrixPlanID);
 
    RETURN @price;

END
GO
/****** Object:  StoredProcedure [dbo].[GetAllCompanies]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 5/2/2013
-- Description:	Gets a list of ALL companies
-- =============================================
CREATE PROCEDURE [dbo].[GetAllCompanies]
AS
BEGIN
	SELECT  ResellerCode,
			OrgPlanID,
			CompanyName,
			CompanyCode,
			Street,
			City,
			State,
			ZipCode,
			PhoneNumber,
			Website,
			Description,
			AdminName,
			AdminEmail,
			DistinguishedName,
			Created
	FROM
			Companies
	WHERE
			IsReseller=0
	ORDER BY
			CompanyName
END
GO
/****** Object:  StoredProcedure [dbo].[EnableCitrix]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 4/15/2013
-- Description:	Enables or Disables Citrix for a company
-- =============================================
CREATE PROCEDURE [dbo].[EnableCitrix]
	@CompanyCode varchar(3),
	@Enable bit
AS
BEGIN
	SET NOCOUNT ON;

    UPDATE Companies
		SET
			CitrixEnabled = @Enable
		WHERE
			CompanyCode = @CompanyCode

END
GO
/****** Object:  StoredProcedure [dbo].[GetOrganizationPlans]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 12/9/2012
-- Description:	Gets all organization plans for a reseller
-- =============================================
CREATE PROCEDURE [dbo].[GetOrganizationPlans]
AS
	SELECT  ResellerCode,
			OrgPlanID,
			OrgPlanName,
			MaxUsers,
			MaxDomains,
			MaxExchangeMailboxes,
			MaxExchangeContacts,
			MaxExchangeDistLists,
			MaxExchangePublicFolders,
			MaxExchangeMailPublicFolders,
			MaxExchangeKeepDeletedItems,
			MaxTerminalServerUsers
	FROM
			Plans_Organization
RETURN
GO
/****** Object:  StoredProcedure [dbo].[GetOrganizationPlan]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 12/9/2012
-- Description:	Gets information about a specific plan
-- =============================================
CREATE PROCEDURE [dbo].[GetOrganizationPlan]
	@OrgPlanID int
AS
	SELECT  ResellerCode,
			OrgPlanID,
			OrgPlanName,
			MaxUsers,
			MaxDomains,
			MaxExchangeMailboxes,
			MaxExchangeContacts,
			MaxExchangeDistLists,
			MaxExchangePublicFolders,
			MaxExchangeMailPublicFolders,
			MaxExchangeKeepDeletedItems,
			MaxTerminalServerUsers
	FROM
			Plans_Organization
	WHERE
			OrgPlanID=@OrgPlanID
RETURN
GO
/****** Object:  StoredProcedure [dbo].[GetNotification]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 2/23/2013
-- Description:	Gets a notification from the database
-- =============================================
CREATE PROCEDURE [dbo].[GetNotification]
	@CompanyCode varchar(10),
	@Product varchar(50)
AS
BEGIN
	SET NOCOUNT ON;

		SELECT 
			HTML,
			IsEnabled
		FROM
			Notifications
		WHERE
			Product=@Product AND CompanyID=(SELECT CompanyID FROM Companies WHERE CompanyCode=@CompanyCode)
END
GO
/****** Object:  StoredProcedure [dbo].[RemoveCitrixApp]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 4/12/2013
-- Description:	Remove Citrix App
-- =============================================
CREATE PROCEDURE [dbo].[RemoveCitrixApp]
	@GroupName varchar(75)
AS
BEGIN
	SET NOCOUNT ON;

    DELETE FROM Plans_Citrix
		WHERE
			GroupName=@GroupName
END
GO
/****** Object:  StoredProcedure [dbo].[AddReseller]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 12/3/2012
-- Description:	Adds a new reseller to the database
-- =============================================
CREATE PROCEDURE [dbo].[AddReseller]
	@CompanyName nvarchar(100),
	@CompanyCode nvarchar(10),
	@Street nvarchar(255),
	@City nvarchar(100),
	@State nvarchar(100),
	@ZipCode int,
	@PhoneNumber nvarchar(50),
	@Description ntext,
	@AdminName nvarchar(100),
	@AdminEmail nvarchar(255),
	@DistinguishedName nvarchar(255)
AS
	INSERT INTO Companies
				(
					IsReseller,
					CompanyName,
					CompanyCode,
					Street,
					City,
					State,
					ZipCode,
					PhoneNumber,
					Description,
					AdminName,
					AdminEmail,
					DistinguishedName,
					Created,
					ExchEnabled
				)
				VALUES
				(
					1,
					@CompanyName,
					@CompanyCode,
					@Street,
					@City,
					@State,
					@ZipCode,
					@PhoneNumber,
					@Description,
					@AdminName,
					@AdminEmail,
					@DistinguishedName,
					CURRENT_TIMESTAMP,
					0
				)
RETURN
GO
/****** Object:  StoredProcedure [dbo].[GetUserExchangeMailboxPlan]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 11/30/2012
-- Description:	Retrieves the user's mailbox plan
-- =============================================
CREATE PROCEDURE [dbo].[GetUserExchangeMailboxPlan]
	@UserGuid uniqueidentifier	
AS
	Select MailboxPlanID, 
		   MailboxPlanName, 
		   MailboxSizeMB, 
		   MaxSendKB, 
		   MaxReceiveKB, 
		   MaxRecipients,
		   EnablePOP3,
		   EnableIMAP,
		   EnableOWA,
		   EnableMAPI,
		   EnableAS,
		   EnableECP,
		   MaxKeepDeletedItems,
		   MailboxPlanDesc,
		   Price
	FROM
		   dbo.Plans_ExchangeMailbox
	INNER JOIN 
		   dbo.UserPlans ON dbo.UserPlans.UserGuid=@UserGuid AND dbo.UserPlans.ProductID=1
RETURN
GO
/****** Object:  StoredProcedure [dbo].[RemoveDistributionGroup]    Script Date: 01/15/2015 19:56:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 1/18/2013
-- Description:	Removes a distribution group
-- =============================================
CREATE PROCEDURE [dbo].[RemoveDistributionGroup]
	@DistinguishedName varchar(255)
AS
	DELETE FROM DistributionGroups
		WHERE
			DistinguishedName=@DistinguishedName
RETURN
GO
/****** Object:  StoredProcedure [dbo].[RemoveContact]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 1/15/2013
-- Description:	Deletes a contact
-- =============================================
CREATE PROCEDURE [dbo].[RemoveContact]
	@DistinguishedName varchar(255)
AS
	DELETE FROM Contacts
		WHERE
			DistinguishedName=@DistinguishedName
RETURN
GO
/****** Object:  StoredProcedure [dbo].[RemoveOrganizationPlan]    Script Date: 01/15/2015 19:56:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 6/4/2013
-- Description:	Removes an organization plan
-- =============================================
CREATE PROCEDURE [dbo].[RemoveOrganizationPlan]
	@PlanID int
AS
BEGIN
	SET NOCOUNT ON;

    IF EXISTS(SELECT CompanyName FROM Companies WHERE OrgPlanID=@PlanID)
		BEGIN
			SELECT 0;
		END
	ELSE
		BEGIN
			DELETE FROM Plans_Organization WHERE OrgPlanID=@PlanID;
			SELECT 1;
		END

END
GO
/****** Object:  StoredProcedure [dbo].[UpdateReseller]    Script Date: 01/15/2015 19:56:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 12/3/2012
-- Description:	Updates the data for a reseller account
-- =============================================
CREATE PROCEDURE [dbo].[UpdateReseller]
	@CompanyId int,
	@CompanyName nvarchar(100),
	@Street nvarchar(255),
	@City nvarchar(100),
	@State nvarchar(100),
	@ZipCode int,
	@PhoneNumber nvarchar(50),
	@Website nvarchar(255),
	@Description ntext,
	@AdminName nvarchar(100),
	@AdminEmail nvarchar(255)
AS
	UPDATE Companies SET
		   CompanyName=@CompanyName,
		   Street=@Street,
		   City=@City,
		   State=@State,
		   ZipCode=@ZipCode,
		   PhoneNumber=@PhoneNumber,
		   Website=@Website,
		   Description=@Description,
		   AdminName=@AdminName,
		   AdminEmail=@AdminEmail
	WHERE
		   CompanyId=@CompanyId
	AND
		   IsReseller=1
RETURN
GO
/****** Object:  StoredProcedure [dbo].[UpdatePriceOverrideExchange]    Script Date: 01/15/2015 19:56:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 5/8/2013 
-- Description:	Updates custom pricing for a specific plan
-- =============================================
CREATE PROCEDURE [dbo].[UpdatePriceOverrideExchange]
	@CompanyCode varchar(10),
	@CustomPrice nvarchar(5),
	@PlanID int
AS
BEGIN
	SET NOCOUNT ON;

	IF EXISTS (SELECT Price FROM PriceOverride WHERE CompanyCode=@CompanyCode AND PlanID=@PlanID AND Product='Exchange')
		UPDATE
			PriceOverride
		SET
			Price=@CustomPrice
		WHERE
			CompanyCode=@CompanyCode
		AND
			PlanID=@PlanID
	ELSE
		INSERT INTO PriceOverride
			(
				CompanyCode,
				Price,
				PlanID,
				Product
			)
			VALUES
			(
				@CompanyCode,
				@CustomPrice,
				@PlanID,
				'Exchange'
			)
END
GO
/****** Object:  StoredProcedure [dbo].[UpdatePriceOverrideCitrix]    Script Date: 01/15/2015 19:56:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 5/8/2013 
-- Description:	Updates custom pricing for a specific plan
-- =============================================
CREATE PROCEDURE [dbo].[UpdatePriceOverrideCitrix]
	@CompanyCode varchar(10),
	@CustomPrice nvarchar(5),
	@PlanID int
AS
BEGIN
	SET NOCOUNT ON;

	IF EXISTS (SELECT Price FROM PriceOverride WHERE CompanyCode=@CompanyCode AND PlanID=@PlanID AND Product='Citrix')
		UPDATE
			PriceOverride
		SET
			Price=@CustomPrice
		WHERE
			CompanyCode=@CompanyCode
		AND
			PlanID=@PlanID
	ELSE
		INSERT INTO PriceOverride
			(
				CompanyCode,
				Price,
				PlanID,
				Product
			)
			VALUES
			(
				@CompanyCode,
				@CustomPrice,
				@PlanID,
				'Citrix'
			)
END
GO
/****** Object:  StoredProcedure [dbo].[UpdatePriceOverride]    Script Date: 01/15/2015 19:56:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdatePriceOverride]
	@CompanyCode varchar(255),
	@CustomPrice nvarchar(25),
	@PlanID int,
	@Product varchar(25)
AS
BEGIN
	SET NOCOUNT ON;

	IF EXISTS (SELECT Price FROM PriceOverride WHERE CompanyCode=@CompanyCode AND PlanID=@PlanID AND Product=@Product)
		UPDATE
			PriceOverride
		SET
			Price=@CustomPrice
		WHERE
			CompanyCode=@CompanyCode
		AND
			PlanID=@PlanID
		AND
			Product=@Product
	ELSE
		INSERT INTO PriceOverride
			(
				CompanyCode,
				Price,
				PlanID,
				Product
			)
			VALUES
			(
				@CompanyCode,
				@CustomPrice,
				@PlanID,
				@Product
			)
END
GO
/****** Object:  StoredProcedure [dbo].[UpdateOrganizationPlan]    Script Date: 01/15/2015 19:56:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 12/9/2012
-- Description:	Updates a organization plan
-- =============================================
CREATE PROCEDURE [dbo].[UpdateOrganizationPlan]
	@OrgPlanID int,
	@OrgPlanName nvarchar(50),
	@MaxUsers int,
	@MaxDomains int,
	@MaxExchangeMailboxes int,
	@MaxExchangeContacts int,
	@MaxExchangeDistLists int,
	@MaxExchangePublicFolders int,
	@MaxExchangeMailPublicFolders int,
	@MaxExchangeKeepDeletedItems int,
	@MaxTerminalServerUsers int
AS
	UPDATE Plans_Organization
		   SET
				OrgPlanName=@OrgPlanName,
				MaxUsers=@MaxUsers,
				MaxDomains=@MaxDomains,
				MaxExchangeMailboxes=@MaxExchangeMailboxes,
				MaxExchangeContacts=@MaxExchangeContacts,
				MaxExchangeDistLists=@MaxExchangeDistLists,
				MaxExchangePublicFolders=@MaxExchangePublicFolders,
				MaxExchangeMailPublicFolders=@MaxExchangeMailPublicFolders,
				MaxExchangeKeepDeletedItems=@MaxExchangeKeepDeletedItems,
				MaxTerminalServerUsers=@MaxTerminalServerUsers
			WHERE
				OrgPlanID=@OrgPlanID
	
RETURN
GO
/****** Object:  StoredProcedure [dbo].[UpdateNotification]    Script Date: 01/15/2015 19:56:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:	Jacob Dixon
-- Create date: 2/23/2013
-- Description:	Adds or updates a notification
-- =============================================
CREATE PROCEDURE [dbo].[UpdateNotification] 
	@CompanyCode varchar(10),
	@Product varchar(50),
	@HTML ntext,
	@IsEnabled bit
AS

	DECLARE @NotificationCount int;
	SET @NotificationCount = (SELECT COUNT(NotificationID) FROM Notifications WHERE Product=@Product AND CompanyID=(SELECT CompanyID FROM Companies WHERE CompanyCode=@CompanyCode))
	
	if @NotificationCount > 0
		UPDATE Notifications
			SET
				HTML=@HTML,
				IsEnabled=@IsEnabled
			WHERE
				Product=@Product AND CompanyID=(SELECT CompanyID FROM Companies WHERE CompanyCode=@CompanyCode)
	else
		INSERT INTO Notifications
			(
				CompanyID,
				Product,
				HTML,
				IsEnabled
			)
			VALUES
			(
				(SELECT CompanyID FROM Companies WHERE CompanyCode=@CompanyCode),
				@Product,
				@HTML,
				@IsEnabled
			)

RETURN
GO
/****** Object:  StoredProcedure [dbo].[UpdateExchangeMailboxPlan]    Script Date: 01/15/2015 19:56:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 11/30/2012
-- Description:	Updates an existing Exchange Mailbox Plan
-- =============================================
CREATE PROCEDURE [dbo].[UpdateExchangeMailboxPlan]
	@MailboxPlanID int,
	@MailboxPlanName nvarchar(50),
	@MailboxSizeMB int,
	@MaxSendKB int,
	@MaxReceiveKB int,
	@MaxRecipients int,
	@EnablePOP3 bit,
	@EnableIMAP bit,
	@EnableOWA bit,
	@EnableMAPI bit,
	@EnableAS bit,
	@EnableECP bit,
	@MaxKeepDeletedItems int,
	@MailboxPlanDesc ntext,
	@CompanyCode varchar(10),
	@Price nvarchar(7),
	@Cost nvarchar(7)
AS
	UPDATE Plans_ExchangeMailbox
		   SET
				MailboxPlanName=@MailboxPlanName,
				MailboxSizeMB=@MailboxSizeMB,
				MaxSendKB=@MaxSendKB,
				MaxReceiveKB=@MaxReceiveKB,
				MaxRecipients=@MaxRecipients,
				EnablePOP3=@EnablePOP3,
				EnableIMAP=@EnableIMAP,
				EnableOWA=@EnableOWA,
				EnableMAPI=@EnableMAPI,
				EnableAS=@EnableAS,
				EnableECP=@EnableECP,
				MaxKeepDeletedItems=@MaxKeepDeletedItems,
				MailboxPlanDesc=@MailboxPlanDesc,
				CompanyCode=@CompanyCode,
				Price=@Price,
				Cost=@Cost
			WHERE
				MailboxPlanID=@MailboxPlanID
	RETURN
GO
/****** Object:  StoredProcedure [dbo].[UpdateDistributionGroup]    Script Date: 01/15/2015 19:56:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 1/28/2013
-- Description: Updates and existing distribution group
-- =============================================
CREATE PROCEDURE [dbo].[UpdateDistributionGroup]
	@DistinguishedName varchar(255),
	@DisplayName varchar(255),
	@Email varchar(255),
	@Hidden bit
AS
	UPDATE DistributionGroups
		SET
			DisplayName=@DisplayName,
			Email=@Email,
			Hidden=@Hidden
		WHERE
			DistinguishedName=@DistinguishedName
RETURN
GO
/****** Object:  StoredProcedure [dbo].[UpdateContact]    Script Date: 01/15/2015 19:56:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 1/15/2013
-- Description:	Updates an Exchange contact
-- =============================================
CREATE PROCEDURE [dbo].[UpdateContact]
	@DistinguishedName varchar(255),
	@DisplayName varchar(255),
	@Email varchar(255)
AS
	UPDATE Contacts
		SET
			DisplayName=@DisplayName,
			Email=@Email
		WHERE
			DistinguishedName=@DistinguishedName
RETURN
GO
/****** Object:  StoredProcedure [dbo].[UpdateCompanyExch]    Script Date: 01/15/2015 19:56:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 1/10/2013
-- Description:	Updates the exchange status for a company
-- =============================================
CREATE PROCEDURE [dbo].[UpdateCompanyExch]
	@CompanyCode varchar(10),
	@ExchEnabled bit,
	@ExchPFEnabled int
AS
	UPDATE Companies
	SET
		ExchEnabled = @ExchEnabled
	WHERE
		CompanyCode=@CompanyCode
RETURN
GO
/****** Object:  StoredProcedure [dbo].[UpdateCompany]    Script Date: 01/15/2015 19:56:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 12/3/2012
-- Description:	Updates the data for a company account
-- =============================================
CREATE PROCEDURE [dbo].[UpdateCompany]
	@CompanyCode varchar(10),
	@CompanyName nvarchar(100),
	@OrgPlanID int,
	@Street nvarchar(255),
	@City nvarchar(100),
	@State nvarchar(100),
	@ZipCode int,
	@PhoneNumber nvarchar(50),
	@Website nvarchar(255),
	@Description ntext,
	@AdminName nvarchar(100),
	@AdminEmail nvarchar(255)
AS
	UPDATE Companies SET
		   CompanyName=@CompanyName,
		   OrgPlanID=@OrgPlanID,
		   Street=@Street,
		   City=@City,
		   State=@State,
		   ZipCode=@ZipCode,
		   PhoneNumber=@PhoneNumber,
		   Website=@Website,
		   Description=@Description,
		   AdminName=@AdminName,
		   AdminEmail=@AdminEmail
	WHERE
		   CompanyCode=@CompanyCode
	AND
		   IsReseller=0
RETURN
GO
/****** Object:  StoredProcedure [dbo].[UpdateCitrixApp]    Script Date: 01/15/2015 19:56:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 6/30/2013
-- Description:	Updates a Citrix App
-- =============================================
CREATE PROCEDURE [dbo].[UpdateCitrixApp]
	@CitrixPlanID int,
	@Name varchar(20),
	@Description ntext,
	@CompanyCode varchar(10),
	@IsServer bit,
	@Price nvarchar(6)
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE Plans_Citrix
		SET
			Name=@Name,
			Description=@Description,
			CompanyCode=@CompanyCode,
			IsServer=@IsServer,
			Price=@Price
		WHERE
			CitrixPlanID=@CitrixPlanID
END
GO
/****** Object:  StoredProcedure [dbo].[UpdateAcceptedDomain]    Script Date: 01/15/2015 19:56:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 1/10/2013
-- Description:	Changes the status of the domain if it is an accepted domain or not
-- =============================================
CREATE PROCEDURE [dbo].[UpdateAcceptedDomain] 
	@DomainID int,
	@IsAcceptedDomain bit
AS
	UPDATE Domains
	SET
		IsAcceptedDomain=@IsAcceptedDomain
	WHERE
		DomainID=@DomainID
RETURN
GO
/****** Object:  Table [dbo].[Users]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[Users](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserGuid] [uniqueidentifier] NOT NULL,
	[CompanyCode] [varchar](255) NULL,
	[UserPrincipalName] [nvarchar](255) NOT NULL,
	[DistinguishedName] [varchar](255) NULL,
	[DisplayName] [nvarchar](100) NOT NULL,
	[Firstname] [nvarchar](50) NULL,
	[Middlename] [nvarchar](50) NULL,
	[Lastname] [nvarchar](50) NULL,
	[Email] [nvarchar](255) NULL,
	[Department] [nvarchar](255) NULL,
	[IsCompanyAdmin] [bit] NULL,
	[MailboxPlan] [int] NULL,
	[TSPlan] [int] NULL,
	[LyncPlan] [int] NULL,
	[Created] [datetime] NULL,
	[AdditionalMB] [int] NULL,
	[sAMAccountName] [nvarchar](255) NULL,
	[IsResellerAdmin] [bit] NULL,
	[ActiveSyncPlan] [int] NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[UserGuid] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING OFF
GO
/****** Object:  Table [dbo].[UserPlansCitrix]    Script Date: 01/15/2015 19:56:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserPlansCitrix](
	[UserID] [int] NOT NULL,
	[CitrixPlanID] [int] NOT NULL,
	[UPCID] [int] IDENTITY(1,1) NOT NULL,
 CONSTRAINT [PK_UserPlansCitrix] PRIMARY KEY CLUSTERED 
(
	[UPCID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[UpdateUser]    Script Date: 01/15/2015 19:56:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 12/9/2012
-- Description:	Updates a user in the database
-- =============================================
CREATE PROCEDURE [dbo].[UpdateUser]
	@UserGuid uniqueidentifier,
	@CompanyCode varchar(10),
	@DisplayName nvarchar(100),
	@Firstname nvarchar(50),
	@Middlename nvarchar(50),
	@Lastname nvarchar(50),
	@Department nvarchar(255),
	@IsCompanyAdmin bit
AS
	UPDATE Users SET
				 CompanyCode=@CompanyCode,
				 DisplayName=@Displayname,
				 Firstname=@Firstname,
				 Middlename=@Middlename,
				 Lastname=@Lastname,
				 Department=@Department,
				 IsCompanyAdmin=@IsCompanyAdmin
	WHERE
			UserGuid=@UserGuid
RETURN
GO
/****** Object:  StoredProcedure [dbo].[UnassignMailboxPlan]    Script Date: 01/15/2015 19:56:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 1/9/2013
-- Description:	Unassigns a user from a mailbox plan
-- =============================================
CREATE PROCEDURE [dbo].[UnassignMailboxPlan]
	@UserGuid uniqueidentifier
AS
	UPDATE Users
	SET 
		MailboxPlan = NULL
	WHERE
		UserGuid=@UserGuid
	
RETURN
GO
/****** Object:  StoredProcedure [dbo].[UnassignCitrixPlan]    Script Date: 01/15/2015 19:56:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 4/12/2013
-- Description:	Unassigns a Citrix Plan to a User
-- =============================================
CREATE PROCEDURE [dbo].[UnassignCitrixPlan]
	@UserPrincipalName varchar(255),
	@CitrixPlanID int
AS
BEGIN
	SET NOCOUNT ON;

    -- Store the UserID based on the UserPrincipalName
	DECLARE @UsersID int
	SET @UsersID = ( SELECT ID FROM Users WHERE UserPrincipalName=@UserPrincipalName )

	-- Don't insert if it already exists
	IF EXISTS ( SELECT UserID FROM UserPlansCitrix WHERE UserID=@UsersID AND CitrixPlanID=@CitrixPlanID )
		BEGIN
			DELETE FROM UserPlansCitrix
				WHERE
					UserID=@UsersID AND CitrixPlanID=@CitrixPlanID
		END

END
GO
/****** Object:  StoredProcedure [dbo].[RemoveUser]    Script Date: 01/15/2015 19:56:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 12/9/2012
-- Description:	Removes a user
-- =============================================
CREATE PROCEDURE [dbo].[RemoveUser]
	@UserGuid uniqueidentifier,
	@CompanyCode varchar(10)
AS
	DELETE FROM Users
		   WHERE UserGuid=@UserGuid AND CompanyCode=@CompanyCode
RETURN
GO
/****** Object:  StoredProcedure [dbo].[RemoveExchangeMailboxPlan]    Script Date: 01/15/2015 19:56:48 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 6/4/2013
-- Description:	Remove mailbox plan
-- =============================================
CREATE PROCEDURE [dbo].[RemoveExchangeMailboxPlan]
	@PlanID int
AS
BEGIN
	SET NOCOUNT ON;

    IF EXISTS(SELECT UserPrincipalName FROM Users WHERE MailboxPlan=@PlanID)
		BEGIN
			SELECT 0;
		END
	ELSE
		BEGIN
			DELETE FROM Plans_ExchangeMailbox WHERE MailboxPlanID=@PlanID;
			SELECT 1;
		END

END
GO
/****** Object:  StoredProcedure [dbo].[RemoveCompany]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 12/4/2012
-- Description:	Removes a company from all the tables
-- =============================================
CREATE PROCEDURE [dbo].[RemoveCompany]
	@CompanyCode varchar(10)
AS
	BEGIN TRAN
		DELETE FROM Domains WHERE CompanyCode=@CompanyCode;
		DELETE FROM Prices WHERE CompanyCode=@CompanyCode;
		DELETE FROM UserPlans WHERE CompanyCode=@CompanyCode;
		DELETE FROM Companies WHERE CompanyCode=@CompanyCode AND IsReseller=0;
		DELETE FROM Users WHERE CompanyCode=@CompanyCode;
		DELETE FROM Contacts WHERE CompanyCode=@CompanyCode;
		DELETE FROM DistributionGroups WHERE CompanyCode=@CompanyCode;
	COMMIT
RETURN
GO
/****** Object:  StoredProcedure [dbo].[GetUser]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 1/28/2013
-- Description:	Gets a user from the database
-- =============================================
CREATE PROCEDURE [dbo].[GetUser]
	@ID int
AS
	SELECT ID,
		   CompanyCode,
		   UserGuid,
		   UserPrincipalName,
		   DistinguishedName,
		   DisplayName,
		   Firstname,
		   Middlename,
		   Lastname,
		   Email,
		   IsCompanyAdmin,
		   MailboxPlan,
		   TSPlan,
		   LyncPlan,
		   Created
	FROM
		   Users
	WHERE
		   ID=@ID
RETURN
GO
/****** Object:  StoredProcedure [dbo].[GetForwardingList]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 1/29/2013
-- Description: Gets a list of contacts, distribution lists, and users for the purpose of fowarding in Exchange
-- =============================================
CREATE PROCEDURE [dbo].[GetForwardingList]
	@CompanyCode varchar(10)
AS

	SELECT DistinguishedName, UserGuid, DisplayName FROM Users WHERE MailboxPlan > 0 AND CompanyCode=@CompanyCode
	
	SELECT DistinguishedName, DisplayName, Email FROM Contacts WHERE CompanyCode=@CompanyCode
	
	SELECT DistinguishedName, DisplayName, Email FROM DistributionGroups WHERE CompanyCode=@CompanyCode

RETURN
GO
/****** Object:  StoredProcedure [dbo].[GetExchangeUsers]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 1/9/2013
-- Description:	Get all users that have a mailbox plan
-- =============================================
CREATE PROCEDURE [dbo].[GetExchangeUsers]
	@CompanyCode varchar(10)
AS
	SELECT ID,
		   UserGuid,
		   UserPrincipalName,
		   DisplayName,
		   Firstname,
		   Middlename,
		   Lastname,
		   Email,
		   MailboxPlan
	FROM
		   Users
	WHERE
		   CompanyCode=@CompanyCode
	AND
		   MailboxPlan IS NOT NULL AND MailboxPlan <> 0
	
RETURN
GO
/****** Object:  StoredProcedure [dbo].[GetUsersCitrixPlans]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 4/12/2013
-- Description:	Gets a list of Citrix Plans assigned to a user
-- =============================================
CREATE PROCEDURE [dbo].[GetUsersCitrixPlans]
	@UserPrincipalName varchar(255)
AS
BEGIN
	SET NOCOUNT ON;

    -- Store the UserID based on the UserPrincipalName
	DECLARE @UsersID int
	SET @UsersID = ( SELECT ID FROM Users WHERE UserPrincipalName=@UserPrincipalName )

	SELECT
		u.UserID,
		u.CitrixPlanID,
		p.Name,
		p.GroupName,
		p.Description,
		p.CompanyCode,
		p.IsServer,
		p.Price
	FROM
		UserPlansCitrix u
	INNER JOIN
		Plans_Citrix p
	ON
		u.CitrixPlanID = p.CitrixPlanID
	WHERE
		u.UserID = @UsersID
END
GO
/****** Object:  StoredProcedure [dbo].[AssignMailboxPlan]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 1/9/2013
-- Description:	Assigned a mailbox plan to a user
-- =============================================
CREATE PROCEDURE [dbo].[AssignMailboxPlan] 
	@UserGuid uniqueidentifier,
	@PlanID int,
	@DisplayName varchar(255),
	@Email varchar(255)
AS
	Update Users
	SET 
		MailboxPlan=@PlanID,
		Email=@Email
	WHERE
		UserGuid=@UserGuid
		
	IF @DisplayName IS NOT NULL
		UPDATE Users SET DisplayName=@DisplayName WHERE UserGuid=@UserGuid	
RETURN
GO
/****** Object:  StoredProcedure [dbo].[AssignLyncPlan]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 2/23/2013
-- Description:	Assign Lync Plan
-- =============================================
CREATE PROCEDURE [dbo].[AssignLyncPlan]
	@UserGuid uniqueidentifier,
	@PlanID int
AS
	
	UPDATE Users
	SET
		LyncPlan = @PlanID
	WHERE
		UserGuid = @UserGuid

RETURN
GO
/****** Object:  StoredProcedure [dbo].[AssignCitrixPlan]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 4/12/2013
-- Description:	Assigns a Citrix Plan to a User
-- =============================================
CREATE PROCEDURE [dbo].[AssignCitrixPlan]
	@UserPrincipalName varchar(255),
	@CitrixPlanID int
AS
BEGIN
	SET NOCOUNT ON;

    -- Store the UserID based on the UserPrincipalName
	DECLARE @UsersID int
	SET @UsersID = ( SELECT ID FROM Users WHERE UserPrincipalName=@UserPrincipalName )

	-- Don't insert if it already exists
	IF NOT EXISTS ( SELECT UserID FROM UserPlansCitrix WHERE UserID=@UsersID AND CitrixPlanID=@CitrixPlanID )
		BEGIN
			INSERT INTO UserPlansCitrix
				(
					UserID,
					CitrixPlanID
				)
				VALUES
				(
					@UsersID,
					@CitrixPlanID
				)
		END

END
GO
/****** Object:  StoredProcedure [dbo].[GetCompanyUsers]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 12/15/2012
-- Description:	Gets a list of company users
-- =============================================
CREATE PROCEDURE [dbo].[GetCompanyUsers]
	@CompanyCode varchar(10)
AS
	SELECT  ID,
			UserGuid,
			UserPrincipalName,
			Displayname,
			Firstname,
			Middlename,
			Lastname,
			Email,
			IsCompanyAdmin,
			MailboxPlan,
			(SELECT MailboxPlanName FROM Plans_ExchangeMailbox WHERE MailboxPlanID=Users.MailboxPlan) AS MailboxPlanName,
			LyncPlan,
			TSPlan,
			Department,
			Created
	FROM
			Users
	WHERE
			CompanyCode=@CompanyCode
	ORDER BY
			DisplayName
RETURN
GO
/****** Object:  StoredProcedure [dbo].[GetCitrixPlans]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 4/11/2013
-- Description:	Gets a list of citrix plans
-- =============================================
CREATE PROCEDURE [dbo].[GetCitrixPlans]
AS
BEGIN
	SET NOCOUNT ON;

    SELECT CitrixPlanID,
		   Name,
		   GroupName,
		   Description,
		   IsServer,
		   CompanyCode,
		   (SELECT COUNT(CitrixPlanID) FROM UserPlansCitrix WHERE p.CitrixPlanID=CitrixPlanID) AS COUNT,
		   Price
	FROM
		   Plans_Citrix p
END
GO
/****** Object:  StoredProcedure [dbo].[CountCompanyInfo]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 12/15/2012
-- Description:	Count Companies Domains and Users
-- =============================================
CREATE PROCEDURE [dbo].[CountCompanyInfo]
	@CompanyCode varchar(10)
AS

	SELECT COUNT(Domain) AS DomainCount FROM Domains WHERE CompanyCode=@CompanyCode
	
	SELECT COUNT(UserGuid) AS UserCount FROM Users WHERE CompanyCode=@CompanyCode
	
	SELECT COUNT(DistinguishedName) AS ContactCount FROM Contacts WHERE CompanyCode=@CompanyCode
	
	SELECT COUNT(UserGuid) AS MailboxCount FROM Users WHERE CompanyCode=@CompanyCode AND (MailboxPlan IS NOT NULL AND MailboxPlan <> 0)
	
	SELECT COUNT(DistinguishedName) AS DistGroupCount FROM DistributionGroups WHERE CompanyCode=@CompanyCode
	
	SELECT COUNT(DISTINCT UserID) AS CitrixCount FROM UserPlansCitrix p 
		INNER JOIN
			Users
		ON
			Users.ID = p.UserID
		WHERE
			Users.CompanyCode = @CompanyCode
	
RETURN
GO
/****** Object:  StoredProcedure [dbo].[AddUser]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jacob Dixon
-- Create date: 12/9/2012
-- Description:	Adds a new user to the database
-- =============================================
CREATE PROCEDURE [dbo].[AddUser]
	@UserGuid uniqueidentifier,
	@CompanyCode varchar(10),
	@UserPrincipalName nvarchar(255),
	@DisplayName nvarchar(100),
	@Firstname nvarchar(50),
	@Middlename nvarchar(50),
	@Lastname nvarchar(50),
	@Email nvarchar(255),
	@DistinguishedName varchar(255),
	@IsCompanyAdmin bit,
	@Department nvarchar(255)
AS
	INSERT INTO Users
				(
					UserGuid,
					CompanyCode,
					UserPrincipalName,
					DisplayName,
					Firstname,
					Middlename,
					Lastname,
					Email,
					DistinguishedName,
					IsCompanyAdmin,
					Department,
					Created
				)
				VALUES
				(
					@UserGuid,
					@CompanyCode,
					@UserPrincipalName,
					@DisplayName,
					@Firstname,
					@Middlename,
					@Lastname,
					@Email,
					@DistinguishedName,
					@IsCompanyAdmin,
					@Department,
					GETDATE()
				)
RETURN
GO
/****** Object:  StoredProcedure [dbo].[DisableExchange]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DisableExchange]
	@CompanyCode varchar(255)
AS
BEGIN
	SET NOCOUNT ON;

	/* DELETE CONTACTS */
    DELETE FROM Contacts WHERE CompanyCode=@CompanyCode;
    
    /* DELETE DISTRIBUTION GROUPS */
    DELETE FROM DistributionGroups WHERE CompanyCode=@CompanyCode;
    
    /* RESET ALL DOMAINS TO NOT ACCEPTED DOMAINS */
    UPDATE Domains SET IsAcceptedDomain=0 WHERE CompanyCode=@CompanyCode;
    
    /* RESET ALL USERS MAILBOX PLAN */
    UPDATE Users SET MailboxPlan=0 WHERE CompanyCode=@CompanyCode;
    
    /* RESET COMPANY TO EXCHANGE DISABLED AND PUBLIC FOLDER DISABLED */
    UPDATE Companies SET ExchPFPlan=0, ExchEnabled=0 WHERE CompanyCode=@CompanyCode;
    
END
GO
/****** Object:  StoredProcedure [dbo].[DeleteCompany]    Script Date: 01/15/2015 19:56:47 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DeleteCompany]
	@CompanyCode varchar(255)
AS
BEGIN
	SET NOCOUNT ON;

    /* RUN THE DISABLE EXCHANGE AGAIN */
    exec dbo.DisableExchange @CompanyCode;
    
    /* DELETE CITRIX PLANS ASSIGNED TO USERS */
    DELETE FROM UserPlansCitrix WHERE UserID IN (SELECT ID FROM Users WHERE CompanyCode=@CompanyCode);
    
    /* DELETE PRICE OVERRIDE STUFF */
    DELETE FROM PriceOverride WHERE CompanyCode=@CompanyCode;
    
    /* DELETE PLANS ASSIGNED TO COMPANIES */
    DELETE FROM Plans_Citrix WHERE CompanyCode=@CompanyCode;
    DELETE FROM Plans_ExchangeActiveSync WHERE CompanyCode=@CompanyCode;
    DELETE FROM Plans_ExchangeMailbox WHERE CompanyCode=@CompanyCode;
    
    /* DELETE ALL USERS */
    DELETE FROM Users WHERE CompanyCode=@CompanyCode;
    
    /* DELETE DOMAINS */
    DELETE FROM Domains WHERE CompanyCode=@CompanyCode;
    
    /* REMOVE COMPANY */
    DELETE FROM Companies WHERE CompanyCode=@CompanyCode AND IsReseller=0;
    
    
END
GO
