﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18010
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DJTestClient.ServiceReference1 {
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
    [System.Runtime.Serialization.DataContractAttribute(Name="LogInResponse", Namespace="http://schemas.datacontract.org/2004/07/KServer")]
    [System.SerializableAttribute()]
    public partial class LogInResponse : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool errorField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string messageField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int resultField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private DJTestClient.ServiceReference1.User userField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private long userKeyField;
        
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
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public DJTestClient.ServiceReference1.User user {
            get {
                return this.userField;
            }
            set {
                if ((object.ReferenceEquals(this.userField, value) != true)) {
                    this.userField = value;
                    this.RaisePropertyChanged("user");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public long userKey {
            get {
                return this.userKeyField;
            }
            set {
                if ((this.userKeyField.Equals(value) != true)) {
                    this.userKeyField = value;
                    this.RaisePropertyChanged("userKey");
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
    [System.Runtime.Serialization.DataContractAttribute(Name="User", Namespace="http://schemas.datacontract.org/2004/07/KServer")]
    [System.SerializableAttribute()]
    public partial class User : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int userIDField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string userNameField;
        
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
        public int userID {
            get {
                return this.userIDField;
            }
            set {
                if ((this.userIDField.Equals(value) != true)) {
                    this.userIDField = value;
                    this.RaisePropertyChanged("userID");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string userName {
            get {
                return this.userNameField;
            }
            set {
                if ((object.ReferenceEquals(this.userNameField, value) != true)) {
                    this.userNameField = value;
                    this.RaisePropertyChanged("userName");
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
    [System.Runtime.Serialization.DataContractAttribute(Name="Session", Namespace="http://schemas.datacontract.org/2004/07/KServer")]
    [System.SerializableAttribute()]
    public partial class Session : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int sessionIDField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int venueIDField;
        
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
        public int sessionID {
            get {
                return this.sessionIDField;
            }
            set {
                if ((this.sessionIDField.Equals(value) != true)) {
                    this.sessionIDField = value;
                    this.RaisePropertyChanged("sessionID");
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
    [System.Runtime.Serialization.DataContractAttribute(Name="Song", Namespace="http://schemas.datacontract.org/2004/07/KServer")]
    [System.SerializableAttribute()]
    public partial class Song : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int IDField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string artistField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string pathOnDiskField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string titleField;
        
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
        public int ID {
            get {
                return this.IDField;
            }
            set {
                if ((this.IDField.Equals(value) != true)) {
                    this.IDField = value;
                    this.RaisePropertyChanged("ID");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string artist {
            get {
                return this.artistField;
            }
            set {
                if ((object.ReferenceEquals(this.artistField, value) != true)) {
                    this.artistField = value;
                    this.RaisePropertyChanged("artist");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string pathOnDisk {
            get {
                return this.pathOnDiskField;
            }
            set {
                if ((object.ReferenceEquals(this.pathOnDiskField, value) != true)) {
                    this.pathOnDiskField = value;
                    this.RaisePropertyChanged("pathOnDisk");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string title {
            get {
                return this.titleField;
            }
            set {
                if ((object.ReferenceEquals(this.titleField, value) != true)) {
                    this.titleField = value;
                    this.RaisePropertyChanged("title");
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
    [System.Runtime.Serialization.DataContractAttribute(Name="SongRequest", Namespace="http://schemas.datacontract.org/2004/07/KServer")]
    [System.SerializableAttribute()]
    public partial class SongRequest : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private int songIDField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private DJTestClient.ServiceReference1.User userField;
        
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
        public int songID {
            get {
                return this.songIDField;
            }
            set {
                if ((this.songIDField.Equals(value) != true)) {
                    this.songIDField = value;
                    this.RaisePropertyChanged("songID");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public DJTestClient.ServiceReference1.User user {
            get {
                return this.userField;
            }
            set {
                if ((object.ReferenceEquals(this.userField, value) != true)) {
                    this.userField = value;
                    this.RaisePropertyChanged("user");
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
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ServiceReference1.IDJ")]
    public interface IDJ {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDJ/DJSignUp", ReplyAction="http://tempuri.org/IDJ/DJSignUpResponse")]
        DJTestClient.ServiceReference1.Response DJSignUp(string userName, string password);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDJ/DJSignUp", ReplyAction="http://tempuri.org/IDJ/DJSignUpResponse")]
        System.Threading.Tasks.Task<DJTestClient.ServiceReference1.Response> DJSignUpAsync(string userName, string password);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDJ/DJSignIn", ReplyAction="http://tempuri.org/IDJ/DJSignInResponse")]
        DJTestClient.ServiceReference1.LogInResponse DJSignIn(string userName, string password);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDJ/DJSignIn", ReplyAction="http://tempuri.org/IDJ/DJSignInResponse")]
        System.Threading.Tasks.Task<DJTestClient.ServiceReference1.LogInResponse> DJSignInAsync(string userName, string password);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDJ/DJSignOut", ReplyAction="http://tempuri.org/IDJ/DJSignOutResponse")]
        DJTestClient.ServiceReference1.Response DJSignOut(long DJKey);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDJ/DJSignOut", ReplyAction="http://tempuri.org/IDJ/DJSignOutResponse")]
        System.Threading.Tasks.Task<DJTestClient.ServiceReference1.Response> DJSignOutAsync(long DJKey);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDJ/DJCreateSession", ReplyAction="http://tempuri.org/IDJ/DJCreateSessionResponse")]
        DJTestClient.ServiceReference1.Session DJCreateSession(long DJKey);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDJ/DJCreateSession", ReplyAction="http://tempuri.org/IDJ/DJCreateSessionResponse")]
        System.Threading.Tasks.Task<DJTestClient.ServiceReference1.Session> DJCreateSessionAsync(long DJKey);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDJ/DJAddSongs", ReplyAction="http://tempuri.org/IDJ/DJAddSongsResponse")]
        DJTestClient.ServiceReference1.Response DJAddSongs(DJTestClient.ServiceReference1.Song[] songs, long DJKey);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDJ/DJAddSongs", ReplyAction="http://tempuri.org/IDJ/DJAddSongsResponse")]
        System.Threading.Tasks.Task<DJTestClient.ServiceReference1.Response> DJAddSongsAsync(DJTestClient.ServiceReference1.Song[] songs, long DJKey);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDJ/DJRemoveSongs", ReplyAction="http://tempuri.org/IDJ/DJRemoveSongsResponse")]
        DJTestClient.ServiceReference1.Response DJRemoveSongs(DJTestClient.ServiceReference1.Song[] songs, long DJKey);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDJ/DJRemoveSongs", ReplyAction="http://tempuri.org/IDJ/DJRemoveSongsResponse")]
        System.Threading.Tasks.Task<DJTestClient.ServiceReference1.Response> DJRemoveSongsAsync(DJTestClient.ServiceReference1.Song[] songs, long DJKey);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDJ/DJListSongs", ReplyAction="http://tempuri.org/IDJ/DJListSongsResponse")]
        DJTestClient.ServiceReference1.DJListSongsResponse DJListSongs(DJTestClient.ServiceReference1.DJListSongsRequest request);
        
        // CODEGEN: Generating message contract since the operation has multiple return values.
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDJ/DJListSongs", ReplyAction="http://tempuri.org/IDJ/DJListSongsResponse")]
        System.Threading.Tasks.Task<DJTestClient.ServiceReference1.DJListSongsResponse> DJListSongsAsync(DJTestClient.ServiceReference1.DJListSongsRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDJ/DJAddQueue", ReplyAction="http://tempuri.org/IDJ/DJAddQueueResponse")]
        DJTestClient.ServiceReference1.Response DJAddQueue(DJTestClient.ServiceReference1.SongRequest sr, int queueIndex, int sessionID, long DJKey);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDJ/DJAddQueue", ReplyAction="http://tempuri.org/IDJ/DJAddQueueResponse")]
        System.Threading.Tasks.Task<DJTestClient.ServiceReference1.Response> DJAddQueueAsync(DJTestClient.ServiceReference1.SongRequest sr, int queueIndex, int sessionID, long DJKey);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDJ/DJRemoveSongRequest", ReplyAction="http://tempuri.org/IDJ/DJRemoveSongRequestResponse")]
        DJTestClient.ServiceReference1.Response DJRemoveSongRequest(DJTestClient.ServiceReference1.SongRequest sr, int sessionID, long DJKey);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDJ/DJRemoveSongRequest", ReplyAction="http://tempuri.org/IDJ/DJRemoveSongRequestResponse")]
        System.Threading.Tasks.Task<DJTestClient.ServiceReference1.Response> DJRemoveSongRequestAsync(DJTestClient.ServiceReference1.SongRequest sr, int sessionID, long DJKey);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDJ/DJChangeSongRequest", ReplyAction="http://tempuri.org/IDJ/DJChangeSongRequestResponse")]
        DJTestClient.ServiceReference1.Response DJChangeSongRequest(DJTestClient.ServiceReference1.SongRequest newSR, DJTestClient.ServiceReference1.SongRequest oldSR, int sessionID, long DJKey);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDJ/DJChangeSongRequest", ReplyAction="http://tempuri.org/IDJ/DJChangeSongRequestResponse")]
        System.Threading.Tasks.Task<DJTestClient.ServiceReference1.Response> DJChangeSongRequestAsync(DJTestClient.ServiceReference1.SongRequest newSR, DJTestClient.ServiceReference1.SongRequest oldSR, int sessionID, long DJKey);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDJ/DJRemoveUser", ReplyAction="http://tempuri.org/IDJ/DJRemoveUserResponse")]
        DJTestClient.ServiceReference1.Response DJRemoveUser(DJTestClient.ServiceReference1.SongRequest newSR, DJTestClient.ServiceReference1.SongRequest oldSR, int sessionID, long DJKey);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDJ/DJRemoveUser", ReplyAction="http://tempuri.org/IDJ/DJRemoveUserResponse")]
        System.Threading.Tasks.Task<DJTestClient.ServiceReference1.Response> DJRemoveUserAsync(DJTestClient.ServiceReference1.SongRequest newSR, DJTestClient.ServiceReference1.SongRequest oldSR, int sessionID, long DJKey);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDJ/DJMoveUser", ReplyAction="http://tempuri.org/IDJ/DJMoveUserResponse")]
        DJTestClient.ServiceReference1.Response DJMoveUser(int userID, int sessionID, long DJKey);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IDJ/DJMoveUser", ReplyAction="http://tempuri.org/IDJ/DJMoveUserResponse")]
        System.Threading.Tasks.Task<DJTestClient.ServiceReference1.Response> DJMoveUserAsync(int userID, int sessionID, long DJKey);
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(WrapperName="DJListSongs", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class DJListSongsRequest {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public long DJKey;
        
        public DJListSongsRequest() {
        }
        
        public DJListSongsRequest(long DJKey) {
            this.DJKey = DJKey;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.MessageContractAttribute(WrapperName="DJListSongsResponse", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class DJListSongsResponse {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public DJTestClient.ServiceReference1.Response DJListSongsResult;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=1)]
        public DJTestClient.ServiceReference1.Song[] songs;
        
        public DJListSongsResponse() {
        }
        
        public DJListSongsResponse(DJTestClient.ServiceReference1.Response DJListSongsResult, DJTestClient.ServiceReference1.Song[] songs) {
            this.DJListSongsResult = DJListSongsResult;
            this.songs = songs;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IDJChannel : DJTestClient.ServiceReference1.IDJ, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class DJClient : System.ServiceModel.ClientBase<DJTestClient.ServiceReference1.IDJ>, DJTestClient.ServiceReference1.IDJ {
        
        public DJClient() {
        }
        
        public DJClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public DJClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DJClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public DJClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public DJTestClient.ServiceReference1.Response DJSignUp(string userName, string password) {
            return base.Channel.DJSignUp(userName, password);
        }
        
        public System.Threading.Tasks.Task<DJTestClient.ServiceReference1.Response> DJSignUpAsync(string userName, string password) {
            return base.Channel.DJSignUpAsync(userName, password);
        }
        
        public DJTestClient.ServiceReference1.LogInResponse DJSignIn(string userName, string password) {
            return base.Channel.DJSignIn(userName, password);
        }
        
        public System.Threading.Tasks.Task<DJTestClient.ServiceReference1.LogInResponse> DJSignInAsync(string userName, string password) {
            return base.Channel.DJSignInAsync(userName, password);
        }
        
        public DJTestClient.ServiceReference1.Response DJSignOut(long DJKey) {
            return base.Channel.DJSignOut(DJKey);
        }
        
        public System.Threading.Tasks.Task<DJTestClient.ServiceReference1.Response> DJSignOutAsync(long DJKey) {
            return base.Channel.DJSignOutAsync(DJKey);
        }
        
        public DJTestClient.ServiceReference1.Session DJCreateSession(long DJKey) {
            return base.Channel.DJCreateSession(DJKey);
        }
        
        public System.Threading.Tasks.Task<DJTestClient.ServiceReference1.Session> DJCreateSessionAsync(long DJKey) {
            return base.Channel.DJCreateSessionAsync(DJKey);
        }
        
        public DJTestClient.ServiceReference1.Response DJAddSongs(DJTestClient.ServiceReference1.Song[] songs, long DJKey) {
            return base.Channel.DJAddSongs(songs, DJKey);
        }
        
        public System.Threading.Tasks.Task<DJTestClient.ServiceReference1.Response> DJAddSongsAsync(DJTestClient.ServiceReference1.Song[] songs, long DJKey) {
            return base.Channel.DJAddSongsAsync(songs, DJKey);
        }
        
        public DJTestClient.ServiceReference1.Response DJRemoveSongs(DJTestClient.ServiceReference1.Song[] songs, long DJKey) {
            return base.Channel.DJRemoveSongs(songs, DJKey);
        }
        
        public System.Threading.Tasks.Task<DJTestClient.ServiceReference1.Response> DJRemoveSongsAsync(DJTestClient.ServiceReference1.Song[] songs, long DJKey) {
            return base.Channel.DJRemoveSongsAsync(songs, DJKey);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        DJTestClient.ServiceReference1.DJListSongsResponse DJTestClient.ServiceReference1.IDJ.DJListSongs(DJTestClient.ServiceReference1.DJListSongsRequest request) {
            return base.Channel.DJListSongs(request);
        }
        
        public DJTestClient.ServiceReference1.Response DJListSongs(long DJKey, out DJTestClient.ServiceReference1.Song[] songs) {
            DJTestClient.ServiceReference1.DJListSongsRequest inValue = new DJTestClient.ServiceReference1.DJListSongsRequest();
            inValue.DJKey = DJKey;
            DJTestClient.ServiceReference1.DJListSongsResponse retVal = ((DJTestClient.ServiceReference1.IDJ)(this)).DJListSongs(inValue);
            songs = retVal.songs;
            return retVal.DJListSongsResult;
        }
        
        public System.Threading.Tasks.Task<DJTestClient.ServiceReference1.DJListSongsResponse> DJListSongsAsync(DJTestClient.ServiceReference1.DJListSongsRequest request) {
            return base.Channel.DJListSongsAsync(request);
        }
        
        public DJTestClient.ServiceReference1.Response DJAddQueue(DJTestClient.ServiceReference1.SongRequest sr, int queueIndex, int sessionID, long DJKey) {
            return base.Channel.DJAddQueue(sr, queueIndex, sessionID, DJKey);
        }
        
        public System.Threading.Tasks.Task<DJTestClient.ServiceReference1.Response> DJAddQueueAsync(DJTestClient.ServiceReference1.SongRequest sr, int queueIndex, int sessionID, long DJKey) {
            return base.Channel.DJAddQueueAsync(sr, queueIndex, sessionID, DJKey);
        }
        
        public DJTestClient.ServiceReference1.Response DJRemoveSongRequest(DJTestClient.ServiceReference1.SongRequest sr, int sessionID, long DJKey) {
            return base.Channel.DJRemoveSongRequest(sr, sessionID, DJKey);
        }
        
        public System.Threading.Tasks.Task<DJTestClient.ServiceReference1.Response> DJRemoveSongRequestAsync(DJTestClient.ServiceReference1.SongRequest sr, int sessionID, long DJKey) {
            return base.Channel.DJRemoveSongRequestAsync(sr, sessionID, DJKey);
        }
        
        public DJTestClient.ServiceReference1.Response DJChangeSongRequest(DJTestClient.ServiceReference1.SongRequest newSR, DJTestClient.ServiceReference1.SongRequest oldSR, int sessionID, long DJKey) {
            return base.Channel.DJChangeSongRequest(newSR, oldSR, sessionID, DJKey);
        }
        
        public System.Threading.Tasks.Task<DJTestClient.ServiceReference1.Response> DJChangeSongRequestAsync(DJTestClient.ServiceReference1.SongRequest newSR, DJTestClient.ServiceReference1.SongRequest oldSR, int sessionID, long DJKey) {
            return base.Channel.DJChangeSongRequestAsync(newSR, oldSR, sessionID, DJKey);
        }
        
        public DJTestClient.ServiceReference1.Response DJRemoveUser(DJTestClient.ServiceReference1.SongRequest newSR, DJTestClient.ServiceReference1.SongRequest oldSR, int sessionID, long DJKey) {
            return base.Channel.DJRemoveUser(newSR, oldSR, sessionID, DJKey);
        }
        
        public System.Threading.Tasks.Task<DJTestClient.ServiceReference1.Response> DJRemoveUserAsync(DJTestClient.ServiceReference1.SongRequest newSR, DJTestClient.ServiceReference1.SongRequest oldSR, int sessionID, long DJKey) {
            return base.Channel.DJRemoveUserAsync(newSR, oldSR, sessionID, DJKey);
        }
        
        public DJTestClient.ServiceReference1.Response DJMoveUser(int userID, int sessionID, long DJKey) {
            return base.Channel.DJMoveUser(userID, sessionID, DJKey);
        }
        
        public System.Threading.Tasks.Task<DJTestClient.ServiceReference1.Response> DJMoveUserAsync(int userID, int sessionID, long DJKey) {
            return base.Channel.DJMoveUserAsync(userID, sessionID, DJKey);
        }
    }
}
