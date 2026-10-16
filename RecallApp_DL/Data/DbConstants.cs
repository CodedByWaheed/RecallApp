using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecallApp_DL.Data
{
    public sealed class DbConstants
    {
        public const int CommandTimeout = 30;
        
        public const string CreateEncryptedContent = "sp_EncryptedContents_Create";
        public const string GetEncryptedContentsBySavedItemId = "sp_EncryptedContents_GetBySavedItemId";
        public const string UpdateEncryptedContent = "sp_EncryptedContent_Update";
        //----------------------Refresh Token --------------------------
        public const string CreateRefreshToken = "sp_RefreshTokens_Create";
        public const string GetRefreshTokenByTokenHash = "sp_RefreshTokens_GetByTokenHash";
        public const string RotateRefreshToken = "sp_RefreshTokens_Rotate";
        public const string RevokeRefreshToken = "sp_RefreshTokens_Revoke";
        //----------------------------- SavedItems -----------------------
        public const string GetSavedItemByID = "sp_SavedItems_GetById";
        public const string CreateSavedItem = "sp_SavedItems_Create";
        public const string GetSavedItemsByUserId = "sp_SavedItems_GetByUserId";
        public const string UpdateSavedItem = "sp_SavedItems_Update";
        public const string ArchiveSavedItem = "sp_SavedItems_Archive";
        public const string RestoreSavedItem = "sp_SavedItems_Restore";
        public const string GetArchivedSavedItemsByUserId = "sp_SavedItems_GetArchivedByUserId";
        public const string DeleteSavedItem = "sp_SavedItems_Delete";
        //----------------------- TAGS ----------------------------------
        public const string CreateTag = "sp_Tags_Create";
        public const string GetTagsByUserId = "sp_Tags_GetByUserId";
        public const string UpdateTag = "sp_Tags_Update";
        public const string DeleteTag = "sp_Tags_Delete";

        public const string AssignTagToSavedItem = "sp_SavedItemTags_Assign";
        public const string RemoveTagFromSavedItem = "sp_SavedItemTags_Remove";
        public const string GetTagsBySavedItemId = "sp_SavedItemTags_GetBySavedItemId";
        public const string GetSavedItemsByTagId = "sp_SavedItems_GetByTagId";

        //----------------------- REMINDERS ----------------------------------
        public const string CreateReminder = "sp_Reminders_Create";
        public const string GetPendingRemindersByUserId = "sp_Reminders_GetPendingByUserId";
        public const string GetRemindersBySavedItemId = "sp_Reminders_GetBySavedItemId";
        public const string UpdateReminder = "sp_Reminders_Update";
        public const string SetReminderCompletion = "sp_Reminders_SetCompletion";
        public const string DeleteReminder = "sp_Reminders_Delete";
        //----------------------- Users ----------------------------------
        public const string GetUserByID = "sp_Users_GetById";
        public const string GetUserByEmail = "sp_Users_GetByEmail";
        public const string UpdateUserEmail = "sp_Users_UpdateEmail";
        public const string UpdateUserPasswordHash = "sp_Users_UpdatePasswordHash";
        public const string CreateUser = "sp_Users_Create";
        public const string DeleteUser = "sp_Users_Delete";

    }
}
