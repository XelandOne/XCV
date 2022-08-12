using System;
using System.Collections.Generic;

namespace XCV.Entities.Enums
{
    /// <summary>
    /// Authorization of employee for accessing pages.
    /// </summary>
    public enum Authorizations
    {
        /// <summary>
        /// Normal employee with no special access rights.
        /// </summary>
        Pleb = 1,
        /// <summary>
        /// Sales employee with access rights regarding offer and project management.
        /// </summary>
        Sales = 2,
        /// <summary>
        /// Admin employee with access rights regarding experience database management.
        /// </summary>
        Admin = 3,
        /// <summary>
        /// Employee with <see cref="Sales"/> and <see cref="Admin"/> rights.
        /// <seealso cref="Sales"/>
        /// <seealso cref="Admin"/>
        /// </summary>
        SalesAdmin = 4
    }
    
    /// <summary>
    /// Class to translate the Authorizations for the UI 
    /// </summary>
    public static class AuthorizationHelper
    {
        private static readonly Dictionary<Authorizations, string> FriendlyNamesGerman = new Dictionary<Authorizations, string>
        {
            {Authorizations.Pleb, "Mitarbeiter"},
            {Authorizations.Sales, "Vertriebsmitarbeiter"},
            {Authorizations.Admin, "Administrator"},
            {Authorizations.SalesAdmin, "Administrator und Vertriebsmitarbeiter"},
        };
        
        public static string ToFriendlyString(Authorizations authorizations)
        {
            return FriendlyNamesGerman[authorizations];
        }
    }
    
    /// <summary>
    /// Class to combine all possible combination Authorizations of an employee,
    /// to manage page restrictions
    /// </summary>
    public static class AuthorizationStringBuilder
    {
        /// <summary>
        /// Returns string with sale roles.
        /// </summary>
        /// <returns></returns>
        public static string GetSalesRolesString()
        {
            return Authorizations.Sales + ", " + Authorizations.SalesAdmin;
        }
        
        /// <summary>
        /// Returns string with admin roles.
        /// </summary>
        /// <returns></returns>
        public static string GetAdminRolesString()
        {
            return Authorizations.Admin + ", " + Authorizations.SalesAdmin;
        }
        
        /// <summary>
        /// Returns string with admin or sales roles.
        /// </summary>
        /// <returns></returns>
        public static string GetSalesAndAdminRolesString()
        {
            return Authorizations.Admin + ", " + Authorizations.SalesAdmin + ", " + Authorizations.SalesAdmin;
        }
    }
}