﻿using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class Permission
    {
        public Permission()
        {
            Actions = new HashSet<Action>();
            RolePermissions = new HashSet<RolePermission>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int? SortOrder { get; set; }
        public int Status { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }

        public virtual ICollection<Action> Actions { get; set; }
        public virtual ICollection<RolePermission> RolePermissions { get; set; }
    }
}
