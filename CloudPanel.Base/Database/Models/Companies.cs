//
// Copyright (c) 2014, Jacob Dixon
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. All advertising materials mentioning features or use of this software
//    must display the following acknowledgement:
//    This product includes software developed by KnowMoreIT and Compsys.
// 4. Neither the name of KnowMoreIT and Compsys nor the
//    names of its contributors may be used to endorse or promote products
//    derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY Jacob Dixon ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL Jacob Dixon BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
namespace CloudPanel.Base.Database.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Companies
    {
        [Key]
        public int CompanyId { get; set; }

        public bool IsReseller { get; set; }

        [StringLength(255)]
        public string ResellerCode { get; set; }

        public int? OrgPlanID { get; set; }

        [Required]
        [StringLength(100)]
        public string CompanyName { get; set; }

        [Required]
        [StringLength(255)]
        public string CompanyCode { get; set; }

        [Required]
        [StringLength(255)]
        public string Street { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; }

        [Required]
        [StringLength(100)]
        public string State { get; set; }

        [Required]
        [StringLength(50)]
        public string ZipCode { get; set; }

        [Required]
        [StringLength(50)]
        public string PhoneNumber { get; set; }

        [StringLength(255)]
        public string Website { get; set; }

        [Column(TypeName = "ntext")]
        public string Description { get; set; }

        [Required]
        [StringLength(100)]
        public string AdminName { get; set; }

        [Required]
        [StringLength(255)]
        public string AdminEmail { get; set; }

        [Required]
        [StringLength(255)]
        public string DistinguishedName { get; set; }

        public DateTime Created { get; set; }

        public bool ExchEnabled { get; set; }

        public bool? LyncEnabled { get; set; }

        public bool? CitrixEnabled { get; set; }

        public int? ExchPFPlan { get; set; }

        [StringLength(50)]
        public string Country { get; set; }

        public bool? ExchPermFixed { get; set; }

        public ICollection<CitrixDesktopGroups> CitrixDesktopGroups { get; set; }

        #region Not Mapped

        [NotMapped]
        public string FullAddressFormatted
        {
            get
            {
                return string.Format("{0}<br/>{1} {2}  {3}", Street, City, State, ZipCode);
            }

        }

        [NotMapped]
        public int TotalUsers { get; set; }

        #endregion
    }
}
