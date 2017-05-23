﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// Represents a user that is being edited
    /// </summary>
    [DataContract(Name = "user", Namespace = "")]
    [ReadOnly(true)]
    public class UserDisplay : EntityBasic, INotificationModel
    {
        public UserDisplay()
        {
            Notifications = new List<Notification>();
        }

        /// <summary>
        /// The MD5 lowercase hash of the email which can be used by gravatar
        /// </summary>
        [DataMember(Name = "emailHash")]
        public string EmailHash { get; set; }

        [DataMember(Name = "lastLoginDate")]
        public DateTime LastLoginDate { get; set; }

        [DataMember(Name = "customAvatar")]
        public string CustomAvatar { get; set; }

        [DataMember(Name = "userState")]
        public UserState UserState { get; set; }

        [DataMember(Name = "culture", IsRequired = true)]
        public string Culture { get; set; }

        [DataMember(Name = "email", IsRequired = true)]
        public string Email { get; set; }

        /// <summary>
        /// The list of group aliases assigned to the user
        /// </summary>
        [DataMember(Name = "userGroups")]
        public IEnumerable<string> UserGroups { get; set; }

        /// <summary>
        /// Gets the available user groups (i.e. to populate a drop down)
        /// The key is the Alias the value is the Name - the Alias is what is used in the UserGroup property and for persistence
        /// </summary>
        [DataMember(Name = "availableUserGroups")]
        public IDictionary<string, string> AvailableUserGroups { get; set; }

        /// <summary>
        /// Gets the available cultures (i.e. to populate a drop down)
        /// The key is the culture stored in the database, the value is the Name
        /// </summary>
        [DataMember(Name = "availableCultures")]
        public IDictionary<string, string> AvailableCultures { get; set; }

        //TODO: This will become StartContentIds as an array!
        [DataMember(Name = "startContentId")]
        public int StartContentId { get; set; }

        //TODO: This will become StartMediaIds as an array!
        [DataMember(Name = "startMediaId")]
        public int StartMediaId { get; set; }

        /// <summary>
        /// A list of sections the user is allowed to view.
        /// </summary>
        [DataMember(Name = "allowedSections")]
        public IEnumerable<string> AllowedSections { get; set; }

        /// <summary>
        /// Gets the available sections (i.e. to populate a drop down)
        /// The key is the Alias the value is the Name - the Alias is what is used in the AllowedSections property and for persistence
        /// </summary>
        [DataMember(Name = "availableSections")]
        public IDictionary<string, string> AvailableSections { get; set; }

        /// <summary>
        /// This is used to add custom localized messages/strings to the response for the app to use for localized UI purposes.
        /// </summary>
        [DataMember(Name = "notifications")]
        public List<Notification> Notifications { get; private set; }
    }
}