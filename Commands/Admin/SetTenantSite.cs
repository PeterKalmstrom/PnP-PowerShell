﻿#if !ONPREMISES
using System;
using System.Management.Automation;
using Microsoft.Online.SharePoint.TenantManagement;
using Microsoft.SharePoint.Client;
using SharePointPnP.PowerShell.CmdletHelpAttributes;
using SharePointPnP.PowerShell.Commands.Base;
using System.Collections.Generic;
using OfficeDevPnP.Core;
using OfficeDevPnP.Core.Entities;
using Microsoft.Online.SharePoint.TenantAdministration;

namespace SharePointPnP.PowerShell.Commands
{
    [Cmdlet(VerbsCommon.Set, "PnPTenantSite")]
    [CmdletHelp(@"Set site information.",
        "Sets site properties for existing sites.",
        SupportedPlatform = CmdletSupportedPlatform.Online,
        Category = CmdletHelpCategory.TenantAdmin)]
    [CmdletExample(
      Code = @"PS:> Set-PnPTenantSite -Url https://contoso.sharepoint.com -Title ""Contoso Website"" -Sharing Disabled",
      Remarks = @"This will set the title of the site collection with the URL 'https://contoso.sharepoint.com' to 'Contoso Website' and disable sharing on this site collection.", SortOrder = 1)]
    [CmdletExample(
      Code = @"PS:> Set-PnPTenantSite -Url https://contoso.sharepoint.com -Title ""Contoso Website"" -StorageWarningLevel 8000 -StorageMaximumLevel 10000",
      Remarks = @"This will set the title of the site collection with the URL 'https://contoso.sharepoint.com' to 'Contoso Website', set the storage warning level to 8GB and set the storage maximum level to 10GB.", SortOrder = 2)]
    [CmdletExample(
      Code = @"PS:> Set-PnPTenantSite -Url https://contoso.sharepoint.com/sites/sales -Owners ""user@contoso.onmicrosoft.com""",
      Remarks = @"This will add user@contoso.onmicrosoft.com as an additional site collection owner at 'https://contoso.sharepoint.com/sites/sales'.", SortOrder = 3)]
    [CmdletExample(
      Code = @"PS:> Set-PnPTenantSite -Url https://contoso.sharepoint.com/sites/sales -Owners @(""user1@contoso.onmicrosoft.com"", ""user2@contoso.onmicrosoft.com"")",
      Remarks = @"This will add user1@contoso.onmicrosoft.com and user2@contoso.onmicrosoft.com as additional site collection owners at 'https://contoso.sharepoint.com/sites/sales'.", SortOrder = 4)]
    [CmdletExample(
      Code = @"PS:> Set-PnPTenantSite -Url https://contoso.sharepoint.com/sites/sales -NoScriptSite:$false",
      Remarks = @"This will enable script support for the site 'https://contoso.sharepoint.com/sites/sales' if disabled.", SortOrder = 5)]
    public class SetTenantSite : PnPAdminCmdlet
    {
        private const string ParameterSet_LOCKSTATE = "Set Lock State";
        private const string ParameterSet_PROPERTIES = "Set Properties";

        [Parameter(Mandatory = true, HelpMessage = "Specifies the URL of the site", Position = 0, ValueFromPipeline = true)]
        public string Url;

        [Parameter(Mandatory = false, HelpMessage = "Specifies the title of the site", ParameterSetName = ParameterSet_PROPERTIES)]
        public string Title;

        [Parameter(Mandatory = false, HelpMessage = "Specifies what the sharing capabilities are for the site. Possible values: Disabled, ExternalUserSharingOnly, ExternalUserAndGuestSharing, ExistingExternalUserSharingOnly", ParameterSetName = ParameterSet_PROPERTIES)]
        [Alias("Sharing")]
        public SharingCapabilities? SharingCapability;

