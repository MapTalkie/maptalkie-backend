namespace MapTalkie.Hubs
{
    public static class MapTalkieGroups
    {
        public static string ConversationPrefix = "conv";
        public static string AreaUpdatesPrefix = "area";
        public static string PostUpdatesPrefix = "post";

        public static string Conversation(string userId1, string userId2)
        {
            if (string.CompareOrdinal(userId1, userId2) > 0)
                (userId1, userId2) = (userId2, userId1);
            return $"{userId1}_{userId2}";
        }
    }
}