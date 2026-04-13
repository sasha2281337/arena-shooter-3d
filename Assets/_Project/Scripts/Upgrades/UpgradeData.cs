using UnityEngine;

namespace ArenaShooter.Upgrades
{
    [CreateAssetMenu(fileName = "UPG_NewUpgrade", menuName = "Arena Shooter/Upgrades/Upgrade Data")]
    public class UpgradeData : ScriptableObject
    {
        [SerializeField] private string title = "Upgrade";
        [SerializeField, TextArea] private string description = "Upgrade description.";
        [SerializeField] private UpgradeKind kind;
        [SerializeField, Min(0f)] private float value = 10f;

        public string Title => title;
        public string Description => description;
        public UpgradeKind Kind => kind;
        public float Value => value;
    }
}