        [Parameter(Mandatory = false, HelpMessage = "Determines whether the Add And Customize Pages right is denied on the site collection. For more information about permission levels, see User permissions and permission levels in SharePoint.", ParameterSetName = ParameterSet_PROPERTIES)]
        public SwitchParameter? DenyAddAndCustomizePages;

        [Parameter(Mandatory = false, HelpMessage = "Specifies the language of this site collection. For more information, see Locale IDs Assigned by Microsoft (https://go.microsoft.com/fwlink/p/?LinkId=242911).", ParameterSetName = ParameterSet_PROPERTIES)]
        public UInt32 LocaleId;

        [Parameter(Mandatory = false, HelpMessage = "Specifies the storage quota for this site collection in megabytes. This value must not exceed the company's available quota.", ParameterSetName = ParameterSet_PROPERTIES)]
        public long? StorageMaximumLevel = null;

        [Parameter(Mandatory = false, HelpMessage = "Specifies the warning level for the storage quota in megabytes. This value must not exceed the values set for the StorageMaximumLevel parameter", ParameterSetName = ParameterSet_PROPERTIES)]
        public long? StorageWarningLevel = null;

        [Parameter(Mandatory = false, HelpMessage = "Specifies the quota for this site collection in Sandboxed Solutions units. This value must not exceed the company's aggregate available Sandboxed Solutions quota. The default value is 0. For more information, see Resource Usage Limits on Sandboxed Solutions in SharePoint 2010 : http://msdn.microsoft.com/en-us/library/gg615462.aspx.", ParameterSetName = ParameterSet_PROPERTIES)]
        public double? UserCodeMaximumLevel = null;

        [Parameter(Mandatory = false, HelpMessage = "Specifies the warning level for the resource quota. This value must not exceed the value set for the UserCodeMaximumLevel parameter", ParameterSetName = ParameterSet_PROPERTIES)]
        public double? UserCodeWarningLevel = null;

        [Parameter(Mandatory = false, HelpMessage = "Specifies if the site administrator can upgrade the site collection", ParameterSetName = ParameterSet_PROPERTIES)]
        public SwitchParameter? AllowSelfServiceUpgrade;

        [Parameter(Mandatory = false, HelpMessage = "Specifies owner(s) to add as site collection administrators. They will be added as additional site collection administrators. Existing administrators will stay. Can be both users and groups.", ParameterSetName = ParameterSet_PROPERTIES)]
        public List<string> Owners;

        [Parameter(Mandatory = false, HelpMessage = "Sets the lockstate of a site", ParameterSetName = ParameterSet_LOCKSTATE)]
        public SiteLockState? LockState;

        [Parameter(Mandatory = false, HelpMessage = "Specifies if a site allows custom script or not. See https://support.office.com/en-us/article/Turn-scripting-capabilities-on-or-off-1f2c515f-5d7e-448a-9fd7-835da935584f for more information.", ParameterSetName = ParameterSet_PROPERTIES)]
        public SwitchParameter? NoScriptSite;

        [Parameter(Mandatory = false, HelpMessage = @"Specifies the default link permission for the site collection. None - Respect the organization default link permission. View - Sets the default link permission for the site to ""view"" permissions. Edit - Sets the default link permission for the site to ""edit"" permissions", ParameterSetName = ParameterSet_PROPERTIES)]
        public SharingPermissionType? DefaultLinkPermission;

        [Parameter(Mandatory = false, HelpMessage = @"Specifies the default link type for the site collection. None - Respect the organization default sharing link type. AnonymousAccess - Sets the default sharing link for this site to an Anonymous Access or Anyone link. Internal - Sets the default sharing link for this site to the ""organization"" link or company shareable link. Direct - Sets the default sharing link for this site to the ""Specific people"" link", ParameterSetName = ParameterSet_PROPERTIES)]
        public SharingLinkType? DefaultSharingLinkType;

