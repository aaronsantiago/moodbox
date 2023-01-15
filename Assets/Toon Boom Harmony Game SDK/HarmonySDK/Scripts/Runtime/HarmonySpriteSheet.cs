using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ToonBoom.Harmony
{
    internal class HarmonyBinarySpriteSheet
    {
        private struct Header
        {
            public const string MagicBytes = "SPSD";
            public const int MajorVersion = 4;
            public const int MinorVersion = 0;

            public static int GetMarshalSize()
            {
                int size = 0;
                size += sizeof(byte) * 4; // SPSD, no length needed, ASCII
                size += sizeof(int); // MajorVersion
                size += sizeof(int); // MinorVersion
                return size;
            }

            public static void WriteToStream(BinaryWriter writer)
            {
                writer.Write(MagicBytes[0]);
                writer.Write(MagicBytes[1]);
                writer.Write(MagicBytes[2]);
                writer.Write(MagicBytes[3]);
                writer.Write(MajorVersion);
                writer.Write(MinorVersion);
            }
        }

        public string SheetFilename = "unitycustom";
        public string SheetName;
        public string SheetResolution;

        public List<HarmonyInternal.BinaryTextureData> Textures;
        public Dictionary<string, HarmonyInternal.BinarySpriteData> Sprites;

        public static int GetStringMarshalSize(string s)
        {
            int size = 0;
            size += sizeof(int); //length;
            if (s != null)
            {
                size += sizeof(byte) * s.Length; //characters as ASCII
            }
            return size;
        }

        public static void WriteStringToStream(BinaryWriter writer, string str)
        {
            writer.Write(str.Length);
            if (str != null)
            {
                writer.Write(Encoding.ASCII.GetBytes(str));
            }
        }

        public int GetMarshalSize()
        {
            int size = 0;

            size += Header.GetMarshalSize();

            size += GetStringMarshalSize(SheetFilename); //SheetFilename
            size += GetStringMarshalSize(SheetName); //SheetName
            size += GetStringMarshalSize(SheetResolution); //SheetResolution

            size += sizeof(int); //Texture length
            if (Textures != null)
            {
                size += HarmonyInternal.BinaryTextureData.GetMarshalSize() * Textures.Count;
            }

            size += sizeof(int); //Sprites length
            if (Sprites != null)
            {
                foreach (KeyValuePair<string, HarmonyInternal.BinarySpriteData> kvp in Sprites)
                {
                    size += GetStringMarshalSize(kvp.Key);
                    size += HarmonyInternal.BinarySpriteData.GetMarshalSize();
                }
            }

            return size;
        }

        public void StoreToMemory(BinaryWriter writer)
        {
            Header.WriteToStream(writer);

            WriteStringToStream(writer, SheetFilename);
            WriteStringToStream(writer, SheetName);
            WriteStringToStream(writer, SheetResolution);

            if (Textures == null)
            {
                writer.Write((int)0);
            }
            else
            {
                writer.Write(Textures.Count);
                for (int i = 0, len = Textures.Count; i < len; i++)
                {
                    Textures[i].WriteToStream(writer);
                }
            }

            if (Sprites == null)
            {
                writer.Write((int)0);
            }
            else
            {
                writer.Write(Sprites.Count);
                foreach (KeyValuePair<string, HarmonyInternal.BinarySpriteData> kvp in Sprites)
                {
                    WriteStringToStream(writer, kvp.Key);
                    kvp.Value.WriteToStream(writer);
                }
            }
        }

        public static HarmonyBinarySpriteSheet MakeFromCustomSprites(List<Sprite> sprites, string sheetName, string resolution, List<CustomSpriteData> customSprites)
        {
            HarmonyBinarySpriteSheet sheet = new HarmonyBinarySpriteSheet();
            sheet.SheetName = sheetName;
            sheet.SheetResolution = resolution;
            sheet.Textures = new List<HarmonyInternal.BinaryTextureData>();
            sheet.Sprites = new Dictionary<string, HarmonyInternal.BinarySpriteData>();

            int globalTextureId = 0;
            Dictionary<Texture, int> texturesHash = Buffers.GetTextureHashBuffer();

            try
            {
                if (sprites != null)
                {
                    for (int i = 0, len = sprites.Count; i < len; i++)
                    {
                        Texture2D texture;
                        Sprite sprite = sprites[i];

                        texture = sprite.texture;
                        if (!texturesHash.TryGetValue(texture, out int textureId))
                        {
                            textureId = globalTextureId++;

                            sheet.Textures.Add(new HarmonyInternal.BinaryTextureData(textureId, texture.width, texture.height));
                            texturesHash.Add(texture, textureId);
                        }

                        HarmonyInternal.BinarySpriteData spriteData = new HarmonyInternal.BinarySpriteData();
                        string name = Path.ChangeExtension(sprite.name.Replace("(Clone)", ""), ".png");
                        spriteData.Rect = sprite.textureRect;
                        spriteData.Rect.y = texture.height - spriteData.Rect.y - spriteData.Rect.height;
                        spriteData.TextureId = textureId;

                        string fullname = sheetName + "/" + resolution + "/" + name;
                        CustomSpriteData csd = customSprites.Find(x => x.Name == fullname);
                        if (csd.Matrix == null)
                        {
                            Debug.LogError("Couldn't find " + fullname);
                            spriteData.Matrix = HarmonyUtils.GenerateIdentityMatrixCopy;
                        }
                        else
                        {
                            spriteData.Matrix = csd.Matrix;
                        }
                        sheet.Sprites.Add(name, spriteData);
                    }
                }
            }
            finally
            {
                texturesHash.Clear();
            }

            return sheet;
        }

        private static class Buffers
        {
            [ThreadStatic]
            private static Dictionary<Texture, int> _texturesHash;
            internal static Dictionary<Texture, int> GetTextureHashBuffer(int neededCount = 8)
            {
                if (_texturesHash == null)
                {
                    _texturesHash = new Dictionary<Texture, int>(neededCount);
                }
                else
                {
                    _texturesHash.Clear();
                }
                return _texturesHash;
            }
        }
    }
}
