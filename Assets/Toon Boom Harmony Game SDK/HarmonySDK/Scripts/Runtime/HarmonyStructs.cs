using System;
using System.Collections.Generic;
using UnityEngine;

namespace ToonBoom.Harmony
{

    /// <summary>
    /// Id name data
    /// </summary>
    [Serializable]
    public struct HarmonyId
    {
        public int Id;
        public string Name;

        public HarmonyId(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    /// <summary>
    /// Drawing Node containing possible skins and its group
    /// </summary>
    [Serializable]
    public class HarmonyNode
    {
        private static readonly int[] EmptySkins = new int[0];

        public int Id;
        public string Name;
        public int[] SkinIds;
        public int GroupId;

        public HarmonyNode()
        {
            Id = 0;
            Name = String.Empty;
            SkinIds = EmptySkins;
            GroupId = 0;
        }

        public HarmonyNode(int id, string name, int[] skinIds, int groupId)
        {
            Id = id;
            Name = name;
            SkinIds = skinIds;
            GroupId = groupId;
        }
    }

    [Serializable]
    public struct PropMeta
    {
        public string ClipName;
        public string PlayName;

        public PropMeta(string clipName, string playName)
        {
            ClipName = clipName;
            PlayName = playName;
        }
    }

    [Serializable]
    public struct AnchorMeta
    {
        public string PlayName;
        public string NodeName;

        public AnchorMeta(string playName, string nodeName)
        {
            PlayName = playName;
            NodeName = nodeName;
        }
    }

    [Serializable]
    public struct GenericMeta
    {
        public string Name;
        public string ClipName;
        public string PlayName;
        public string NodeName;
        public string Value;

        public GenericMeta(string name, string clipName, string playName, string nodeName, string value)
        {
            Name = name;
            ClipName = clipName;
            PlayName = playName;
            NodeName = nodeName;
            Value = value;
        }
    }

    [Serializable]
    public struct CustomSpriteData
    {
        public string Name;
        public double[] Matrix;

        public CustomSpriteData(string name, double[] matrix)
        {
            Name = name;
            Matrix = matrix;
        }
    }

    [Serializable]
    public struct ClipData
    {
        public string DisplayName;
        public string FullName;
        public string Name;
        public float FrameCount;
        public bool IsProp;

        public ClipData(string displayName, string fullName, string name, float frameCount, bool isProp)
        {
            DisplayName = displayName;
            FullName = fullName;
            Name = name;
            FrameCount = frameCount;
            IsProp = isProp;
        }
    }

    [Serializable]
    public struct Spritesheet : IEquatable<Spritesheet>
    {
        public string SheetName;
        public string ResolutionName;
        public List<Sprite> Sprites;

        public Spritesheet(string sheetName, string resolutionName)
        {
            SheetName = sheetName;
            ResolutionName = resolutionName;
            Sprites = new List<Sprite>();
        }

        public bool HasValidSprites()
        {
            if(Sprites == null || Sprites.Count == 0)
            {
                return false;
            }
            for(int i = 0, len = Sprites.Count; i < len; i++)
            {
                if(Sprites[i] == null)
                {
                    return false;
                }
            }
            return true;
        }

        // NOTE: Used to prevent duplicate sprite sheets with List.Contains()
        public bool Equals(Spritesheet other)
        {
            return ResolutionName == other.ResolutionName && SheetName == other.SheetName;
        }

        public override bool Equals(object obj)
        {
            return obj is Spritesheet other && Equals(other);
        }

        public override int GetHashCode()
        {
            int hashCode = 1525831759;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SheetName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ResolutionName);
            return hashCode;
        }
    }
}