        [Parameter(Mandatory = false, HelpMessage = @"Specifies a list of email domains that is allowed for sharing with the external collaborators. Use the space character as the delimiter for entering multiple values. For example, ""contoso.com fabrikam.com"".", ParameterSetName = ParameterSet_PROPERTIES)]
        public string SharingAllowedDomainList;

        [Parameter(Mandatory = false, HelpMessage = @"Specifies a list of email domains that is blocked for sharing with the external collaborators. Use the space character as the delimiter for entering multiple values. For example, ""contoso.com fabrikam.com"".", ParameterSetName = ParameterSet_PROPERTIES)]
        public string SharingBlockedDomainList;

        [Parameter(Mandatory = false, HelpMessage = @"Specifies the external sharing mode for domains.")]
        public SharingDomainRestrictionModes SharingDomainRestrictionMode = SharingDomainRestrictionModes.None;

        [Parameter(Mandatory = false, HelpMessage = "Wait for the operation to complete")]
        public SwitchParameter Wait;
        protected override void ExecuteCmdlet()
        {
            Func<TenantOperationMessage, bool> timeoutFunction = TimeoutFunction;

            if (LockState.HasValue)
            {
                Tenant.SetSiteLockState(Url, LockState.Value, Wait, Wait ? timeoutFunction : null);
                WriteWarning("You changed the lockstate of a site. This change is not guaranteed to be effective immediately. Please wait a few minutes for this to take effect.");
            }

            if (!LockState.HasValue)
            {
                Tenant.SetSiteProperties(Url, title: Title,
                    sharingCapability: SharingCapability,
                    storageMaximumLevel: StorageMaximumLevel,
                    storageWarningLevel: StorageWarningLevel,
                    allowSelfServiceUpgrade: AllowSelfServiceUpgrade,
                    userCodeMaximumLevel: UserCodeMaximumLevel,
                    userCodeWarningLevel: UserCodeWarningLevel,
                    noScriptSite: NoScriptSite,
                    defaultLinkPermission: DefaultLinkPermission,
                    defaultSharingLinkType: DefaultSharingLinkType,
                    wait: Wait, timeoutFunction: Wait ? timeoutFunction : null
                    );

                SiteProperties props = null;
                var updateRequired = false;
                if (ParameterSpecified(nameof(SharingAllowedDomainList)))
                {
                    if (props == null)
                    {
                        props = GetSiteProperties(Url);
                    }
                    props.SharingAllowedDomainList = SharingAllowedDomainList;
                    updateRequired = true;
                }
                if (ParameterSpecified(nameof(SharingBlockedDomainList)))
                {
                    if (props == null)
                    {
                        props = GetSiteProperties(Url);
                    }
                    props.SharingBlockedDomainList = SharingBlockedDomainList;
                    updateRequired = true;
                }
                if (ParameterSpecified(nameof(SharingDomainRestrictionMode)))
                {
                    if (props == null)
                    {
                        props = GetSiteProperties(Url);
                    }
                    props.SharingDomainRestrictionMode = SharingDomainRestrictionMode;
                    updateRequired = true;
                }
                if (updateRequired)
                {
                    props.Update();
                    ClientContext.ExecuteQueryRetry();
                }

                if (Owners != null && Owners.Count > 0)
                {
                    var admins = new List<UserEntity>();
                    foreach (var owner in Owners)
                    {
                        var userEntity = new UserEntity { LoginName = owner };
                        admins.Add(userEntity);
                    }
                    Tenant.AddAdministrators(admins, new Uri(Url));
                }
            }
        }

        private SiteProperties GetSiteProperties(string url)
        {
            return Tenant.GetSitePropertiesByUrl(url, true);
        }

        private bool TimeoutFunction(TenantOperationMessage message)
        {
            if (message == TenantOperationMessage.SettingSiteProperties || message == TenantOperationMessage.SettingSiteLockState)
            {
                Host.UI.Write(".");
            }
            return Stopping;
        }

    }
}
#endif