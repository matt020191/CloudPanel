// <auto-generated />
namespace CloudPanel.Database.EntityFramework.Migrations
{
    using System.CodeDom.Compiler;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Resources;
    
    [GeneratedCode("EntityFramework.Migrations", "6.1.1-30610")]
    public sealed partial class AddUsernameAndIPToAuditTrace : IMigrationMetadata
    {
        private readonly ResourceManager Resources = new ResourceManager(typeof(AddUsernameAndIPToAuditTrace));
        
        string IMigrationMetadata.Id
        {
            get { return "201412142106439_AddUsernameAndIPToAuditTrace"; }
        }
        
        string IMigrationMetadata.Source
        {
            get { return null; }
        }
        
        string IMigrationMetadata.Target
        {
            get { return Resources.GetString("Target"); }
        }
    }
}
