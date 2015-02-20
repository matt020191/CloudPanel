SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ApiKeys]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ApiKeys](
	[UserID] [int] NOT NULL,
	[Key] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.ApiKeys] PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AuditTraces]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[AuditTraces](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TimeStamp] [datetime] NOT NULL,
	[Username] [nvarchar](max) NULL,
	[IPAddress] [nvarchar](max) NULL,
	[Method] [nvarchar](max) NULL,
	[Route] [nvarchar](max) NULL,
	[Parameters] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.AuditTraces] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Brandings]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Brandings](
	[BrandingID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Phone] [nvarchar](max) NOT NULL,
	[HostName] [nvarchar](max) NOT NULL,
	[Email] [nvarchar](max) NOT NULL,
	[LoginLogo] [nvarchar](max) NOT NULL,
	[HeaderLogo] [nvarchar](max) NULL,
	[Theme] [nvarchar](max) NOT NULL,
	[MenuType] [int] NOT NULL,
 CONSTRAINT [PK_dbo.Brandings] PRIMARY KEY CLUSTERED 
(
	[BrandingID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CitrixApplications]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[CitrixApplications](
	[ApplicationID] [int] IDENTITY(1,1) NOT NULL,
	[Uid] [int] NOT NULL,
	[UUID] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[PublishedName] [nvarchar](max) NULL,
	[ApplicationName] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[SecurityGroup] [nvarchar](max) NULL,
	[CommandLineExecutable] [nvarchar](max) NULL,
	[CommandLineArguments] [nvarchar](max) NULL,
	[ShortcutAddedToDesktop] [bit] NOT NULL,
	[ShortcutAddedToStartMenu] [bit] NOT NULL,
	[IsEnabled] [bit] NOT NULL,
	[UserFilterEnabled] [bit] NOT NULL,
	[LastRetrieved] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.CitrixApplications] PRIMARY KEY CLUSTERED 
(
	[ApplicationID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CitrixApplicationsToDesktop]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[CitrixApplicationsToDesktop](
	[ApplicationRefDesktopGroupId] [int] NOT NULL,
	[DesktopGroupRefApplicationId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.CitrixApplicationsToDesktop] PRIMARY KEY CLUSTERED 
(
	[ApplicationRefDesktopGroupId] ASC,
	[DesktopGroupRefApplicationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CitrixCompanyToDesktopGroup]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[CitrixCompanyToDesktopGroup](
	[CompanyRefDesktopGroupId] [int] NOT NULL,
	[DesktopGroupRefCompanyId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.CitrixCompanyToDesktopGroup] PRIMARY KEY CLUSTERED 
(
	[CompanyRefDesktopGroupId] ASC,
	[DesktopGroupRefCompanyId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CitrixDesktopGroups]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[CitrixDesktopGroups](
	[DesktopGroupID] [int] IDENTITY(1,1) NOT NULL,
	[Uid] [int] NOT NULL,
	[UUID] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[PublishedName] [nvarchar](max) NULL,
	[SecurityGroup] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[IsEnabled] [bit] NOT NULL,
	[LastRetrieved] [datetime] NOT NULL,
	[ApplicationId] [int] NOT NULL,
	[DesktopId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.CitrixDesktopGroups] PRIMARY KEY CLUSTERED 
(
	[DesktopGroupID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CitrixDesktops]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[CitrixDesktops](
	[DesktopID] [int] IDENTITY(1,1) NOT NULL,
	[Uid] [int] NOT NULL,
	[DesktopGroupID] [int] NOT NULL,
	[SID] [nvarchar](max) NOT NULL,
	[AgentVersion] [nvarchar](max) NOT NULL,
	[CatalogUid] [int] NOT NULL,
	[CatalogName] [nvarchar](max) NULL,
	[DNSName] [nvarchar](max) NULL,
	[MachineName] [nvarchar](max) NULL,
	[MachineUid] [int] NOT NULL,
	[OSType] [nvarchar](max) NULL,
	[OSVersion] [nvarchar](max) NULL,
	[IPAddress] [nvarchar](max) NULL,
	[InMaintenanceMode] [bit] NOT NULL,
	[LastRetrieved] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.CitrixDesktops] PRIMARY KEY CLUSTERED 
(
	[DesktopID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CitrixSecurityGroups]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[CitrixSecurityGroups](
	[GroupID] [int] IDENTITY(1,1) NOT NULL,
	[GroupName] [nvarchar](max) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[CompanyCode] [nvarchar](max) NOT NULL,
	[DesktopGroupID] [int] NOT NULL,
	[ApplicationID] [int] NULL,
 CONSTRAINT [PK_dbo.CitrixSecurityGroups] PRIMARY KEY CLUSTERED 
(
	[GroupID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CitrixUserToApplications]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[CitrixUserToApplications](
	[UserRefApplicationId] [int] NOT NULL,
	[ApplicationRefUserId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.CitrixUserToApplications] PRIMARY KEY CLUSTERED 
(
	[UserRefApplicationId] ASC,
	[ApplicationRefUserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CitrixUserToDesktopGroup]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[CitrixUserToDesktopGroup](
	[UserRefDesktopGroupId] [int] NOT NULL,
	[DesktopGroupRefUserId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.CitrixUserToDesktopGroup] PRIMARY KEY CLUSTERED 
(
	[UserRefDesktopGroupId] ASC,
	[DesktopGroupRefUserId] ASC
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
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Contacts]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Contacts](
	[DistinguishedName] [varchar](255) NOT NULL,
	[CompanyCode] [varchar](255) NULL,
	[DisplayName] [varchar](255) NOT NULL,
	[Email] [varchar](255) NOT NULL,
	[Hidden] [bit] NOT NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ObjectGuid] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_Contacts] PRIMARY KEY CLUSTERED 
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
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DelayedUserTasks]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[DelayedUserTasks](
	[TaskID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[LastMessage] [ntext] NULL,
	[Created] [datetime] NOT NULL,
	[DelayedUntil] [datetime] NOT NULL,
	[LastUpdated] [datetime] NULL,
 CONSTRAINT [PK_dbo.DelayedUserTasks] PRIMARY KEY CLUSTERED 
(
	[TaskID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
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
	[DistinguishedName] [varchar](255) NULL,
	[CompanyCode] [varchar](255) NULL,
	[DisplayName] [varchar](255) NOT NULL,
	[Email] [varchar](255) NOT NULL,
	[Hidden] [bit] NOT NULL,
	[ObjectGuid] [uniqueidentifier] NOT NULL,
	[IsSecurityGroup] [bit] NOT NULL,
 CONSTRAINT [PK_DistributionGroups] PRIMARY KEY CLUSTERED 
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
	[DomainType] [int] NULL,
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
	[Price] [decimal](18, 2) NOT NULL,
	[Cost] [decimal](18, 2) NOT NULL,
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
	[CompanyCode] [varchar](255) NULL,
	[DisplayName] [varchar](150) NOT NULL,
	[Description] [ntext] NULL,
	[ExchangeName] [varchar](75) NULL,
	[AllowNonProvisionableDevices] [bit] NOT NULL,
	[RefreshIntervalInHours] [int] NULL,
	[RequirePassword] [bit] NOT NULL,
	[RequireAlphanumericPassword] [bit] NOT NULL,
	[EnablePasswordRecovery] [bit] NOT NULL,
	[RequireEncryptionOnDevice] [bit] NOT NULL,
	[RequireEncryptionOnStorageCard] [bit] NOT NULL,
	[AllowSimplePassword] [bit] NOT NULL,
	[NumberOfFailedAttempted] [int] NULL,
	[MinimumPasswordLength] [int] NULL,
	[InactivityTimeoutInMinutes] [int] NULL,
	[PasswordExpirationInDays] [int] NULL,
	[EnforcePasswordHistory] [int] NULL,
	[IncludePastCalendarItems] [varchar](20) NULL,
	[IncludePastEmailItems] [varchar](20) NULL,
	[LimitEmailSizeInKB] [int] NULL,
	[AllowDirectPushWhenRoaming] [bit] NOT NULL,
	[AllowHTMLEmail] [bit] NOT NULL,
	[AllowAttachmentsDownload] [bit] NOT NULL,
	[MaximumAttachmentSizeInKB] [int] NULL,
	[AllowRemovableStorage] [bit] NOT NULL,
	[AllowCamera] [bit] NOT NULL,
	[AllowWiFi] [bit] NOT NULL,
	[AllowInfrared] [bit] NOT NULL,
	[AllowInternetSharing] [bit] NOT NULL,
	[AllowRemoteDesktop] [bit] NOT NULL,
	[AllowDesktopSync] [bit] NOT NULL,
	[AllowBluetooth] [varchar](10) NULL,
	[AllowBrowser] [bit] NOT NULL,
	[AllowConsumerMail] [bit] NOT NULL,
	[IsEnterpriseCAL] [bit] NULL,
	[AllowTextMessaging] [bit] NOT NULL,
	[AllowUnsignedApplications] [bit] NOT NULL,
	[AllowUnsignedInstallationPackages] [bit] NOT NULL,
	[MinDevicePasswordComplexCharacters] [int] NULL,
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
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Plans_ExchangeArchiving]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Plans_ExchangeArchiving](
	[ArchivingID] [int] IDENTITY(1,1) NOT NULL,
	[DisplayName] [nvarchar](50) NOT NULL,
	[Database] [nvarchar](max) NULL,
	[ResellerCode] [nvarchar](255) NULL,
	[CompanyCode] [nvarchar](255) NULL,
	[Description] [ntext] NOT NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[Cost] [decimal](18, 2) NOT NULL,
	[ArchiveSizeMB] [int] NOT NULL,
 CONSTRAINT [PK_dbo.Plans_ExchangeArchiving] PRIMARY KEY CLUSTERED 
(
	[ArchivingID] ASC
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
	[Price] [decimal](18, 2) NOT NULL,
	[Cost] [decimal](18, 2) NOT NULL,
	[AdditionalGBPrice] [decimal](18, 2) NOT NULL,
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
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Plans_ExchangePublicFolders]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Plans_ExchangePublicFolders](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[MailboxSizeMB] [int] NOT NULL,
	[CompanyCode] [nvarchar](max) NULL,
	[Description] [ntext] NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[Cost] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_dbo.Plans_ExchangePublicFolders] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
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
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PriceOverride]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[PriceOverride](
	[CompanyCode] [nvarchar](255) NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[PlanID] [int] NULL,
	[Product] [varchar](25) NULL,
	[ID] [int] IDENTITY(1,1) NOT NULL,
 CONSTRAINT [PK_PriceOverride] PRIMARY KEY CLUSTERED 
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
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PublicFolderMailboxes]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[PublicFolderMailboxes](
	[MailboxID] [int] IDENTITY(1,1) NOT NULL,
	[CompanyID] [int] NOT NULL,
	[PlanID] [int] NOT NULL,
	[Identity] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.PublicFolderMailboxes] PRIMARY KEY CLUSTERED 
(
	[MailboxID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
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
	[DistinguishedName] [nvarchar](max) NULL,
	[ResourceGuid] [uniqueidentifier] NULL,
 CONSTRAINT [PK_ResourceMailboxes] PRIMARY KEY CLUSTERED 
(
	[ResourceID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Statistics]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Statistics](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Retrieved] [datetime] NOT NULL,
	[UserCount] [int] NOT NULL,
	[MailboxCount] [int] NOT NULL,
	[CitrixCount] [int] NOT NULL,
	[ResellerCode] [nvarchar](max) NULL,
	[CompanyCode] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.Statistics] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StatMailboxArchiveSizes]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[StatMailboxArchiveSizes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserGuid] [uniqueidentifier] NOT NULL,
	[UserPrincipalName] [nvarchar](64) NOT NULL,
	[MailboxDatabase] [nvarchar](255) NOT NULL,
	[TotalItemSize] [nvarchar](255) NOT NULL,
	[TotalItemSizeInBytes] [bigint] NOT NULL,
	[TotalDeletedItemSize] [nvarchar](255) NOT NULL,
	[TotalDeletedItemSizeInBytes] [bigint] NOT NULL,
	[ItemCount] [int] NOT NULL,
	[DeletedItemCount] [int] NOT NULL,
	[Retrieved] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.StatMailboxArchiveSizes] PRIMARY KEY CLUSTERED 
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
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StatMailboxDatabaseSizes]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[StatMailboxDatabaseSizes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[DatabaseName] [nvarchar](64) NOT NULL,
	[Server] [nvarchar](64) NOT NULL,
	[DatabaseSize] [nvarchar](255) NOT NULL,
	[Retrieved] [datetime] NOT NULL,
	[DatabaseSizeInBytes] [bigint] NOT NULL,
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
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StatMailboxSizes]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[StatMailboxSizes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserGuid] [uniqueidentifier] NOT NULL,
	[UserPrincipalName] [nvarchar](64) NOT NULL,
	[MailboxDatabase] [nvarchar](255) NOT NULL,
	[TotalItemSize] [nvarchar](255) NOT NULL,
	[TotalItemSizeInBytes] [bigint] NOT NULL,
	[TotalDeletedItemSize] [nvarchar](255) NOT NULL,
	[TotalDeletedItemSizeInBytes] [bigint] NOT NULL,
	[ItemCount] [int] NOT NULL,
	[DeletedItemCount] [int] NOT NULL,
	[Retrieved] [datetime] NOT NULL,
 CONSTRAINT [PK_dbo.StatMailboxSizes] PRIMARY KEY CLUSTERED 
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
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserActiveSyncDevices]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[UserActiveSyncDevices](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[DeviceGuid] [uniqueidentifier] NOT NULL,
	[FirstSyncTime] [datetime] NULL,
	[LastPolicyUpdateTime] [datetime] NULL,
	[LastSyncAttemptTime] [datetime] NULL,
	[LastSuccessSync] [datetime] NULL,
	[DeviceWipeSentTime] [datetime] NULL,
	[DeviceWipeRequestTime] [datetime] NULL,
	[DeviceWipeAckTime] [datetime] NULL,
	[LastPingHeartbeat] [int] NOT NULL,
	[Identity] [nvarchar](max) NULL,
	[DeviceType] [nvarchar](max) NULL,
	[DeviceID] [nvarchar](max) NULL,
	[DeviceUserAgent] [nvarchar](max) NULL,
	[DeviceModel] [nvarchar](max) NULL,
	[DeviceImei] [nvarchar](max) NULL,
	[DeviceFriendlyName] [nvarchar](max) NULL,
	[DeviceOS] [nvarchar](max) NULL,
	[DevicePhoneNumber] [nvarchar](max) NULL,
	[Status] [nvarchar](max) NULL,
	[StatusNote] [nvarchar](max) NULL,
	[DevicePolicyApplied] [nvarchar](max) NULL,
	[DevicePolicyApplicationStatus] [nvarchar](max) NULL,
	[DeviceActiveSyncVersion] [nvarchar](max) NULL,
	[NumberOfFoldersSynced] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.UserActiveSyncDevices] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserPermission]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[UserPermission](
	[UserID] [int] NOT NULL,
	[RoleID] [int] NOT NULL,
 CONSTRAINT [PK_dbo.UserPermission] PRIMARY KEY CLUSTERED 
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
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserRoles]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[UserRoles](
	[RoleID] [int] IDENTITY(1,1) NOT NULL,
	[CompanyCode] [nvarchar](max) NULL,
	[DisplayName] [nvarchar](max) NULL,
	[cDomains] [bit] NOT NULL,
	[cUsers] [bit] NOT NULL,
	[cExchangeActiveSyncPlans] [bit] NOT NULL,
	[cExchangeContacts] [bit] NOT NULL,
	[cExchangeGroups] [bit] NOT NULL,
	[cExchangeResources] [bit] NOT NULL,
	[cExchangePublicFolders] [bit] NOT NULL,
	[cCitrix] [bit] NOT NULL,
	[cLync] [bit] NOT NULL,
	[vDomains] [bit] NOT NULL,
	[vUsers] [bit] NOT NULL,
	[vUsersEdit] [bit] NOT NULL,
	[vExchangeActiveSyncPlans] [bit] NOT NULL,
	[vExchangeContacts] [bit] NOT NULL,
	[vExchangeGroups] [bit] NOT NULL,
	[vExchangeResources] [bit] NOT NULL,
	[vExchangePublicFolders] [bit] NOT NULL,
	[vCitrix] [bit] NOT NULL,
	[vLync] [bit] NOT NULL,
	[eDomains] [bit] NOT NULL,
	[eUsers] [bit] NOT NULL,
	[eExchangeActiveSyncPlans] [bit] NOT NULL,
	[eExchangeContacts] [bit] NOT NULL,
	[eExchangeGroups] [bit] NOT NULL,
	[eExchangeResources] [bit] NOT NULL,
	[eExchangePublicFolders] [bit] NOT NULL,
	[eCitrix] [bit] NOT NULL,
	[eLync] [bit] NOT NULL,
	[ePermissions] [bit] NOT NULL,
	[dDomains] [bit] NOT NULL,
	[dUsers] [bit] NOT NULL,
	[dExchangeActiveSyncPlans] [bit] NOT NULL,
	[dExchangeContacts] [bit] NOT NULL,
	[dExchangeGroups] [bit] NOT NULL,
	[dExchangeResources] [bit] NOT NULL,
	[dExchangePublicFolders] [bit] NOT NULL,
	[dCitrix] [bit] NOT NULL,
	[dLync] [bit] NOT NULL,
	[dPermissions] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.UserRoles] PRIMARY KEY CLUSTERED 
(
	[RoleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
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
	[IsEnabled] [bit] NULL,
	[Street] [nvarchar](max) NULL,
	[City] [nvarchar](max) NULL,
	[State] [nvarchar](max) NULL,
	[PostalCode] [nvarchar](max) NULL,
	[Country] [nvarchar](max) NULL,
	[Company] [nvarchar](max) NULL,
	[JobTitle] [nvarchar](max) NULL,
	[TelephoneNumber] [nvarchar](max) NULL,
	[Fax] [nvarchar](max) NULL,
	[HomePhone] [nvarchar](max) NULL,
	[MobilePhone] [nvarchar](max) NULL,
	[Notes] [nvarchar](max) NULL,
	[RoleID] [int] NULL,
	[ArchivePlan] [int] NULL,
	[Skype] [nvarchar](max) NULL,
	[Facebook] [nvarchar](max) NULL,
	[Twitter] [nvarchar](max) NULL,
	[Dribbble] [nvarchar](max) NULL,
	[Tumblr] [nvarchar](max) NULL,
	[LinkedIn] [nvarchar](max) NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ApiKeys]') AND name = N'IX_UserID')
CREATE NONCLUSTERED INDEX [IX_UserID] ON [dbo].[ApiKeys]
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CitrixApplicationsToDesktop]') AND name = N'IX_ApplicationRefDesktopGroupId')
CREATE NONCLUSTERED INDEX [IX_ApplicationRefDesktopGroupId] ON [dbo].[CitrixApplicationsToDesktop]
(
	[ApplicationRefDesktopGroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CitrixApplicationsToDesktop]') AND name = N'IX_DesktopGroupRefApplicationId')
CREATE NONCLUSTERED INDEX [IX_DesktopGroupRefApplicationId] ON [dbo].[CitrixApplicationsToDesktop]
(
	[DesktopGroupRefApplicationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CitrixCompanyToDesktopGroup]') AND name = N'IX_CompanyRefDesktopGroupId')
CREATE NONCLUSTERED INDEX [IX_CompanyRefDesktopGroupId] ON [dbo].[CitrixCompanyToDesktopGroup]
(
	[CompanyRefDesktopGroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CitrixCompanyToDesktopGroup]') AND name = N'IX_DesktopGroupRefCompanyId')
CREATE NONCLUSTERED INDEX [IX_DesktopGroupRefCompanyId] ON [dbo].[CitrixCompanyToDesktopGroup]
(
	[DesktopGroupRefCompanyId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CitrixDesktops]') AND name = N'IX_DesktopGroupID')
CREATE NONCLUSTERED INDEX [IX_DesktopGroupID] ON [dbo].[CitrixDesktops]
(
	[DesktopGroupID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CitrixSecurityGroups]') AND name = N'IX_DesktopGroupID')
CREATE NONCLUSTERED INDEX [IX_DesktopGroupID] ON [dbo].[CitrixSecurityGroups]
(
	[DesktopGroupID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CitrixUserToApplications]') AND name = N'IX_ApplicationRefUserId')
CREATE NONCLUSTERED INDEX [IX_ApplicationRefUserId] ON [dbo].[CitrixUserToApplications]
(
	[ApplicationRefUserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CitrixUserToApplications]') AND name = N'IX_UserRefApplicationId')
CREATE NONCLUSTERED INDEX [IX_UserRefApplicationId] ON [dbo].[CitrixUserToApplications]
(
	[UserRefApplicationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CitrixUserToDesktopGroup]') AND name = N'IX_DesktopGroupRefUserId')
CREATE NONCLUSTERED INDEX [IX_DesktopGroupRefUserId] ON [dbo].[CitrixUserToDesktopGroup]
(
	[DesktopGroupRefUserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CitrixUserToDesktopGroup]') AND name = N'IX_UserRefDesktopGroupId')
CREATE NONCLUSTERED INDEX [IX_UserRefDesktopGroupId] ON [dbo].[CitrixUserToDesktopGroup]
(
	[UserRefDesktopGroupId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[DelayedUserTasks]') AND name = N'IX_UserID')
CREATE NONCLUSTERED INDEX [IX_UserID] ON [dbo].[DelayedUserTasks]
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PublicFolderMailboxes]') AND name = N'IX_CompanyID')
CREATE NONCLUSTERED INDEX [IX_CompanyID] ON [dbo].[PublicFolderMailboxes]
(
	[CompanyID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[PublicFolderMailboxes]') AND name = N'IX_PlanID')
CREATE NONCLUSTERED INDEX [IX_PlanID] ON [dbo].[PublicFolderMailboxes]
(
	[PlanID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[UserActiveSyncDevices]') AND name = N'IX_UserID')
CREATE NONCLUSTERED INDEX [IX_UserID] ON [dbo].[UserActiveSyncDevices]
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = N'IX_RoleID')
CREATE NONCLUSTERED INDEX [IX_RoleID] ON [dbo].[Users]
(
	[RoleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[DF__Contacts__Object__05D8E0BE]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Contacts] ADD  DEFAULT ('00000000-0000-0000-0000-000000000000') FOR [ObjectGuid]
END

GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[DF__Distribut__Objec__06CD04F7]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[DistributionGroups] ADD  DEFAULT ('00000000-0000-0000-0000-000000000000') FOR [ObjectGuid]
END

GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[DF__Distribut__IsSec__07C12930]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[DistributionGroups] ADD  DEFAULT ((0)) FOR [IsSecurityGroup]
END

GO
IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[DF__StatMailb__Datab__797309D9]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[StatMailboxDatabaseSizes] ADD  DEFAULT ((0)) FOR [DatabaseSizeInBytes]
END

GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.ApiKeys_dbo.Users_UserID]') AND parent_object_id = OBJECT_ID(N'[dbo].[ApiKeys]'))
ALTER TABLE [dbo].[ApiKeys]  WITH CHECK ADD  CONSTRAINT [FK_dbo.ApiKeys_dbo.Users_UserID] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([ID])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.ApiKeys_dbo.Users_UserID]') AND parent_object_id = OBJECT_ID(N'[dbo].[ApiKeys]'))
ALTER TABLE [dbo].[ApiKeys] CHECK CONSTRAINT [FK_dbo.ApiKeys_dbo.Users_UserID]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.CitrixApplicationsToDesktop_dbo.CitrixApplications_ApplicationRefDesktopGroupId]') AND parent_object_id = OBJECT_ID(N'[dbo].[CitrixApplicationsToDesktop]'))
ALTER TABLE [dbo].[CitrixApplicationsToDesktop]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CitrixApplicationsToDesktop_dbo.CitrixApplications_ApplicationRefDesktopGroupId] FOREIGN KEY([ApplicationRefDesktopGroupId])
REFERENCES [dbo].[CitrixApplications] ([ApplicationID])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.CitrixApplicationsToDesktop_dbo.CitrixApplications_ApplicationRefDesktopGroupId]') AND parent_object_id = OBJECT_ID(N'[dbo].[CitrixApplicationsToDesktop]'))
ALTER TABLE [dbo].[CitrixApplicationsToDesktop] CHECK CONSTRAINT [FK_dbo.CitrixApplicationsToDesktop_dbo.CitrixApplications_ApplicationRefDesktopGroupId]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.CitrixApplicationsToDesktop_dbo.CitrixDesktopGroups_DesktopGroupRefApplicationId]') AND parent_object_id = OBJECT_ID(N'[dbo].[CitrixApplicationsToDesktop]'))
ALTER TABLE [dbo].[CitrixApplicationsToDesktop]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CitrixApplicationsToDesktop_dbo.CitrixDesktopGroups_DesktopGroupRefApplicationId] FOREIGN KEY([DesktopGroupRefApplicationId])
REFERENCES [dbo].[CitrixDesktopGroups] ([DesktopGroupID])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.CitrixApplicationsToDesktop_dbo.CitrixDesktopGroups_DesktopGroupRefApplicationId]') AND parent_object_id = OBJECT_ID(N'[dbo].[CitrixApplicationsToDesktop]'))
ALTER TABLE [dbo].[CitrixApplicationsToDesktop] CHECK CONSTRAINT [FK_dbo.CitrixApplicationsToDesktop_dbo.CitrixDesktopGroups_DesktopGroupRefApplicationId]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.CitrixCompanyToDesktopGroup_dbo.CitrixDesktopGroups_DesktopGroupRefCompanyId]') AND parent_object_id = OBJECT_ID(N'[dbo].[CitrixCompanyToDesktopGroup]'))
ALTER TABLE [dbo].[CitrixCompanyToDesktopGroup]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CitrixCompanyToDesktopGroup_dbo.CitrixDesktopGroups_DesktopGroupRefCompanyId] FOREIGN KEY([DesktopGroupRefCompanyId])
REFERENCES [dbo].[CitrixDesktopGroups] ([DesktopGroupID])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.CitrixCompanyToDesktopGroup_dbo.CitrixDesktopGroups_DesktopGroupRefCompanyId]') AND parent_object_id = OBJECT_ID(N'[dbo].[CitrixCompanyToDesktopGroup]'))
ALTER TABLE [dbo].[CitrixCompanyToDesktopGroup] CHECK CONSTRAINT [FK_dbo.CitrixCompanyToDesktopGroup_dbo.CitrixDesktopGroups_DesktopGroupRefCompanyId]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.CitrixCompanyToDesktopGroup_dbo.Companies_CompanyRefDesktopGroupId]') AND parent_object_id = OBJECT_ID(N'[dbo].[CitrixCompanyToDesktopGroup]'))
ALTER TABLE [dbo].[CitrixCompanyToDesktopGroup]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CitrixCompanyToDesktopGroup_dbo.Companies_CompanyRefDesktopGroupId] FOREIGN KEY([CompanyRefDesktopGroupId])
REFERENCES [dbo].[Companies] ([CompanyId])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.CitrixCompanyToDesktopGroup_dbo.Companies_CompanyRefDesktopGroupId]') AND parent_object_id = OBJECT_ID(N'[dbo].[CitrixCompanyToDesktopGroup]'))
ALTER TABLE [dbo].[CitrixCompanyToDesktopGroup] CHECK CONSTRAINT [FK_dbo.CitrixCompanyToDesktopGroup_dbo.Companies_CompanyRefDesktopGroupId]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.CitrixDesktops_dbo.CitrixDesktopGroups_DesktopGroupID]') AND parent_object_id = OBJECT_ID(N'[dbo].[CitrixDesktops]'))
ALTER TABLE [dbo].[CitrixDesktops]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CitrixDesktops_dbo.CitrixDesktopGroups_DesktopGroupID] FOREIGN KEY([DesktopGroupID])
REFERENCES [dbo].[CitrixDesktopGroups] ([DesktopGroupID])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.CitrixDesktops_dbo.CitrixDesktopGroups_DesktopGroupID]') AND parent_object_id = OBJECT_ID(N'[dbo].[CitrixDesktops]'))
ALTER TABLE [dbo].[CitrixDesktops] CHECK CONSTRAINT [FK_dbo.CitrixDesktops_dbo.CitrixDesktopGroups_DesktopGroupID]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.CitrixSecurityGroups_dbo.CitrixDesktopGroups_DesktopGroupID]') AND parent_object_id = OBJECT_ID(N'[dbo].[CitrixSecurityGroups]'))
ALTER TABLE [dbo].[CitrixSecurityGroups]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CitrixSecurityGroups_dbo.CitrixDesktopGroups_DesktopGroupID] FOREIGN KEY([DesktopGroupID])
REFERENCES [dbo].[CitrixDesktopGroups] ([DesktopGroupID])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.CitrixSecurityGroups_dbo.CitrixDesktopGroups_DesktopGroupID]') AND parent_object_id = OBJECT_ID(N'[dbo].[CitrixSecurityGroups]'))
ALTER TABLE [dbo].[CitrixSecurityGroups] CHECK CONSTRAINT [FK_dbo.CitrixSecurityGroups_dbo.CitrixDesktopGroups_DesktopGroupID]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.CitrixUserToApplications_dbo.CitrixApplications_ApplicationRefUserId]') AND parent_object_id = OBJECT_ID(N'[dbo].[CitrixUserToApplications]'))
ALTER TABLE [dbo].[CitrixUserToApplications]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CitrixUserToApplications_dbo.CitrixApplications_ApplicationRefUserId] FOREIGN KEY([ApplicationRefUserId])
REFERENCES [dbo].[CitrixApplications] ([ApplicationID])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.CitrixUserToApplications_dbo.CitrixApplications_ApplicationRefUserId]') AND parent_object_id = OBJECT_ID(N'[dbo].[CitrixUserToApplications]'))
ALTER TABLE [dbo].[CitrixUserToApplications] CHECK CONSTRAINT [FK_dbo.CitrixUserToApplications_dbo.CitrixApplications_ApplicationRefUserId]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.CitrixUserToApplications_dbo.Users_UserRefApplicationId]') AND parent_object_id = OBJECT_ID(N'[dbo].[CitrixUserToApplications]'))
ALTER TABLE [dbo].[CitrixUserToApplications]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CitrixUserToApplications_dbo.Users_UserRefApplicationId] FOREIGN KEY([UserRefApplicationId])
REFERENCES [dbo].[Users] ([ID])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.CitrixUserToApplications_dbo.Users_UserRefApplicationId]') AND parent_object_id = OBJECT_ID(N'[dbo].[CitrixUserToApplications]'))
ALTER TABLE [dbo].[CitrixUserToApplications] CHECK CONSTRAINT [FK_dbo.CitrixUserToApplications_dbo.Users_UserRefApplicationId]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.CitrixUserToDesktopGroup_dbo.CitrixDesktopGroups_DesktopGroupRefUserId]') AND parent_object_id = OBJECT_ID(N'[dbo].[CitrixUserToDesktopGroup]'))
ALTER TABLE [dbo].[CitrixUserToDesktopGroup]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CitrixUserToDesktopGroup_dbo.CitrixDesktopGroups_DesktopGroupRefUserId] FOREIGN KEY([DesktopGroupRefUserId])
REFERENCES [dbo].[CitrixDesktopGroups] ([DesktopGroupID])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.CitrixUserToDesktopGroup_dbo.CitrixDesktopGroups_DesktopGroupRefUserId]') AND parent_object_id = OBJECT_ID(N'[dbo].[CitrixUserToDesktopGroup]'))
ALTER TABLE [dbo].[CitrixUserToDesktopGroup] CHECK CONSTRAINT [FK_dbo.CitrixUserToDesktopGroup_dbo.CitrixDesktopGroups_DesktopGroupRefUserId]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.CitrixUserToDesktopGroup_dbo.Users_UserRefDesktopGroupId]') AND parent_object_id = OBJECT_ID(N'[dbo].[CitrixUserToDesktopGroup]'))
ALTER TABLE [dbo].[CitrixUserToDesktopGroup]  WITH CHECK ADD  CONSTRAINT [FK_dbo.CitrixUserToDesktopGroup_dbo.Users_UserRefDesktopGroupId] FOREIGN KEY([UserRefDesktopGroupId])
REFERENCES [dbo].[Users] ([ID])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.CitrixUserToDesktopGroup_dbo.Users_UserRefDesktopGroupId]') AND parent_object_id = OBJECT_ID(N'[dbo].[CitrixUserToDesktopGroup]'))
ALTER TABLE [dbo].[CitrixUserToDesktopGroup] CHECK CONSTRAINT [FK_dbo.CitrixUserToDesktopGroup_dbo.Users_UserRefDesktopGroupId]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.DelayedUserTasks_dbo.Users_UserID]') AND parent_object_id = OBJECT_ID(N'[dbo].[DelayedUserTasks]'))
ALTER TABLE [dbo].[DelayedUserTasks]  WITH CHECK ADD  CONSTRAINT [FK_dbo.DelayedUserTasks_dbo.Users_UserID] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([ID])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.DelayedUserTasks_dbo.Users_UserID]') AND parent_object_id = OBJECT_ID(N'[dbo].[DelayedUserTasks]'))
ALTER TABLE [dbo].[DelayedUserTasks] CHECK CONSTRAINT [FK_dbo.DelayedUserTasks_dbo.Users_UserID]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.PublicFolderMailboxes_dbo.Companies_CompanyID]') AND parent_object_id = OBJECT_ID(N'[dbo].[PublicFolderMailboxes]'))
ALTER TABLE [dbo].[PublicFolderMailboxes]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PublicFolderMailboxes_dbo.Companies_CompanyID] FOREIGN KEY([CompanyID])
REFERENCES [dbo].[Companies] ([CompanyId])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.PublicFolderMailboxes_dbo.Companies_CompanyID]') AND parent_object_id = OBJECT_ID(N'[dbo].[PublicFolderMailboxes]'))
ALTER TABLE [dbo].[PublicFolderMailboxes] CHECK CONSTRAINT [FK_dbo.PublicFolderMailboxes_dbo.Companies_CompanyID]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.PublicFolderMailboxes_dbo.Plans_ExchangePublicFolders_PlanID]') AND parent_object_id = OBJECT_ID(N'[dbo].[PublicFolderMailboxes]'))
ALTER TABLE [dbo].[PublicFolderMailboxes]  WITH CHECK ADD  CONSTRAINT [FK_dbo.PublicFolderMailboxes_dbo.Plans_ExchangePublicFolders_PlanID] FOREIGN KEY([PlanID])
REFERENCES [dbo].[Plans_ExchangePublicFolders] ([ID])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.PublicFolderMailboxes_dbo.Plans_ExchangePublicFolders_PlanID]') AND parent_object_id = OBJECT_ID(N'[dbo].[PublicFolderMailboxes]'))
ALTER TABLE [dbo].[PublicFolderMailboxes] CHECK CONSTRAINT [FK_dbo.PublicFolderMailboxes_dbo.Plans_ExchangePublicFolders_PlanID]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.UserActiveSyncDevices_dbo.Users_UserID]') AND parent_object_id = OBJECT_ID(N'[dbo].[UserActiveSyncDevices]'))
ALTER TABLE [dbo].[UserActiveSyncDevices]  WITH CHECK ADD  CONSTRAINT [FK_dbo.UserActiveSyncDevices_dbo.Users_UserID] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([ID])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.UserActiveSyncDevices_dbo.Users_UserID]') AND parent_object_id = OBJECT_ID(N'[dbo].[UserActiveSyncDevices]'))
ALTER TABLE [dbo].[UserActiveSyncDevices] CHECK CONSTRAINT [FK_dbo.UserActiveSyncDevices_dbo.Users_UserID]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.Users_dbo.UserRoles_RoleID]') AND parent_object_id = OBJECT_ID(N'[dbo].[Users]'))
ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [FK_dbo.Users_dbo.UserRoles_RoleID] FOREIGN KEY([RoleID])
REFERENCES [dbo].[UserRoles] ([RoleID])
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_dbo.Users_dbo.UserRoles_RoleID]') AND parent_object_id = OBJECT_ID(N'[dbo].[Users]'))
ALTER TABLE [dbo].[Users] CHECK CONSTRAINT [FK_dbo.Users_dbo.UserRoles_RoleID]
GO
