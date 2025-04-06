using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpriteManager : NetworkBehaviour
{
    public List<SpriteMapping> spriteMappings;
    private Dictionary<string, Sprite> spriteDict;

    [System.Serializable]
    public class SpriteMapping
    {
        public string key;    
        public Sprite sprite; 
    }

    void Awake()
    {
        spriteDict = new Dictionary<string, Sprite>();
        foreach (var mapping in spriteMappings)
        {
            if (!string.IsNullOrEmpty(mapping.key) && mapping.sprite != null)
            {
                spriteDict[mapping.key] = mapping.sprite;
            }
        }
    }

    public Sprite GetSprite(string spriteKey)
    {
        if (spriteDict.TryGetValue(spriteKey, out Sprite foundSprite))
        {
            return foundSprite;
        }
        else
        {
            return null; 
        }
    }
}
