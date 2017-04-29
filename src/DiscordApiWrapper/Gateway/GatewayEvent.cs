namespace DiscordApiWrapper.Gateway
{
    public enum GatewayEvent
    {
        Ready,
        Resumed,
        Channel_Create,
        Channel_Update,
        Channel_Delete,
        Guild_Create,
        Guild_Update,
        Guild_Delete,
        Guild_Ban_Add,
        Guild_Ban_Remove,
        Guild_Emojis_Update,
        Guild_Integrations_Update,
        Guild_Member_Add,
        Guild_Member_Remove,
        Guild_Member_Update,
        Guild_Members_Chunk,
        Guild_Role_Create,
        Guild_Role_Update,
        Guild_Role_Delete,
        Message_Create,
        Message_Update,
        Message_Delete,
        Message_Delete_Bulk,
        Presence_Update,
        Typing_Start,
        User_Update,
        Voice_State_Update,
        Voice_Server_Update,
    }
}