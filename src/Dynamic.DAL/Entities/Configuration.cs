using System;

namespace Dynamic.DAL.Entities
{
    public class Configuration : IIdentifiable
    {
        int IIdentifiable.Id => ConfigurationId;
        public int ConfigurationId { get; set; }
        public Guid SuperUserGroup { get; set; }
        public string Language { get; set; }
        public Guid TeamsAdmin { get; set; }
        public Guid ArchiveDrive { get; set; }
        public bool AutentiEnabled { get; set; }
        public string AutentiApiKey { get; set; }
        public bool DocuSignEnabled { get; set; }
        public Guid? DocuSignClientId { get; set; }
        public Guid? ImpersonatedUserGuid { get; set; }
        public string DocuSignPrivateKey { get; set; }
    }
}
