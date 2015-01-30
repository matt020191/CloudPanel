SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ApiAccess]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ApiAccess](
	[CustomerKey] [varchar](255) NOT NULL,
	[CustomerSecret] [varchar](255) NOT NULL,
	[CompanyCode] [varchar](255) NOT NULL,
 CONSTRAINT [PK_ApiAccess] PRIMARY KEY CLUSTERED 
(
	[CustomerKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Audit]') AND type in (N'U'))
BEGIN
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AuditLogin]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AuditLogin](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[IPAddress] [varchar](128) NOT NULL,
	[Username] [varchar](255) NOT NULL,
	[LoginStatus] [bit] NOT NULL,
	[AuditTimeStamp] [datetime] NOT NULL,
 CONSTRAINT [PK_AuditLogin] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Companies]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Companies](
	[CompanyId] [int] IDENTITY(1,1) NOT NULL,
	[IsReseller] [bit] NOT NULL,
	[ResellerCode] [nvarchar](255) NULL,
	[OrgPlanID] [int] NULL,
	[CompanyName] [nvarchar](100) NOT NULL,
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CompanyStats]') AND type in (N'U'))
BEGIN
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Contacts]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Contacts](
	[DistinguishedName] [varchar](255) NOT NULL,
	[CompanyCode] [varchar](255) NULL,
	[DisplayName] [varchar](255) NOT NULL,
	[Email] [varchar](255) NOT NULL,
	[Hidden] [bit] NOT NULL,
 CONSTRAINT [PK_Contacts] PRIMARY KEY CLUSTERED 
(
	[DistinguishedName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DistributionGroups]') AND type in (N'U'))
BEGIN
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Domains]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Domains](
	[DomainID] [int] IDENTITY(1,1) NOT NULL,
	[CompanyCode] [varchar](255) NULL,
	[Domain] [nvarchar](255) NOT NULL,
	[IsSubDomain] [bit] NULL,
	[IsLyncDomain] [bit] NULL,
	[IsDefault] [bit] NOT NULL,
	[IsAcceptedDomain] [bit] NOT NULL,
 CONSTRAINT [PK_Domains] PRIMARY KEY CLUSTERED 
(
	[DomainID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Plans_Citrix]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Plans_Citrix](
	[CitrixPlanID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](56) NOT NULL,
	[GroupName] [varchar](64) NOT NULL,
	[Description] [ntext] NULL,
	[IsServer] [bit] NOT NULL,
	[CompanyCode] [varchar](255) NULL,
	[Price] [nvarchar](20) NULL,
	[Cost] [nvarchar](20) NULL,
	[PictureURL] [varchar](255) NULL,
 CONSTRAINT [PK_Plans_Citrix] PRIMARY KEY CLUSTERED 
(
	[CitrixPlanID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Plans_ExchangeActiveSync]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Plans_ExchangeActiveSync](
	[ASID] [int] IDENTITY(1,1) NOT NULL,
	[CompanyCode] [varchar](255) NOT NULL,
	[DisplayName] [varchar](150) NOT NULL,
	[Description] [ntext] NULL,
	[ExchangeName] [varchar](75) NULL,
	[AllowNonProvisionableDevices] [bit] NULL,
	[RefreshIntervalInHours] [int] NULL,
	[RequirePassword] [bit] NULL,
	[RequireAlphanumericPassword] [bit] NULL,
	[EnablePasswordRecovery] [bit] NULL,
	[RequireEncryptionOnDevice] [bit] NULL,
	[RequireEncryptionOnStorageCard] [bit] NULL,
	[AllowSimplePassword] [bit] NULL,
	[NumberOfFailedAttempted] [int] NULL,
	[MinimumPasswordLength] [int] NULL,
	[InactivityTimeoutInMinutes] [int] NULL,
	[PasswordExpirationInDays] [int] NULL,
	[EnforcePasswordHistory] [int] NULL,
	[IncludePastCalendarItems] [varchar](20) NULL,
	[IncludePastEmailItems] [varchar](20) NULL,
	[LimitEmailSizeInKB] [int] NULL,
	[AllowDirectPushWhenRoaming] [bit] NULL,
	[AllowHTMLEmail] [bit] NULL,
	[AllowAttachmentsDownload] [bit] NULL,
	[MaximumAttachmentSizeInKB] [int] NULL,
	[AllowRemovableStorage] [bit] NULL,
	[AllowCamera] [bit] NULL,
	[AllowWiFi] [bit] NULL,
	[AllowInfrared] [bit] NULL,
	[AllowInternetSharing] [bit] NULL,
	[AllowRemoteDesktop] [bit] NULL,
	[AllowDesktopSync] [bit] NULL,
	[AllowBluetooth] [varchar](10) NULL,
	[AllowBrowser] [bit] NULL,
	[AllowConsumerMail] [bit] NULL,
	[IsEnterpriseCAL] [bit] NULL,
	[AllowTextMessaging] [bit] NULL,
	[AllowUnsignedApplications] [bit] NULL,
	[AllowUnsignedInstallationPackages] [bit] NULL,
 CONSTRAINT [PK_Plans_ExchangeActiveSync] PRIMARY KEY CLUSTERED 
(
	[ASID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Plans_ExchangeMailbox]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Plans_ExchangeMailbox](
	[MailboxPlanID] [int] IDENTITY(1,1) NOT NULL,
	[MailboxPlanName] [nvarchar](50) NOT NULL,
	[ProductID] [int] NULL,
	[ResellerCode] [varchar](255) NULL,
	[CompanyCode] [varchar](255) NULL,
	[MailboxSizeMB] [int] NOT NULL,
	[MaxMailboxSizeMB] [int] NULL,
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
	[Price] [nvarchar](20) NULL,
	[Cost] [nvarchar](20) NULL,
	[AdditionalGBPrice] [nvarchar](20) NULL,
 CONSTRAINT [PK_Plans_ExchangeMailbox] PRIMARY KEY CLUSTERED 
(
	[MailboxPlanID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Plans_Organization]') AND type in (N'U'))
BEGIN
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
	[MaxExchangeActivesyncPolicies] [int] NULL,
	[MaxTerminalServerUsers] [int] NOT NULL,
	[MaxExchangeResourceMailboxes] [int] NULL,
 CONSTRAINT [PK_Plans_Organization] PRIMARY KEY CLUSTERED 
(
	[OrgPlanID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Plans_TerminalServices]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Plans_TerminalServices](
	[TSPlanID] [int] NOT NULL,
	[TSPlanName] [nvarchar](50) NOT NULL,
	[ResellerCode] [varchar](255) NOT NULL,
	[ProductID] [int] NOT NULL,
	[MaxUserSpaceMB] [int] NULL
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PriceOverride]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[PriceOverride](
	[CompanyCode] [varchar](255) NOT NULL,
	[Price] [nvarchar](25) NULL,
	[PlanID] [int] NULL,
	[Product] [varchar](25) NULL
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Prices]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Prices](
	[PriceID] [int] IDENTITY(1,1) NOT NULL,
	[ProductID] [int] NOT NULL,
	[PlanID] [int] NOT NULL,
	[CompanyCode] [varchar](255) NOT NULL,
	[Price] [money] NOT NULL,
 CONSTRAINT [PK_Prices] PRIMARY KEY CLUSTERED 
(
	[PriceID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ResourceMailboxes]') AND type in (N'U'))
BEGIN
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Settings]') AND type in (N'U'))
BEGIN
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
	[SupportMailPort] [int] NULL,
	[SupportMailUsername] [varchar](255) NULL,
	[SupportMailPassword] [varchar](255) NULL,
	[SupportMailFrom] [varchar](255) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Stats_CitrixCount]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Stats_CitrixCount](
	[StatDate] [date] NOT NULL,
	[UserCount] [int] NOT NULL,
 CONSTRAINT [PK_Stats_CitrixCount] PRIMARY KEY CLUSTERED 
(
	[StatDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Stats_ExchCount]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Stats_ExchCount](
	[StatDate] [date] NOT NULL,
	[UserCount] [int] NOT NULL,
 CONSTRAINT [PK_Stats_ExchCount] PRIMARY KEY CLUSTERED 
(
	[StatDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Stats_UserCount]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Stats_UserCount](
	[StatDate] [date] NOT NULL,
	[UserCount] [int] NOT NULL,
 CONSTRAINT [PK_Stats_UserCount] PRIMARY KEY CLUSTERED 
(
	[StatDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SvcMailboxDatabaseSizes]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[SvcMailboxDatabaseSizes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[DatabaseName] [nvarchar](64) NOT NULL,
	[Server] [nvarchar](64) NOT NULL,
	[DatabaseSize] [nvarchar](255) NOT NULL,
	[Retrieved] [datetime] NOT NULL,
 CONSTRAINT [PK_SvcMailboxDatabaseSizes_1] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SvcMailboxSizes]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[SvcMailboxSizes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserPrincipalName] [varchar](64) NOT NULL,
	[MailboxDatabase] [varchar](255) NOT NULL,
	[TotalItemSizeInKB] [varchar](255) NOT NULL,
	[TotalDeletedItemSizeInKB] [varchar](255) NOT NULL,
	[ItemCount] [int] NOT NULL,
	[DeletedItemCount] [int] NOT NULL,
	[Retrieved] [datetime] NOT NULL,
 CONSTRAINT [PK_SvcMailboxSizes] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SvcQueue]') AND type in (N'U'))
BEGIN
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SvcTask]') AND type in (N'U'))
BEGIN
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserPermissions]') AND type in (N'U'))
BEGIN
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
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserPlans]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[UserPlans](
	[UserGuid] [uniqueidentifier] NOT NULL,
	[ProductID] [int] NOT NULL,
	[PlanID] [int] NOT NULL,
	[CompanyCode] [varchar](255) NULL
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserPlansCitrix]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[UserPlansCitrix](
	[UPCID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[CitrixPlanID] [int] NOT NULL,
 CONSTRAINT [PK_UserPlansCitrix] PRIMARY KEY CLUSTERED 
(
	[UPCID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Users](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserGuid] [uniqueidentifier] NOT NULL,
	[CompanyCode] [varchar](255) NULL,
	[sAMAccountName] [nvarchar](255) NULL,
	[UserPrincipalName] [nvarchar](255) NOT NULL,
	[DistinguishedName] [varchar](255) NULL,
	[DisplayName] [nvarchar](100) NOT NULL,
	[Firstname] [nvarchar](50) NULL,
	[Middlename] [nvarchar](50) NULL,
	[Lastname] [nvarchar](50) NULL,
	[Email] [nvarchar](255) NULL,
	[Department] [nvarchar](255) NULL,
	[IsResellerAdmin] [bit] NULL,
	[IsCompanyAdmin] [bit] NULL,
	[MailboxPlan] [int] NULL,
	[TSPlan] [int] NULL,
	[LyncPlan] [int] NULL,
	[Created] [datetime] NULL,
	[AdditionalMB] [int] NULL,
	[ActiveSyncPlan] [int] NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[UserGuid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[Trigger_CitrixStats]'))
EXEC dbo.sp_executesql @statement = N'
CREATE TRIGGER [dbo].[Trigger_CitrixStats] ON [dbo].[UserPlansCitrix]
                AFTER INSERT,UPDATE,DELETE
AS
BEGIN
 
                DECLARE @UserCount int;
                SET @UserCount = (SELECT COUNT(DISTINCT UserId) FROM UserPlansCitrix);
               
                DECLARE @TodaysDate date;
                SET @TodaysDate = CONVERT(date, getdate());
 
                /* USER COUNT */
                IF EXISTS (SELECT StatDate FROM Stats_CitrixCount WHERE StatDate = @TodaysDate)
                                UPDATE Stats_CitrixCount
                                                SET UserCount=@UserCount
                                WHERE
                                                StatDate = @TodaysDate
               
                ELSE
                                INSERT INTO Stats_CitrixCount
                                                (
                                                                StatDate,
                                                                Usercount
                                                )
                                                VALUES
                                                (
                                                                @TodaysDate,
                                                                @UserCount
                                                )
END
' 
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[Trigger_UserStats]'))
EXEC dbo.sp_executesql @statement = N'CREATE TRIGGER [dbo].[Trigger_UserStats] ON [dbo].[Users]
                AFTER INSERT,UPDATE,DELETE
AS
BEGIN
 
                DECLARE @UserCount int;
                SET @UserCount = (SELECT COUNT(UserPrincipalName) FROM Users);
 
                DECLARE @ExchCount int;
                SET @ExchCount = (SELECT COUNT(MailboxPlan) FROM Users WHERE MailboxPlan IS NOT NULL AND MailboxPlan <> 0);
               
                DECLARE @TodaysDate date;
                SET @TodaysDate = CONVERT(date, getdate());
 
                /* USER COUNT */
                IF EXISTS (SELECT StatDate FROM Stats_UserCount WHERE StatDate = @TodaysDate)
                                UPDATE Stats_UserCount
                                                SET UserCount=@UserCount
                                WHERE
                                                StatDate = @TodaysDate
               
                ELSE
                                INSERT INTO Stats_UserCount
                                                (
                                                                StatDate,
                                                                Usercount
                                                )
                                                VALUES
                                                (
                                                                @TodaysDate,
                                                                @UserCount
                                                )
 
 
                /* EXCHANGE COUNT */
                IF EXISTS (SELECT StatDate FROM Stats_ExchCount WHERE StatDate = @TodaysDate)
                                UPDATE Stats_ExchCount
                                                SET UserCount=@ExchCount
                                WHERE
                                                StatDate = @TodaysDate
                ELSE
                                INSERT INTO Stats_ExchCount
                                                (
                                                                StatDate,
                                                                UserCount
                                                )
                                                VALUES
                                                (
                                                                @TodaysDate,
                                                                @ExchCount
                                                )
END
' 
GO
