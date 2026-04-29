using UnityEngine;

[CreateAssetMenu(fileName = "TarotCard_", menuName = "Starry/Tarot Card")]
public class TarotCardData : ScriptableObject
{
    public int id;
    public string cardName;
    [TextArea(3, 5)]
    public string description;
    public Sprite frontSprite;
    public Sprite backSprite;
}
