using UnityEngine;
using UnityEngine.TextCore;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Runtime.InteropServices;

namespace TMPro
{
    public class TMP_EmojiTexture
    {

#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern int TMP_EmojiTexture_Render(string text, IntPtr buffer, int size);
#elif UNITY_ANDROID
#endif
        private Texture2D m_texture;

        public Texture2D texture { get { return m_texture; } }

        private IntPtr m_buffer = IntPtr.Zero;

        private int m_size;
        private int m_bufferSize;


        public TMP_EmojiTexture(int size)
        {
            m_size = size;
            m_texture = new Texture2D(m_size, m_size, TextureFormat.RGBA32, false);

            m_bufferSize = m_size * m_size * 4;
            m_buffer = Marshal.AllocHGlobal(m_bufferSize);
        }

        ~TMP_EmojiTexture()
        {
            // 버퍼 날리고~
            if(m_buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(m_buffer);
                m_buffer = IntPtr.Zero;
            }
            
            m_texture = null;
        }

        public void Render(string emoji)
        {
            if ( m_texture == null || m_buffer == null )
            {
                return;
            }
#if UNITY_EDITOR
            // 디버그용으로.. texture에 음 빨갛게 칠해볼까?
            unsafe
            {
                byte* bytes = (byte*)m_buffer.ToPointer();
                for( int offset = 0; offset < m_bufferSize; offset+=4)
                {
                    bytes[offset + 0] = 0xff;
                    bytes[offset + 1] = 0xff;
                    bytes[offset + 2] = 0xff;
                    bytes[offset + 3] = 0xff;
                }
            }
#elif UNITY_IOS
            TMP_EmojiTexture_Render(emoji, m_buffer, m_size);
#elif UNITY_ANDROID
#endif
            m_texture.LoadRawTextureData(m_buffer, m_bufferSize);
            m_texture.Apply();
        }
    }

    public class TMP_EmojiHelper
    {
        const int EMOJI_SIZE = 64; 
        const int SHEET_TILES = 32; 
        const int SHEET_SIZE = SHEET_TILES * EMOJI_SIZE;

        private static TMP_EmojiSpriteAsset m_emojiSpriteAsset = null;
        private static TMP_EmojiSpriteAsset m_currentEmojiSpriteAsset = null;

        private static int m_currentEmojiIndex = 0;

        public static TMP_EmojiSpriteAsset emojiSpriteAsset
        {
            get
            {
                if (m_emojiSpriteAsset == null)
                {
                    m_emojiSpriteAsset = CreateEmojiSpriteAsset();
                }

                if (m_currentEmojiSpriteAsset == null )
                {
                    m_currentEmojiSpriteAsset = m_emojiSpriteAsset;
                }

                return m_emojiSpriteAsset;
            }
        }

        private static TMP_EmojiTexture m_emojiTexture = null;
        public static TMP_EmojiTexture emojiTexture
        {
            get
            {
                if (m_emojiTexture == null)
                {
                    m_emojiTexture = new TMP_EmojiTexture(EMOJI_SIZE);
                }

                return m_emojiTexture;
            }
        }
        public static bool CanCopyTextures
        {
            get { return SystemInfo.copyTextureSupport != UnityEngine.Rendering.CopyTextureSupport.None; }
        }

        //
        public static bool GetEmojiFromSpriteAsset(string emoji, bool isUpdate)
        {
            bool result = true;
            // 해당 이모지가 렌더되어 있나?
            int hash = TMP_TextUtilities.GetSimpleHashCode(emoji);
            int spriteIndex = emojiSpriteAsset.GetSpriteIndexFromHashcode(hash);
            if (spriteIndex == -1)
            {
                if (emojiSpriteAsset.fallbackSpriteAssets != null && emojiSpriteAsset.fallbackSpriteAssets.Count > 0)
                {
                    foreach(var asset in emojiSpriteAsset.fallbackSpriteAssets)
                    {
                        spriteIndex = asset.GetSpriteIndexFromHashcode(hash);
                        if (spriteIndex != -1)
                        {
                            break;
                        }
                    }
                }
            }

            if (spriteIndex == -1)
            {
                result = false;

                // 시트 업데이트가 필요하다.
                if (isUpdate )
                {
                    UpdateEmoji(emoji);
                }
            }

            return result;
        }

