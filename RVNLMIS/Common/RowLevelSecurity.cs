using RVNLMIS.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVNLMIS.Common
{
    public static class RowLevelSecurity
    {
        public static Hashtable getUsernameAndRole(string tableDataName, string roleCode)
        {
            Hashtable values = new Hashtable();
            try
            {
                string a = string.Empty;
                string Role = "";
                string Username = "";
                string code = "";
                string name = "";

                if (!string.IsNullOrEmpty(tableDataName))
                {
                    string[] CodeName = tableDataName.Split('~');
                    code = CodeName[0];
                    name = CodeName[1];
                }
                else
                {
                    name = "Super User";
                }

                Role = roleCode == "CUS" ? "SUS" : roleCode;

                if (Role == "SAD" || Role == "SUS")
                {
                    Username = name;
                }
                else if (Role == "CPM" || Role == "PRJ" || Role == "EDM" || Role == "PKG" || Role == "DISP")
                {
                    Username = code.Trim();
                }
                values.Add("Username", Username);
                values.Add("Role", Role);
            }
            catch (Exception e)
            {

            }
            return values;
        }
    }
}