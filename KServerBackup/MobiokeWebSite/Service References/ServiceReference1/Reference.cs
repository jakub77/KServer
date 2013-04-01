﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18033
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MobiokeWebSite.ServiceReference1 {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Response", Namespace="http://schemas.datacontract.org/2004/07/KServer")]
    [System.SerializableAttribute()]
    public partial class Response : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool errorField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string messageField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int resultField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool error {
            get {
                return this.errorField;
            }
            set {
                if ((this.errorField.Equals(value) != true)) {
                    this.errorField = value;
                    this.RaisePropertyChanged("error");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string message {
            get {
                return this.messageField;
            }
            set {
                if ((object.ReferenceEquals(this.messageField, value) != true)) {
                    this.messageField = value;
                    this.RaisePropertyChanged("message");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int result {
            get {
                return this.resultField;
            }
            set {
                if ((this.resultField.Equals(value) != true)) {
                    this.resultField = value;
                    this.RaisePropertyChanged("result");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="Venue", Namespace="http://schemas.datacontract.org/2004/07/KServer")]
    [System.SerializableAttribute()]
    public partial class Venue : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string venueAddressField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int venueIDField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string venueNameField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string venueAddress {
            get {
                return this.venueAddressField;
            }
            set {
                if ((object.ReferenceEquals(this.venueAddressField, value) != true)) {
                    this.venueAddressField = value;
                    this.RaisePropertyChanged("venueAddress");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int venueID {
            get {
                return this.venueIDField;
            }
            set {
                if ((this.venueIDField.Equals(value) != true)) {
                    this.venueIDField = value;
                    this.RaisePropertyChanged("venueID");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string venueName {
            get {
                return this.venueNameField;
            }
            set {
                if ((object.ReferenceEquals(this.venueNameField, value) != true)) {
                    this.venueNameField = value;
                    this.RaisePropertyChanged("venueName");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ServiceReference1.IWebsite")]
    public interface IWebsite {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWebsite/SendEmailWithUsername", ReplyAction="http://tempuri.org/IWebsite/SendEmailWithUsernameResponse")]
        MobiokeWebSite.ServiceReference1.Response SendEmailWithUsername(string email);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWebsite/SendEmailWithUsername", ReplyAction="http://tempuri.org/IWebsite/SendEmailWithUsernameResponse")]
        System.Threading.Tasks.Task<MobiokeWebSite.ServiceReference1.Response> SendEmailWithUsernameAsync(string email);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWebsite/StartPasswordReset", ReplyAction="http://tempuri.org/IWebsite/StartPasswordResetResponse")]
        MobiokeWebSite.ServiceReference1.Response StartPasswordReset(string email, string username, bool isDJ, string websiteAddress);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWebsite/StartPasswordReset", ReplyAction="http://tempuri.org/IWebsite/StartPasswordResetResponse")]
        System.Threading.Tasks.Task<MobiokeWebSite.ServiceReference1.Response> StartPasswordResetAsync(string email, string username, bool isDJ, string websiteAddress);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWebsite/ValidatePasswordResetKey", ReplyAction="http://tempuri.org/IWebsite/ValidatePasswordResetKeyResponse")]
        MobiokeWebSite.ServiceReference1.ValidatePasswordResetKeyResponse ValidatePasswordResetKey(MobiokeWebSite.ServiceReference1.ValidatePasswordResetKeyRequest request);
        
        // CODEGEN: Generating message contract since the operation has multiple return values.
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWebsite/ValidatePasswordResetKey", ReplyAction="http://tempuri.org/IWebsite/ValidatePasswordResetKeyResponse")]
        System.Threading.Tasks.Task<MobiokeWebSite.ServiceReference1.ValidatePasswordResetKeyResponse> ValidatePasswordResetKeyAsync(MobiokeWebSite.ServiceReference1.ValidatePasswordResetKeyRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWebsite/UsePasswordResetKey", ReplyAction="http://tempuri.org/IWebsite/UsePasswordResetKeyResponse")]
        MobiokeWebSite.ServiceReference1.UsePasswordResetKeyResponse UsePasswordResetKey(MobiokeWebSite.ServiceReference1.UsePasswordResetKeyRequest request);
        
        // CODEGEN: Generating message contract since the operation has multiple return values.
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWebsite/UsePasswordResetKey", ReplyAction="http://tempuri.org/IWebsite/UsePasswordResetKeyResponse")]
        System.Threading.Tasks.Task<MobiokeWebSite.ServiceReference1.UsePasswordResetKeyResponse> UsePasswordResetKeyAsync(MobiokeWebSite.ServiceReference1.UsePasswordResetKeyRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWebsite/Login", ReplyAction="http://tempuri.org/IWebsite/LoginResponse")]
        MobiokeWebSite.ServiceReference1.LoginResponse Login(MobiokeWebSite.ServiceReference1.LoginRequest request);
        
        // CODEGEN: Generating message contract since the operation has multiple return values.
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWebsite/Login", ReplyAction="http://tempuri.org/IWebsite/LoginResponse")]
        System.Threading.Tasks.Task<MobiokeWebSite.ServiceReference1.LoginResponse> LoginAsync(MobiokeWebSite.ServiceReference1.LoginRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWebsite/ChangePassword", ReplyAction="http://tempuri.org/IWebsite/ChangePasswordResponse")]
        MobiokeWebSite.ServiceReference1.Response ChangePassword(int ID, string role, string newPassword);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWebsite/ChangePassword", ReplyAction="http://tempuri.org/IWebsite/ChangePasswordResponse")]
        System.Threading.Tasks.Task<MobiokeWebSite.ServiceReference1.Response> ChangePasswordAsync(int ID, string role, string newPassword);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWebsite/ChangeEmail", ReplyAction="http://tempuri.org/IWebsite/ChangeEmailResponse")]
        MobiokeWebSite.ServiceReference1.Response ChangeEmail(int ID, string role, string newEmail);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWebsite/ChangeEmail", ReplyAction="http://tempuri.org/IWebsite/ChangeEmailResponse")]
        System.Threading.Tasks.Task<MobiokeWebSite.ServiceReference1.Response> ChangeEmailAsync(int ID, string role, string newEmail);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWebsite/DJSignUp", ReplyAction="http://tempuri.org/IWebsite/DJSignUpResponse")]
        MobiokeWebSite.ServiceReference1.Response DJSignUp(string username, string password, MobiokeWebSite.ServiceReference1.Venue venue, string email);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWebsite/DJSignUp", ReplyAction="http://tempuri.org/IWebsite/DJSignUpResponse")]
        System.Threading.Tasks.Task<MobiokeWebSite.ServiceReference1.Response> DJSignUpAsync(string username, string password, MobiokeWebSite.ServiceReference1.Venue venue, string email);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWebsite/MobileSignUp", ReplyAction="http://tempuri.org/IWebsite/MobileSignUpResponse")]
        MobiokeWebSite.ServiceReference1.Response MobileSignUp(string username, string password, string email);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWebsite/MobileSignUp", ReplyAction="http://tempuri.org/IWebsite/MobileSignUpResponse")]
        System.Threading.Tasks.Task<MobiokeWebSite.ServiceReference1.Response> MobileSignUpAsync(string username, string password, string email);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWebsite/EnableDisableRegistration", ReplyAction="http://tempuri.org/IWebsite/EnableDisableRegistrationResponse")]
        MobiokeWebSite.ServiceReference1.Response EnableDisableRegistration(bool enableRegistration);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWebsite/EnableDisableRegistration", ReplyAction="http://tempuri.org/IWebsite/EnableDisableRegistrationResponse")]
        System.Threading.Tasks.Task<MobiokeWebSite.ServiceReference1.Response> EnableDisableRegistrationAsync(bool enableRegistration);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWebsite/IsRegistrationAllowed", ReplyAction="http://tempuri.org/IWebsite/IsRegistrationAllowedResponse")]
        MobiokeWebSite.ServiceReference1.IsRegistrationAllowedResponse IsRegistrationAllowed(MobiokeWebSite.ServiceReference1.IsRegistrationAllowedRequest request);
        
        // CODEGEN: Generating message contract since the operation has multiple return values.
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWebsite/IsRegistrationAllowed", ReplyAction="http://tempuri.org/IWebsite/IsRegistrationAllowedResponse")]
        System.Threading.Tasks.Task<MobiokeWebSite.ServiceReference1.IsRegistrationAllowedResponse> IsRegistrationAllowedAsync(MobiokeWebSite.ServiceReference1.IsRegistrationAllowedRequest request);
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(WrapperName="ValidatePasswordResetKey", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class ValidatePasswordResetKeyRequest {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public string key;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=1)]
        public bool isDJ;
        
        public ValidatePasswordResetKeyRequest() {
        }
        
        public ValidatePasswordResetKeyRequest(string key, bool isDJ) {
            this.key = key;
            this.isDJ = isDJ;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(WrapperName="ValidatePasswordResetKeyResponse", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class ValidatePasswordResetKeyResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public MobiokeWebSite.ServiceReference1.Response ValidatePasswordResetKeyResult;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=1)]
        public int ID;
        
        public ValidatePasswordResetKeyResponse() {
        }
        
        public ValidatePasswordResetKeyResponse(MobiokeWebSite.ServiceReference1.Response ValidatePasswordResetKeyResult, int ID) {
            this.ValidatePasswordResetKeyResult = ValidatePasswordResetKeyResult;
            this.ID = ID;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(WrapperName="UsePasswordResetKey", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class UsePasswordResetKeyRequest {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public string key;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=1)]
        public bool isDJ;
        
        public UsePasswordResetKeyRequest() {
        }
        
        public UsePasswordResetKeyRequest(string key, bool isDJ) {
            this.key = key;
            this.isDJ = isDJ;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(WrapperName="UsePasswordResetKeyResponse", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class UsePasswordResetKeyResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public MobiokeWebSite.ServiceReference1.Response UsePasswordResetKeyResult;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=1)]
        public int ID;
        
        public UsePasswordResetKeyResponse() {
        }
        
        public UsePasswordResetKeyResponse(MobiokeWebSite.ServiceReference1.Response UsePasswordResetKeyResult, int ID) {
            this.UsePasswordResetKeyResult = UsePasswordResetKeyResult;
            this.ID = ID;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(WrapperName="Login", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class LoginRequest {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public string username;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=1)]
        public string password;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=2)]
        public string role;
        
        public LoginRequest() {
        }
        
        public LoginRequest(string username, string password, string role) {
            this.username = username;
            this.password = password;
            this.role = role;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(WrapperName="LoginResponse", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class LoginResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public MobiokeWebSite.ServiceReference1.Response LoginResult;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=1)]
        public int ID;
        
        public LoginResponse() {
        }
        
        public LoginResponse(MobiokeWebSite.ServiceReference1.Response LoginResult, int ID) {
            this.LoginResult = LoginResult;
            this.ID = ID;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(WrapperName="IsRegistrationAllowed", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class IsRegistrationAllowedRequest {
        
        public IsRegistrationAllowedRequest() {
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(WrapperName="IsRegistrationAllowedResponse", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class IsRegistrationAllowedResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public MobiokeWebSite.ServiceReference1.Response IsRegistrationAllowedResult;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=1)]
        public bool registrationAllowed;
        
        public IsRegistrationAllowedResponse() {
        }
        
        public IsRegistrationAllowedResponse(MobiokeWebSite.ServiceReference1.Response IsRegistrationAllowedResult, bool registrationAllowed) {
            this.IsRegistrationAllowedResult = IsRegistrationAllowedResult;
            this.registrationAllowed = registrationAllowed;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IWebsiteChannel : MobiokeWebSite.ServiceReference1.IWebsite, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class WebsiteClient : System.ServiceModel.ClientBase<MobiokeWebSite.ServiceReference1.IWebsite>, MobiokeWebSite.ServiceReference1.IWebsite {
        
        public WebsiteClient() {
        }
        
        public WebsiteClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public WebsiteClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public WebsiteClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public WebsiteClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public MobiokeWebSite.ServiceReference1.Response SendEmailWithUsername(string email) {
            return base.Channel.SendEmailWithUsername(email);
        }
        
        public System.Threading.Tasks.Task<MobiokeWebSite.ServiceReference1.Response> SendEmailWithUsernameAsync(string email) {
            return base.Channel.SendEmailWithUsernameAsync(email);
        }
        
        public MobiokeWebSite.ServiceReference1.Response StartPasswordReset(string email, string username, bool isDJ, string websiteAddress) {
            return base.Channel.StartPasswordReset(email, username, isDJ, websiteAddress);
        }
        
        public System.Threading.Tasks.Task<MobiokeWebSite.ServiceReference1.Response> StartPasswordResetAsync(string email, string username, bool isDJ, string websiteAddress) {
            return base.Channel.StartPasswordResetAsync(email, username, isDJ, websiteAddress);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        MobiokeWebSite.ServiceReference1.ValidatePasswordResetKeyResponse MobiokeWebSite.ServiceReference1.IWebsite.ValidatePasswordResetKey(MobiokeWebSite.ServiceReference1.ValidatePasswordResetKeyRequest request) {
            return base.Channel.ValidatePasswordResetKey(request);
        }
        
        public MobiokeWebSite.ServiceReference1.Response ValidatePasswordResetKey(string key, bool isDJ, out int ID) {
            MobiokeWebSite.ServiceReference1.ValidatePasswordResetKeyRequest inValue = new MobiokeWebSite.ServiceReference1.ValidatePasswordResetKeyRequest();
            inValue.key = key;
            inValue.isDJ = isDJ;
            MobiokeWebSite.ServiceReference1.ValidatePasswordResetKeyResponse retVal = ((MobiokeWebSite.ServiceReference1.IWebsite)(this)).ValidatePasswordResetKey(inValue);
            ID = retVal.ID;
            return retVal.ValidatePasswordResetKeyResult;
        }
        
        public System.Threading.Tasks.Task<MobiokeWebSite.ServiceReference1.ValidatePasswordResetKeyResponse> ValidatePasswordResetKeyAsync(MobiokeWebSite.ServiceReference1.ValidatePasswordResetKeyRequest request) {
            return base.Channel.ValidatePasswordResetKeyAsync(request);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        MobiokeWebSite.ServiceReference1.UsePasswordResetKeyResponse MobiokeWebSite.ServiceReference1.IWebsite.UsePasswordResetKey(MobiokeWebSite.ServiceReference1.UsePasswordResetKeyRequest request) {
            return base.Channel.UsePasswordResetKey(request);
        }
        
        public MobiokeWebSite.ServiceReference1.Response UsePasswordResetKey(string key, bool isDJ, out int ID) {
            MobiokeWebSite.ServiceReference1.UsePasswordResetKeyRequest inValue = new MobiokeWebSite.ServiceReference1.UsePasswordResetKeyRequest();
            inValue.key = key;
            inValue.isDJ = isDJ;
            MobiokeWebSite.ServiceReference1.UsePasswordResetKeyResponse retVal = ((MobiokeWebSite.ServiceReference1.IWebsite)(this)).UsePasswordResetKey(inValue);
            ID = retVal.ID;
            return retVal.UsePasswordResetKeyResult;
        }
        
        public System.Threading.Tasks.Task<MobiokeWebSite.ServiceReference1.UsePasswordResetKeyResponse> UsePasswordResetKeyAsync(MobiokeWebSite.ServiceReference1.UsePasswordResetKeyRequest request) {
            return base.Channel.UsePasswordResetKeyAsync(request);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        MobiokeWebSite.ServiceReference1.LoginResponse MobiokeWebSite.ServiceReference1.IWebsite.Login(MobiokeWebSite.ServiceReference1.LoginRequest request) {
            return base.Channel.Login(request);
        }
        
        public MobiokeWebSite.ServiceReference1.Response Login(string username, string password, string role, out int ID) {
            MobiokeWebSite.ServiceReference1.LoginRequest inValue = new MobiokeWebSite.ServiceReference1.LoginRequest();
            inValue.username = username;
            inValue.password = password;
            inValue.role = role;
            MobiokeWebSite.ServiceReference1.LoginResponse retVal = ((MobiokeWebSite.ServiceReference1.IWebsite)(this)).Login(inValue);
            ID = retVal.ID;
            return retVal.LoginResult;
        }
        
        public System.Threading.Tasks.Task<MobiokeWebSite.ServiceReference1.LoginResponse> LoginAsync(MobiokeWebSite.ServiceReference1.LoginRequest request) {
            return base.Channel.LoginAsync(request);
        }
        
        public MobiokeWebSite.ServiceReference1.Response ChangePassword(int ID, string role, string newPassword) {
            return base.Channel.ChangePassword(ID, role, newPassword);
        }
        
        public System.Threading.Tasks.Task<MobiokeWebSite.ServiceReference1.Response> ChangePasswordAsync(int ID, string role, string newPassword) {
            return base.Channel.ChangePasswordAsync(ID, role, newPassword);
        }
        
        public MobiokeWebSite.ServiceReference1.Response ChangeEmail(int ID, string role, string newEmail) {
            return base.Channel.ChangeEmail(ID, role, newEmail);
        }
        
        public System.Threading.Tasks.Task<MobiokeWebSite.ServiceReference1.Response> ChangeEmailAsync(int ID, string role, string newEmail) {
            return base.Channel.ChangeEmailAsync(ID, role, newEmail);
        }
        
        public MobiokeWebSite.ServiceReference1.Response DJSignUp(string username, string password, MobiokeWebSite.ServiceReference1.Venue venue, string email) {
            return base.Channel.DJSignUp(username, password, venue, email);
        }
        
        public System.Threading.Tasks.Task<MobiokeWebSite.ServiceReference1.Response> DJSignUpAsync(string username, string password, MobiokeWebSite.ServiceReference1.Venue venue, string email) {
            return base.Channel.DJSignUpAsync(username, password, venue, email);
        }
        
        public MobiokeWebSite.ServiceReference1.Response MobileSignUp(string username, string password, string email) {
            return base.Channel.MobileSignUp(username, password, email);
        }
        
        public System.Threading.Tasks.Task<MobiokeWebSite.ServiceReference1.Response> MobileSignUpAsync(string username, string password, string email) {
            return base.Channel.MobileSignUpAsync(username, password, email);
        }
        
        public MobiokeWebSite.ServiceReference1.Response EnableDisableRegistration(bool enableRegistration) {
            return base.Channel.EnableDisableRegistration(enableRegistration);
        }
        
        public System.Threading.Tasks.Task<MobiokeWebSite.ServiceReference1.Response> EnableDisableRegistrationAsync(bool enableRegistration) {
            return base.Channel.EnableDisableRegistrationAsync(enableRegistration);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        MobiokeWebSite.ServiceReference1.IsRegistrationAllowedResponse MobiokeWebSite.ServiceReference1.IWebsite.IsRegistrationAllowed(MobiokeWebSite.ServiceReference1.IsRegistrationAllowedRequest request) {
            return base.Channel.IsRegistrationAllowed(request);
        }
        
        public MobiokeWebSite.ServiceReference1.Response IsRegistrationAllowed(out bool registrationAllowed) {
            MobiokeWebSite.ServiceReference1.IsRegistrationAllowedRequest inValue = new MobiokeWebSite.ServiceReference1.IsRegistrationAllowedRequest();
            MobiokeWebSite.ServiceReference1.IsRegistrationAllowedResponse retVal = ((MobiokeWebSite.ServiceReference1.IWebsite)(this)).IsRegistrationAllowed(inValue);
            registrationAllowed = retVal.registrationAllowed;
            return retVal.IsRegistrationAllowedResult;
        }
        
        public System.Threading.Tasks.Task<MobiokeWebSite.ServiceReference1.IsRegistrationAllowedResponse> IsRegistrationAllowedAsync(MobiokeWebSite.ServiceReference1.IsRegistrationAllowedRequest request) {
            return base.Channel.IsRegistrationAllowedAsync(request);
        }
    }
}