        public static void UpdateEmoji(string emoji)
        {
            if (m_currentEmojiIndex >= SHEET_TILES * SHEET_TILES )
            {
                //새로운 시트를 생성해야해
                var newAsset = CreateEmojiSpriteAsset();
                m_emojiSpriteAsset.fallbackSpriteAssets.Add(newAsset);
                m_currentEmojiSpriteAsset = m_emojiSpriteAsset;
                m_currentEmojiIndex = 0;
            }

            int row = m_currentEmojiIndex % SHEET_TILES;
            int column = m_currentEmojiIndex / SHEET_TILES;

            // 일단 이모지를 렌더하고?
            emojiTexture.Render(emoji);

            if ( CanCopyTextures)
            {
                // 카핏!
                Graphics.CopyTexture(emojiTexture.texture, 0, 0, 0, 0, EMOJI_SIZE, EMOJI_SIZE,
                    m_currentEmojiSpriteAsset.spriteSheet, 0, 0, 
                    row * EMOJI_SIZE, SHEET_SIZE - (column + 1) * EMOJI_SIZE);
            }
            else
            {
                // 텍스처에 일일이 복사를..?
                Debug.Log("아직 텍스처 복사 코드 안넣음");
            }

            TMP_SpriteGlyph spriteGlyph = new TMP_SpriteGlyph();
            spriteGlyph.index = (uint)m_currentEmojiIndex;
            spriteGlyph.sprite = null;
            spriteGlyph.metrics = new GlyphMetrics(EMOJI_SIZE, EMOJI_SIZE, 0, EMOJI_SIZE * 0.9f, EMOJI_SIZE);
            spriteGlyph.glyphRect = new GlyphRect(row * EMOJI_SIZE, (SHEET_SIZE) - ((column + 1) * EMOJI_SIZE), EMOJI_SIZE-1, EMOJI_SIZE-1);

            spriteGlyph.scale = 1.0f;
            spriteGlyph.atlasIndex = 0;

            m_currentEmojiSpriteAsset.spriteGlyphTable.Add(spriteGlyph);

            TMP_SpriteCharacter spriteCharacter = new TMP_SpriteCharacter();
            spriteCharacter.glyph = spriteGlyph;
            spriteCharacter.glyphIndex= spriteGlyph.index;
            spriteCharacter.unicode = (uint)TMP_TextUtilities.GetSimpleHashCode(emoji); // 해시코드로 속이자.
            spriteCharacter.name = emoji;
            spriteCharacter.scale = 1.0f;

            m_currentEmojiSpriteAsset.spriteCharacterTable.Add(spriteCharacter);
            m_currentEmojiSpriteAsset.UpdateLookupTables();

            ++m_currentEmojiIndex;
        }

        private static TMP_EmojiSpriteAsset CreateEmojiSpriteAsset()
        {
            // 밉맵 만들면 안된다..
            var texture = new Texture2D(SHEET_SIZE, SHEET_SIZE, TextureFormat.RGBA32, false);
            if (CanCopyTextures)
            {
                texture.Apply(false, true);
            }

            var spriteAsset = ScriptableObject.CreateInstance<TMP_EmojiSpriteAsset>();
            spriteAsset.fallbackSpriteAssets = new List<TMP_SpriteAsset>();
            spriteAsset.spriteInfoList = new List<TMP_Sprite>();
            spriteAsset.spriteSheet = texture;
            spriteAsset.material = new Material(Shader.Find("TextMeshPro/Sprite"));
            spriteAsset.material.mainTexture = spriteAsset.spriteSheet;
            return spriteAsset;
        }
    }

    [ExcludeFromPresetAttribute]
    public class TMP_EmojiSpriteAsset : TMP_SpriteAsset
    {
        // 음..... unicode 하나만이 아니라 string 혹은 전체 이모지의 hash를 기준으로... 관리를 해야하기 때문에
        // 클래스를 상속받는다.
    }
}
