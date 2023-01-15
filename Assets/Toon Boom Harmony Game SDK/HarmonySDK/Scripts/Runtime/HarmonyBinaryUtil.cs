using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace ToonBoom.Harmony
{
    public static class HarmonyBinaryUtil
    {
        private const int _ExpectedMagicbytes = 0x66626274;
        private const int _ExpectedMajorVersion = 2;
        private const int _ExpectedMinorVersion = 0;

        public static void FillProjectFromBinary(HarmonyProject project, byte[] binaryFile)
        {
            HarmonyInternal.Header.FromBinary(binaryFile, out var header);

            if(header.Magicbyte != _ExpectedMagicbytes)
            {
                throw new InvalidDataException("File '" + project.name + "' is not a valid harmony binary project.");
            }
            if(header.MajorVersion != _ExpectedMajorVersion)
            {
                throw new InvalidDataException("The version of binary project '" + project.name + "' is '" + header.MajorVersion + "', expecting '" + _ExpectedMajorVersion + "'.");
            }

            InitializeLists(header, project);

            using (MemoryStream stream = new MemoryStream(binaryFile))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                ExtractClipData(reader, header.Clipnames, project.Clips, project.Props);
                ExtractSheetNamesData(reader, header.Sheetnames, project.SheetResolutions);
                ExtractSpritesData(reader, header.CustomSprites, project.CustomSprites, project.SheetResolutions);
                ExtractNodeData(reader, header.Nodes, project.Nodes);
                ExtractSkinData(reader, header.Skins, project.Skins);
                ExtractGroupData(reader, header.Groups, project.Groups);
                ExtractPropMetaData(reader, header.Props, project.PropsMeta);
                ExtractAnchorMetaData(reader, header.Anchors, project.AnchorsMeta);
                ExtractGenericMetaData(reader, header.Metas, project.GenericMeta);
            }
        }

        private static void InitializeLists(in HarmonyInternal.Header header, HarmonyProject project)
        {
            project.Clips = new List<ClipData>();
            project.Props = new List<ClipData>();
            project.Nodes = new List<HarmonyNode>((int)header.Nodes.Count);
            project.Skins = new List<string>((int)header.Skins.Count);
            project.Groups = new List<string>((int)header.Groups.Count);
            project.AnchorsMeta = new List<AnchorMeta>((int)header.Anchors.Count);
            project.PropsMeta = new List<PropMeta>((int)header.Props.Count);
            project.GenericMeta = new List<GenericMeta>((int)header.Metas.Count);
            project.SheetResolutions = new List<Spritesheet>((int)header.Sheetnames.Count);
            project.CustomSprites = new List<CustomSpriteData>((int)header.CustomSprites.Count);
        }

        private static void ExtractClipData(BinaryReader reader, HarmonyInternal.Header.Entry entry, List<ClipData> clips, List<ClipData> props)
        {
            clips.Clear();
            props.Clear();
            reader.BaseStream.Position = entry.Offset;

            for (long i = 0; i < entry.Count; ++i)
            {
                string displayName = HarmonyUtils.ReadHarmonyString(reader);
                string fullName = HarmonyUtils.ReadHarmonyString(reader);
                string name = HarmonyUtils.ReadHarmonyString(reader);
                float frameCount = reader.ReadSingle();
                bool isProp = reader.ReadBoolean();
                ClipData clipData = new ClipData(displayName, fullName, name, frameCount, isProp);

                if (clipData.IsProp)
                {
                    props.Add(clipData);
                }
                else
                {
                    clips.Add(clipData);
                }
            }
        }

        private static void ExtractSheetNamesData(BinaryReader reader, HarmonyInternal.Header.Entry entry, List<Spritesheet> sheetnames)
        {
            if (entry.Count > 0)
            {
                sheetnames.Clear();
                reader.BaseStream.Position = entry.Offset;

                for (long i = 0; i < entry.Count; ++i)
                {
                    string sheetname = HarmonyUtils.ReadHarmonyString(reader);
                    string resolutionName = HarmonyUtils.ReadHarmonyString(reader);
                    Spritesheet spritesheetName = new Spritesheet(sheetname, resolutionName);
                    sheetnames.Add(spritesheetName);
                }
            }
        }

        private static void ExtractSpritesData(BinaryReader reader, HarmonyInternal.Header.Entry entry, List<CustomSpriteData> customSprites, List<Spritesheet> sheetnames)
        {
            customSprites.Clear();
            if (entry.Count > 0)
            {
                sheetnames.Clear();

                reader.BaseStream.Position = entry.Offset;

                for (long i = 0; i < entry.Count; ++i)
                {
                    string name = HarmonyUtils.ReadHarmonyString(reader);
                    double[] matrix = HarmonyUtils.GenerateIdentityMatrixCopy;
                    HarmonyUtils.ReadBinaryMatrix(reader, matrix);

                    customSprites.Add(new CustomSpriteData(name, matrix));

                    string[] strings = name.Split('/');
                    Spritesheet spritesheetName = new Spritesheet(strings[0], strings[1]);
                    if (!sheetnames.Contains(spritesheetName))
                    {
                        sheetnames.Add(spritesheetName);
                    }
                }
            }
        }

        private static void ExtractNodeData(BinaryReader reader, HarmonyInternal.Header.Entry entry, List<HarmonyNode> nodes)
        {
            nodes.Clear();

            reader.BaseStream.Position = entry.Offset;
            nodes.Add(new HarmonyNode());

            // Read the entries
            for (long i = 0; i < entry.Count; ++i)
            {
                HarmonyNode tempNode;
                {
                    string name = HarmonyUtils.ReadHarmonyString(reader);
                    int[] skinIds = new int[reader.ReadInt32()];
                    for (int j = 0; j < skinIds.Length; j++)
                    {
                        skinIds[j] = reader.ReadInt32();
                    }
                    int groupId = reader.ReadInt32();
                    int id = reader.ReadInt32();
                    tempNode = new HarmonyNode(id, name, skinIds, groupId);
                }

                while (nodes.Count <= tempNode.Id)
                {
                    // Fill up the list up to the Id.
                    nodes.Add(null);
                }

                HarmonyNode node = nodes[tempNode.Id];
                if (node == null)
                {
                    nodes[tempNode.Id] = tempNode;
                }
                else
                {
                    node.SkinIds = tempNode.SkinIds
                        .Union(node.SkinIds)
                        .Distinct()
                        .OrderBy(x => x)
                        .ToArray();
                }
            }
        }

        private static void ExtractSkinData(BinaryReader reader, HarmonyInternal.Header.Entry entry, List<string> skins)
        {
            skins.Clear();
            reader.BaseStream.Position = entry.Offset;

            List<HarmonyId> skinIds = new List<HarmonyId>() { new HarmonyId(0, "none") };
            int biggestId = 0;
            for (long i = 0; i < entry.Count; ++i)
            {
                HarmonyId hid;
                HarmonyUtils.ReadHarmonyId(reader, out hid);
                if (hid.Id > biggestId) biggestId = hid.Id;
                skinIds.Add(hid);
            }

            string[] skinsFlattened = new string[biggestId + 1];
            for (int i = 0, len = skinIds.Count; i < len; i++)
            {
                HarmonyId hid = skinIds[i];
                skinsFlattened[hid.Id] = hid.Name;
            }
            skins.AddRange(skinsFlattened);
        }

        private static void ExtractGroupData(BinaryReader reader, HarmonyInternal.Header.Entry entry, List<string> groups)
        {
            groups.Clear();
            reader.BaseStream.Position = entry.Offset;

            List<HarmonyId> groupIds = new List<HarmonyId>() { new HarmonyId(0, "all") };
            int biggestId = 0;
            for (long i = 0; i < entry.Count; ++i)
            {
                HarmonyId hid;
                HarmonyUtils.ReadHarmonyId(reader, out hid);
                if (hid.Id > biggestId) biggestId = hid.Id;
                groupIds.Add(hid);
            }

            string[] groupsFlattened = new string[biggestId + 1];
            for (int i = 0, len = groupIds.Count; i < len; i++)
            {
                HarmonyId hid = groupIds[i];
                groupsFlattened[hid.Id] = hid.Name;
            }
            groups.AddRange(groupsFlattened);
        }

        private static void ExtractPropMetaData(BinaryReader reader, HarmonyInternal.Header.Entry entry, List<PropMeta> props)
        {
            props.Clear();
            reader.BaseStream.Position = entry.Offset;

            for (long i = 0; i < entry.Count; ++i)
            {
                string clipName = HarmonyUtils.ReadHarmonyString(reader);
                string playName = HarmonyUtils.ReadHarmonyString(reader);
                PropMeta meta = new PropMeta(clipName, playName);
                props.Add(meta);
            }
        }

        private static void ExtractAnchorMetaData(BinaryReader reader, HarmonyInternal.Header.Entry entry, List<AnchorMeta> anchors)
        {
            anchors.Clear();
            reader.BaseStream.Position = entry.Offset;

            for (long i = 0; i < entry.Count; ++i)
            {
                string playName = HarmonyUtils.ReadHarmonyString(reader);
                string nodeName = HarmonyUtils.ReadHarmonyString(reader);
                AnchorMeta meta = new AnchorMeta(playName, nodeName);
                if (anchors.IndexOf(meta) < 0)
                {
                    anchors.Add(meta);
                }
            }
        }

        private static void ExtractGenericMetaData(BinaryReader reader, HarmonyInternal.Header.Entry entry, List<GenericMeta> anchors)
        {
            anchors.Clear();
            reader.BaseStream.Position = entry.Offset;

            for (long i = 0; i < entry.Count; ++i)
            {
                string name = HarmonyUtils.ReadHarmonyString(reader);
                string clipName = HarmonyUtils.ReadHarmonyString(reader);
                string playName = HarmonyUtils.ReadHarmonyString(reader);
                string nodeName = HarmonyUtils.ReadHarmonyString(reader);
                string value = HarmonyUtils.ReadHarmonyString(reader);
                GenericMeta meta = new GenericMeta(name, clipName, playName, nodeName, value);
                anchors.Add(meta);
            }
        }
    }
}