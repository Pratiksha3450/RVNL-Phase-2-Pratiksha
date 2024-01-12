using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVNLMIS.Models
{
    public class RBACUser
    {
        public int User_Id { get; set; }
        public bool IsSysAdmin { get; set; }
        public string Username { get; set; }
        private List<UserRole> Roles = new List<UserRole>();

        public RBACUser(string _username)
        {
            this.Username = _username;
            this.IsSysAdmin = false;
            GetDatabaseUserRolesPermissions();
        }

        private void GetDatabaseUserRolesPermissions()
        {
            //Get user roles and permissions from database tables...      
        }

        public bool HasPermission(string requiredPermission)
        {
            bool bFound = false;
            foreach (UserRole role in this.Roles)
            {
                bFound = (role.Permissions.Where(
                          p => p.PermissionDescription == requiredPermission).ToList().Count > 0);
                if (bFound)
                    break;
            }
            return bFound;
        }

        public bool HasRole(string role)
        {
            return (Roles.Where(p => p.RoleName == role).ToList().Count > 0);
        }

        public bool HasRoles(string roles)
        {
            bool bFound = false;
            string[] _roles = roles.ToLower().Split(';');
            foreach (UserRole role in this.Roles)
            {
                try
                {
                    bFound = _roles.Contains(role.RoleName.ToLower());
                    if (bFound)
                        return bFound;
                }
                catch (Exception)
                {
                }
            }
            return bFound;
        }


        public class UserRole
        {
            public int Role_Id { get; set; }
            public string RoleName { get; set; }
            public List<RolePermission> Permissions = new List<RolePermission>();
        }

        public class RolePermission
        {
            public int Permission_Id { get; set; }
            public string PermissionDescription { get; set; }
        }
    }
}