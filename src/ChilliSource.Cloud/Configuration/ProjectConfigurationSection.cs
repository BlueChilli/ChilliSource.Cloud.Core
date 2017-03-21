using ChilliSource.Cloud.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ChilliSource.Cloud.Configuration
{
    /// <summary>
    /// Represents a section for a ChilliSource web project in configuration file, which includes multiple optional sub configuration elements like s3, bugherd or youtube
    /// </summary>
    public class ProjectConfigurationSection : ConfigurationSection
    {
        /// <summary>
        /// Gets current ChilliSource.Cloud.Configuration.ProjectConfigurationSection instance from configuration file.
        /// </summary>
        /// <returns>Current ChilliSource.Cloud.Configuration.ProjectConfigurationSection instance.</returns>
        public static ProjectConfigurationSection GetConfig(string sectionName)
        {
            var config = (ProjectConfigurationSection)ConfigurationManager.GetSection(sectionName);
            config = (config == null) ? new ProjectConfigurationSection() : config;

            return config;
        }        

        /// <summary>
        /// Same as GlobalConfiguration.Instance.ProjectConfigurationSection
        /// </summary>
        /// <returns></returns>
        public static ProjectConfigurationSection GetConfig()
        {
            return GlobalConfiguration.Instance.GetProjectConfigurationSection();
        }

        /// <summary>
        /// Build a url using base url to replace "~"
        /// </summary>
        /// <param name="url">url to build. Pass in the format "~/myurl/tobuild"</param>
        /// <param name="parameters">anonymous object whose properties are turned into querystring key value pairs</param>
        /// <returns></returns>
        public string ResolveUrl(string url, object parameters = null)
        {
            url = url.Replace("~", this.BaseUrl);
            if (parameters != null)
            {
                var queryString = new NameValueCollection();

                Type t = parameters.GetType();
                foreach (var property in t.GetProperties())
                {
                    queryString[property.Name] = property.GetValue(parameters, null) == null ? "" : property.GetValue(parameters, null).ToString();
                }
                url = String.Format("{0}?{1}", url, queryString.ToString());
            }

            return url;
        }

        /// <summary>
        /// Gets or sets a value of base url for the project configuration.
        /// </summary>
        [ConfigurationProperty("baseUrl", IsRequired = true)]
        public string BaseUrl
        {
            get { return (string)this["baseUrl"]; }
            set { this["baseUrl"] = value; }
        }

        /// <summary>
        /// Gets or sets a value of host name for the project configuration.
        /// </summary>
        /// <returns></returns>
        public string HostName()
        {
            return new Uri(BaseUrl).Host;
        }        

        /// <summary>
        /// Public url is used when public site is hosted elsewhere from the application. Eq squarespace. Defaults to BaseUrl if not entered.
        /// </summary>
        [ConfigurationProperty("publicUrl", IsRequired = false)]
        public string PublicUrl
        {
            get { return (string)(this["publicUrl"] ?? BaseUrl); }
            set { this["publicUrl"] = value; }
        }

        /// <summary>
        /// Gets or sets a value of unique project id for this project.
        /// </summary>
        [ConfigurationProperty("projectId", DefaultValue = null)]
        public Guid? ProjectId
        {
            get
            {
                return (Guid?)this["projectId"];
            }
            set
            {
                this["projectId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value of project name, which is used in some html and meta helpers as a default value.
        /// </summary>
        [ConfigurationProperty("projectName", IsRequired = true)]
        public string ProjectName
        {
            get
            {
                return (string)this["projectName"];
            }
            set
            {
                this["projectName"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value of project display name.
        /// </summary>
        [ConfigurationProperty("projectDisplayName", IsRequired = false)]
        public string ProjectDisplayName
        {
            get
            {
                var name = (string)this["projectDisplayName"];
                return name.DefaultTo(ProjectName);
            }
            set
            {
                this["projectDisplayName"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value representing ChilliSource.Cloud.Configuration.ProjectEnvironment for this configuration targeting.
        /// </summary>
        [ConfigurationProperty("projectEnvironment", DefaultValue = ProjectEnvironment.Production)]
        public ProjectEnvironment ProjectEnvironment
        {
            get
            {
                return (ProjectEnvironment)this["projectEnvironment"];
            }
            set
            {
                this["projectEnvironment"] = value;
            }
        }

        /// <summary>
        /// Gets a value representing ChilliSource.Cloud.Configuration.GoogleAnalyticsElement from configuration file.
        /// </summary>
        [ConfigurationProperty("googleAnalytics", IsRequired = false)]
        public GoogleAnalyticsElement GoogleAnalytics
        {
            get { return this["googleAnalytics"] as GoogleAnalyticsElement; }
        }

        /// <summary>
        /// Gets a value representing ChilliSource.Cloud.Configuration.YouTubeElement from configuration file.
        /// </summary>
        [ConfigurationProperty("YouTube", IsRequired = false)]
        public YouTubeElement YouTube
        {
            get { return this["YouTube"] as YouTubeElement; }
        }

        /// <summary>
        /// Gets a value representing ChilliSource.Cloud.Configuration.FacebookElement from configuration file.
        /// </summary>
        [ConfigurationProperty("facebook", IsRequired = false)]
        public FacebookElement Facebook
        {
            get { return this["facebook"] as FacebookElement; }
        }

        /// <summary>
        /// Gets a value representing ChilliSource.Cloud.Configuration.EmailTemplateElement from configuration file.
        /// </summary>
        [ConfigurationProperty("emailTemplate", IsRequired = false)]
        public EmailTemplateElement EmailTemplate
        {
            get { return this["emailTemplate"] as EmailTemplateElement; }
        }

        /// <summary>
        /// Gets a value representing ChilliSource.Cloud.Configuration.BugHerdElement from configuration file.
        /// </summary>
        [ConfigurationProperty("bugherd", IsRequired = false)]
        public BugHerdElement BugHerd
        {
            get { return this["bugherd"] as BugHerdElement; }
        }

        /// <summary>
        /// Gets a value representing ChilliSource.Cloud.Configuration.UserVoiceElement from configuration file.
        /// </summary>
        [ConfigurationProperty("uservoice", IsRequired = false)]
        public UserVoiceElement UserVoice
        {
            get { return this["uservoice"] as UserVoiceElement; }
        }

        /// <summary>
        /// Gets a value representing ChilliSource.Cloud.Configuration.FileStorageElement from configuration file.
        /// </summary>
        [ConfigurationProperty("filestorage", IsRequired = false)]
        public FileStorageElement FileStorage
        {
            get
            {

                var element = this["filestorage"] as FileStorageElement ?? new FileStorageElement(new S3Element(), new AzureStorageElement());
                return element;
            }
        }

        /// <summary>
        /// Gets a value representing ChilliSource.Cloud.Configuration.GoogleApisElement from configuration file.
        /// </summary>
        [ConfigurationProperty("googleApis", IsRequired = false)]
        public GoogleApisElement GoogleApis
        {
            get { return this["googleApis"] as GoogleApisElement; }
        }

        /// <summary>
        /// Gets a value representing ChilliSource.Cloud.Configuration.MixpanelElement from configuration file.
        /// </summary>
        [ConfigurationProperty("mixpanel", IsRequired = false)]
        public MixpanelElement Mixpanel
        {
            get { return this["mixpanel"] as MixpanelElement; }
        }

        /// <summary>
        /// Gets a value representing ChilliSource.Cloud.Configuration.MandrillElement from configuration file.
        /// </summary>
        [ConfigurationProperty("mandrill", IsRequired = false)]
        public MandrillElement Mandrill
        {
            get { return this["mandrill"] as MandrillElement; }
        }
    }

    #region Custom elements
    /// <summary>
    /// Represents Google Analytics configuration element within a configuration file.
    /// </summary>
    public class GoogleAnalyticsElement : ConfigurationElement
    {
        /// <summary>
        /// Initializes a new instance of the System.Configuration.GoogleAnalyticsElement class.
        /// </summary>
        public GoogleAnalyticsElement() { }

        /// <summary>
        /// Initializes a new instance of the System.Configuration.GoogleAnalyticsElement class.
        /// </summary>
        /// <param name="enabled">A Boolean value to indicate whether the configuration is enabled or not.</param>
        /// <param name="account">Google Analytics account name.</param>
        public GoogleAnalyticsElement(bool enabled, string account)
        {
            this.Enabled = enabled;
            this.Account = account;
        }

        /// <summary>
        /// Gets or sets a value to indicate whether the configuration is enabled or not.
        /// </summary>
        [ConfigurationProperty("enabled", DefaultValue = false)]
        public bool Enabled
        {
            get { return (bool)this["enabled"]; }
            set { this["enabled"] = value; }
        }

        /// <summary>
        /// Gets or sets a value of Google Analytics account name.
        /// </summary>
        [ConfigurationProperty("account", IsRequired = true)]
        public string Account
        {
            get { return (string)this["account"]; }
            set { this["account"] = value; }
        }
    }

    /// <summary>
    /// Represents YouTube configuration element within a configuration file.
    /// </summary>
    public class YouTubeElement : ConfigurationElement
    {
        /// <summary>
        /// Initializes a new instance of the System.Configuration.YouTubeElement class.
        /// </summary>
        public YouTubeElement() { }

        /// <summary>
        /// Gets or sets a value of YouTube application name.
        /// </summary>
        [ConfigurationProperty("applicationName", IsRequired = true)]
        public string ApplicationName
        {
            get { return (string)this["applicationName"]; }
            set { this["applicationName"] = value; }
        }

        /// <summary>
        /// Gets or sets a value of YouTube developer key.
        /// </summary>
        [ConfigurationProperty("developerKey", IsRequired = true)]
        public string DeveloperKey
        {
            get { return (string)this["developerKey"]; }
            set { this["developerKey"] = value; }
        }

        /// <summary>
        /// Gets or sets a value of YouTube Username.
        /// </summary>
        [ConfigurationProperty("userName", IsRequired = true)]
        public string UserName
        {
            get { return (string)this["userName"]; }
            set { this["userName"] = value; }
        }

        /// <summary>
        /// Gets or sets a value of YouTube password.
        /// </summary>
        [ConfigurationProperty("password", IsRequired = true)]
        public string Password
        {
            get { return (string)this["password"]; }
            set { this["password"] = value; }
        }
    }

    /// <summary>
    /// Represents Facebook configuration element within a configuration file.
    /// </summary>
    public class FacebookElement : ConfigurationElement
    {
        /// <summary>
        /// Initializes a new instance of the System.Configuration.FacebookElement class.
        /// </summary>
        public FacebookElement() { }

        /// <summary>
        /// Gets or sets a value of Facebook application Id.
        /// </summary>
        [ConfigurationProperty("facebookAppId", IsRequired = true)]
        public string FacebookAppId
        {
            get { return (string)this["facebookAppId"]; }
            set { this["facebookAppId"] = value; }
        }

        /// <summary>
        /// Gets or sets a value of Facebook Application secret key.
        /// </summary>
        [ConfigurationProperty("facebookAppSecret", IsRequired = true)]
        public string FacebookAppSecret
        {
            get { return (string)this["facebookAppSecret"]; }
            set { this["facebookAppSecret"] = value; }
        }

        /// <summary>
        /// Gets or sets a value to indicate whether if Facebook is offline or not.
        /// </summary>
        [ConfigurationProperty("offline", DefaultValue = false)]
        public bool Offline
        {
            get { return (bool)this["offline"]; }
            set { this["offline"] = value; }
        }

        /// <summary>
        /// Gets or sets a value of Facebook offline user name.
        /// </summary>
        [ConfigurationProperty("offlineUser", IsRequired = false)]
        public string OfflineUser
        {
            get { return (string)this["offlineUser"]; }
            set { this["offlineUser"] = value; }
        }
    }

    /// <summary>
    /// Represents Email Template configuration element within a configuration file.
    /// </summary>
    public class EmailTemplateElement : ConfigurationElement
    {
        /// <summary>
        /// Initializes a new instance of the System.Configuration.EmailTemplateElement class.
        /// </summary>
        public EmailTemplateElement() { }

        /// <summary>
        /// Gets or sets a value to indicate whether the Email Template is enabled or not.
        /// </summary>
        [ConfigurationProperty("enabled", DefaultValue = false)]
        public bool Enabled
        {
            get { return (bool)this["enabled"]; }
            set { this["enabled"] = value; }
        }

        /// <summary>
        /// Gets or sets a value of Url used in Email Template.
        /// </summary>
        [ConfigurationProperty("url", IsRequired = false)]
        public string Url
        {
            get { return (string)this["url"]; }
            set { this["url"] = value; }
        }

        /// <summary>
        /// Gets or sets a value to redirect emails.
        /// </summary>
        [ConfigurationProperty("redirectTo", IsRequired = false)]
        public string RedirectTo
        {
            get { return (string)this["redirectTo"]; }
            set { this["redirectTo"] = value; }
        }

        /// <summary>
        /// Gets or sets a value of the blind carbon copy (bcc) for all emails.
        /// </summary>
        [ConfigurationProperty("bcc", IsRequired = false)]
        public string Bcc
        {
            get { return (string)this["bcc"]; }
            set { this["bcc"] = value; }
        }
    }

    /// <summary>
    /// Represents BugHerd configuration element within a configuration file.
    /// </summary>
    public class BugHerdElement : ConfigurationElement
    {
        /// <summary>
        /// Initializes a new instance of the System.Configuration.BugHerdElement class.
        /// </summary>
        public BugHerdElement() { }

        /// <summary>
        /// Gets or sets a value to indicate whether the BugHerd is enabled or not.
        /// </summary>
        [ConfigurationProperty("enabled", DefaultValue = true)]
        public bool Enabled
        {
            get { return (bool)this["enabled"]; }
            set { this["enabled"] = value; }
        }

        /// <summary>
        /// Gets or sets a value of BugHerd API key.
        /// </summary>
        [ConfigurationProperty("apiKey", IsRequired = true)]
        public string ApiKey
        {
            get { return (string)this["apiKey"]; }
            set { this["apiKey"] = value; }
        }

        /// <summary>
        /// Gets or sets a value of BugHerd alternative API key.
        /// </summary>
        [ConfigurationProperty("alternativeApiKey", IsRequired = false)]
        public string AlternativeApiKey
        {
            get { return (string)this["alternativeApiKey"]; }
            set { this["alternativeApiKey"] = value; }
        }
    }

    /// <summary>
    /// Represents UserVoice configuration element within a configuration file.
    /// </summary>
    public class UserVoiceElement : ConfigurationElement
    {
        /// <summary>
        /// Initializes a new instance of the System.Configuration.UserVoiceElement class.
        /// </summary>
        public UserVoiceElement() { }

        /// <summary>
        /// Gets or sets a value to indicate whether the UserVoice is enabled or not.
        /// </summary>
        [ConfigurationProperty("enabled", DefaultValue = true)]
        public bool Enabled
        {
            get { return (bool)this["enabled"]; }
            set { this["enabled"] = value; }
        }

        /// <summary>
        /// Gets or sets a value of UserVoice API key.
        /// </summary>
        [ConfigurationProperty("apiKey", IsRequired = true)]
        public string ApiKey
        {
            get { return (string)this["apiKey"]; }
            set { this["apiKey"] = value; }
        }
    }

    /// <summary>
    /// Represents S3 configuration element within a configuration file.
    /// </summary>
    public class S3Element : ConfigurationElement
    {
        /// <summary>
        /// Initializes a new instance of the System.Configuration.S3Element class.
        /// </summary>
        public S3Element() { }

        /// <summary>
        /// Gets or sets a value of S3 access key Id.
        /// </summary>
        [ConfigurationProperty("accessKeyId")]
        public string AccessKeyId { get { return (string)this["accessKeyId"]; } set { this["accessKeyId"] = value; } }

        /// <summary>
        /// Gets or sets a value of S3 secret access key.
        /// </summary>
        [ConfigurationProperty("secretAccessKey")]
        public string SecretAccessKey { get { return (string)this["secretAccessKey"]; } set { this["secretAccessKey"] = value; } }

        /// <summary>
        /// Gets or sets a value of S3 bucket name.
        /// </summary>
        [ConfigurationProperty("bucket")]
        public string Bucket { get { return (string)this["bucket"]; } set { this["bucket"] = value; } }

        /// <summary>
        /// Gets or sets a value of S3 host name.
        /// </summary>
        [ConfigurationProperty("host", DefaultValue = "s3.amazonaws.com", IsRequired = false)]
        public string Host { get { return (string)this["host"]; } set { this["host"] = value?.Trim(); } }
    }

    public enum FileStorageProvider : int
    {
        S3 = 1,
        Azure
    }

    /// <summary>
    /// Represents azure storage configuration element within a configuration file.
    /// </summary>
    public class FileStorageElement : ConfigurationElement
    {
        /// <summary>
        /// Initializes a new instance of the System.Configuration.AzureStorageElement class.
        /// </summary>
        public FileStorageElement() { }
        public FileStorageElement(S3Element s3, AzureStorageElement azure)
        {
            this["s3"] = s3;
            this["azure"] = azure;
        }

        /// <summary>
        /// Gets or sets the default storage provider: S3 or Azure
        /// </summary>
        [ConfigurationProperty("defaultprovider", IsRequired = true)]
        public FileStorageProvider DefaultProvider { get { return (FileStorageProvider)this["defaultprovider"]; } set { this["defaultprovider"] = value; } }

        /// <summary>
        /// Gets a value representing ChilliSource.Cloud.Configuration.S3Element from configuration file.
        /// </summary>
        [ConfigurationProperty("s3", IsRequired = false)]
        public S3Element S3
        {
            get { return this["s3"] as S3Element; }
        }

        /// <summary>
        /// Gets a value representing ChilliSource.Cloud.Configuration.AzureStorageElement from configuration file.
        /// </summary>
        [ConfigurationProperty("azure", IsRequired = false)]
        public AzureStorageElement Azure
        {
            get { return this["azure"] as AzureStorageElement; }
        }
    }

    /// <summary>
    /// Represents azure storage configuration element within a configuration file.
    /// </summary>
    public class AzureStorageElement : ConfigurationElement
    {
        /// <summary>
        /// Initializes a new instance of the System.Configuration.AzureStorageElement class.
        /// </summary>
        public AzureStorageElement() { }

        /// <summary>
        /// Gets or sets a value of azure storage account name.
        /// </summary>
        [ConfigurationProperty("accountName", IsRequired = true)]
        public string AccountName { get { return (string)this["accountName"]; } set { this["accountName"] = value; } }

        /// <summary>
        /// Gets or sets a value of azure storage account key.
        /// </summary>
        [ConfigurationProperty("accountKey", IsRequired = true)]
        public string AccountKey { get { return (string)this["accountKey"]; } set { this["accountKey"] = value; } }

        /// <summary>
        /// Gets or sets a value of azure storage container.
        /// </summary>
        [ConfigurationProperty("container", IsRequired = true)]
        public string Container { get { return (string)this["container"]; } set { this["container"] = value; } }
    }

    /// <summary>
    /// Represents Google APIs configuration element within a configuration file.
    /// </summary>
    public class GoogleApisElement : ConfigurationElement
    {
        /// <summary>
        /// Initializes a new instance of the System.Configuration.GoogleApisElement class.
        /// </summary>
        public GoogleApisElement() { }

        /// <summary>
        /// Gets or sets a value of Google API develop key.
        /// </summary>
        [ConfigurationProperty("devApiKey", IsRequired = false)]
        private string DevApiKey
        {
            get { return (string)this["devApiKey"]; }
            set { this["devApiKey"] = value; }
        }

        /// <summary>
        /// Gets or sets a value of Google API production key.
        /// </summary>
        [ConfigurationProperty("prodApiKey", IsRequired = true)]
        private string ProdApiKey
        {
            get { return (string)this["prodApiKey"]; }
            set { this["prodApiKey"] = value; }
        }

        /// <summary>
        /// Gets or sets a value of Google API Libraries.
        /// </summary>
        [ConfigurationProperty("libraries", IsRequired = false)]
        public string Libraries
        {
            get { return (string)this["libraries"]; }
            set { this["libraries"] = value; }
        }

        /// <summary>
        /// Gets Google API key for specified environment.
        /// </summary>
        /// <param name="environment">A ChilliSource.Cloud.Configuration.ProjectEnvironment enum.</param>
        /// <returns>Google API key for specified environment</returns>
        public string ApiKey(ProjectEnvironment environment)
        {
            var key = environment == ProjectEnvironment.Production ? ProdApiKey : DevApiKey;
            if (String.IsNullOrEmpty(key)) throw new NotImplementedException("Key for google api not found in ProjectConfigurationSection googleApis configuration. Use google api console to setup a key https://code.google.com/apis/console");
            return key;
        }

        [ConfigurationProperty("oauth", IsRequired = false)]
        public OAuthElement OAuth
        {
            get { return this["oauth"] as OAuthElement; }
        }
    }

    public class OAuthElement : ConfigurationElement
    {
        /// <summary>
        /// Initializes a new instance of the System.Configuration.OAuthElement class.
        /// </summary>
        public OAuthElement() { }

        /// <summary>
        /// Gets or sets a value of google app client Id.
        /// </summary>
        [ConfigurationProperty("clientId")]
        public string ClientID { get { return (string)this["clientId"]; } set { this["clientId"] = value; } }

        /// <summary>
        /// Gets or sets a value of google app client secret.
        /// </summary>
        [ConfigurationProperty("clientSecret")]
        public string ClientSecret { get { return (string)this["clientSecret"]; } set { this["clientSecret"] = value; } }
    }

    /// <summary>
    /// Represents Mixpanel configuration element within a configuration file.
    /// </summary>
    public class MixpanelElement : ConfigurationElement
    {
        /// <summary>
        /// Initializes a new instance of the System.Configuration.MixpanelElement class.
        /// </summary>
        public MixpanelElement() { }

        /// <summary>
        /// Gets or sets a value to indicate whether the Mixpanel is enabled or not.
        /// </summary>
        [ConfigurationProperty("enabled", DefaultValue = true)]
        public bool Enabled
        {
            get { return (bool)this["enabled"]; }
            set { this["enabled"] = value; }
        }

        /// <summary>
        /// Gets or sets a value of Mixpanel API key.
        /// </summary>
        [ConfigurationProperty("apiKey", IsRequired = true)]
        public string ApiKey
        {
            get { return (string)this["apiKey"]; }
            set { this["apiKey"] = value; }
        }
    }

    /// <summary>
    /// Represents Mandrill configuration element within a configuration file.
    /// </summary>
    public class MandrillElement : ConfigurationElement
    {
        /// <summary>
        /// Initializes a new instance of the System.Configuration.MandrillElement class.
        /// </summary>
        public MandrillElement() { }


        /// </summary>
        [ConfigurationProperty("enabled", DefaultValue = true)]
        public bool Enabled
        {
            get { return (bool)this["enabled"]; }
            set { this["enabled"] = value; }
        }

        /// <summary>
        /// Gets or sets a value of Mandrill API key.
        /// </summary>
        [ConfigurationProperty("apiKey", IsRequired = true)]
        public string ApiKey
        {
            get { return (string)this["apiKey"]; }
            set { this["apiKey"] = value; }
        }

        /// <summary>
        /// Gets or sets a whether to use https.
        /// </summary>
        [ConfigurationProperty("useHttps", IsRequired = false, DefaultValue = false)]
        public bool UseHttps
        {
            get { return (bool)this["useHttps"]; }
            set { this["useHttps"] = value; }
        }

        /// <summary>
        /// Gets or sets a whether to use subaccount.
        /// </summary> 
        [ConfigurationProperty("subaccount", IsRequired = false, DefaultValue = "")]
        public string SubAccount
        {
            get { return (string)this["subaccount"]; }
            set { this["subaccount"] = value; }
        }
        // <summary>
        /// Gets or sets from email address.
        /// </summary> 
        [ConfigurationProperty("fromEmail", IsRequired = false, DefaultValue = "")]
        public string FromEmail
        {
            get { return (string)this["fromEmail"]; }
            set { this["fromEmail"] = value; }
        }
        // <summary>
        /// Gets or sets from name.
        /// </summary> 
        [ConfigurationProperty("fromName", IsRequired = false, DefaultValue = "")]
        public string FromName
        {
            get { return (string)this["fromName"]; }
            set { this["fromName"] = value; }
        }

    }

    #endregion


    /// <summary>
    /// Enumeration values of environment of the project.
    /// </summary>
    public enum ProjectEnvironment
    {
        /// <summary>
        /// Development environment of the project.
        /// </summary>
        Development,
        /// <summary>
        /// UAT environment of the project.
        /// </summary>
        UAT,
        /// <summary>
        /// Staging environment of the project.
        /// </summary>
        Staging,
        /// <summary>
        /// Production environment of the project.
        /// </summary>
        Production,

        Local
    }
}
