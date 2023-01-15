using System;

namespace ToonBoom.Harmony
{
    /// <summary>
    /// Data used to select group/skin pair in Unity interface
    /// </summary>
    [Serializable]
    public struct GroupSkin
    {
        public int GroupId;
        public int SkinId;

        public GroupSkin(int groupId, int skinId)
        {
            GroupId = groupId;
            SkinId = skinId;
        }
    }
}
