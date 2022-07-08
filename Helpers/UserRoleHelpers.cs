using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WandSyncFile.Constants;

namespace WandSyncFile.Helpers
{
    public static class UserRoleHelpers
    {
        public static bool IsAdmin()
        {
            var roles = Properties.Settings.Default.Roles;
            if (string.IsNullOrEmpty(roles))
            {
                return false;
            }

            var userRoles = JsonConvert.DeserializeObject<List<string>>(roles);
            if (userRoles.Any(u => u == Options.ROLE_ADMIN))
            {
                return true;
            }

            return false;
        }

        public static bool IsSales()
        {
            var roles = Properties.Settings.Default.Roles;
            if (string.IsNullOrEmpty(roles))
            {
                return false;
            }

            var userRoles = JsonConvert.DeserializeObject<List<string>>(roles);
            if (userRoles.Any(u => u == Options.ROLE_SALES))
            {
                return true;
            }

            return false;
        }

        public static bool IsEditors()
        {
            var roles = Properties.Settings.Default.Roles;
            if (string.IsNullOrEmpty(roles))
            {
                return false;
            }

            var userRoles = JsonConvert.DeserializeObject<List<string>>(roles);
            if (userRoles.Any(u => u == Options.ROLE_EDITOR))
            {
                return true;
            }

            return false;
        }

        public static bool IsMarketing()
        {
            var roles = Properties.Settings.Default.Roles;
            if (string.IsNullOrEmpty(roles))
            {
                return false;
            }

            var userRoles = JsonConvert.DeserializeObject<List<string>>(roles);
            if (userRoles.Any(u => u == Options.ROLE_MARKETING))
            {
                return true;
            }

            return false;
        }

        public static bool IsCustomerSupport()
        {
            var roles = Properties.Settings.Default.Roles;
            if (string.IsNullOrEmpty(roles))
            {
                return false;
            }

            var userRoles = JsonConvert.DeserializeObject<List<string>>(roles);
            if (userRoles.Any(u => u == Options.ROLE_CUSTOMER_SUPPORT))
            {
                return true;
            }

            return false;
        }

        public static bool IsLeader()
        {
            var roles = Properties.Settings.Default.Roles;
            if (string.IsNullOrEmpty(roles))
            {
                return false;
            }

            var userRoles = JsonConvert.DeserializeObject<List<string>>(roles);
            if (userRoles.Any(u => u == Options.ROLE_LEADER))
            {
                return true;
            }

            return false;
        }

        public static bool IsSubLeader()
        {
            var roles = Properties.Settings.Default.Roles;
            if (string.IsNullOrEmpty(roles))
            {
                return false;
            }

            var userRoles = JsonConvert.DeserializeObject<List<string>>(roles);
            if (userRoles.Any(u => u == Options.ROLE_SUBLEADER))
            {
                return true;
            }

            return false;
        }
    }
}
