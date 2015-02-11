namespace CloudPanel.Base.Models.Database
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class ApiKeys
    {
        [Key, ForeignKey("User")]
        public int UserID { get; set; }

        public string Key { get; set; }

        public virtual Users User { get; set; }
    }
}
