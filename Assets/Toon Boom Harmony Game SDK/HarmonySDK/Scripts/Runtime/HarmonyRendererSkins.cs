using System;

namespace ToonBoom.Harmony
{
    public partial class HarmonyRenderer
    {
        public GroupSkinList GroupSkins;
        private uint[] _skins = new uint[0];

        /// <summary>
        /// Update nodes skins values modified by the Group Skins
        /// </summary>
        public bool UpdateSkins()
        {
            bool changed = false;

            // make sure Skins length is the same as the number of node
            if (_skins.Length != Project.Nodes.Count)
            {
                Array.Resize(ref _skins, Project.Nodes.Count);
                changed = true;
            }

            for (int i = 0; i < _skins.Length; i++)
            {
                uint skinValue = 0;
                HarmonyNode currentNode = Project.Nodes[i];

                if (currentNode != null && currentNode.SkinIds != null)
                {
                    // group skins applied in order. Last one always overrides
                    for (int j = GroupSkins.Count - 1; j >= 0; j--)
                    {
                        int groupId = GroupSkins[j].GroupId;
                        if (groupId == 0 || currentNode.GroupId == groupId)
                        {
                            int skinId = GroupSkins[j].SkinId;
                            for (int k = 0; k < currentNode.SkinIds.Length; k++)
                            {
                                if (currentNode.SkinIds[k] == skinId)
                                {
                                    skinValue = (uint)skinId;
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }

                changed = changed || _skins[i] != skinValue;
                _skins[i] = skinValue;
            }

            return changed;
        }
    }
}
