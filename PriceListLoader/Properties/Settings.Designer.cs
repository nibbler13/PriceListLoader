﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PriceListLoader.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.1.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("172.16.9.9")]
        public string MisDbAddress {
            get {
                return ((string)(this["MisDbAddress"]));
            }
            set {
                this["MisDbAddress"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("central")]
        public string MisDbName {
            get {
                return ((string)(this["MisDbName"]));
            }
            set {
                this["MisDbName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("sysdba")]
        public string MisDbUser {
            get {
                return ((string)(this["MisDbUser"]));
            }
            set {
                this["MisDbUser"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("masterkey")]
        public string MisDbPassword {
            get {
                return ((string)(this["MisDbPassword"]));
            }
            set {
                this["MisDbPassword"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("select\r\n  sprice\r\nfrom\r\n  get_pricebyid (\r\n    current_date,\r\n    13 /*13 НАЛ, 14" +
            " БНАЛ*/,\r\n    @filid,\r\n    38,\r\n    0,\r\n    (select\r\n      schid\r\n    from\r\n    " +
            "  wschema\r\n    where\r\n      kodoper = @kodoper\r\n      and structid = 4\r\n    )\r\n " +
            " )")]
        public string MisDbSelectPriceByCode {
            get {
                return ((string)(this["MisDbSelectPriceByCode"]));
            }
            set {
                this["MisDbSelectPriceByCode"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("donotreply")]
        public string MailUser {
            get {
                return ((string)(this["MailUser"]));
            }
            set {
                this["MailUser"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("klNhr1hi9qp55weewAXd")]
        public string MailPassword {
            get {
                return ((string)(this["MailPassword"]));
            }
            set {
                this["MailPassword"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("bzklinika.ru")]
        public string MailDomain {
            get {
                return ((string)(this["MailDomain"]));
            }
            set {
                this["MailDomain"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("mail.bzklinika.ru")]
        public string MailSmtpServer {
            get {
                return ((string)(this["MailSmtpServer"]));
            }
            set {
                this["MailSmtpServer"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("nn-admin@bzklinika.ru")]
        public string MailCopy {
            get {
                return ((string)(this["MailCopy"]));
            }
            set {
                this["MailCopy"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("s.a.starodymov@bzklinika.ru")]
        public string MailTo {
            get {
                return ((string)(this["MailTo"]));
            }
            set {
                this["MailTo"] = value;
            }
        }
    }
}
