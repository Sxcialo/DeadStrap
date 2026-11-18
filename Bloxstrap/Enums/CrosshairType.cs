namespace Bloxstrap.Enums
{
    public enum CrosshairType
    {
        [EnumSort(Order = 1)]
        [EnumName(FromTranslation = "Common.Default")]
        Default,

        [EnumSort(Order = 2)]
        [EnumName(StaticName = "Red glow dot")]
        RedGlowDot,

        [EnumSort(Order = 3)]
        [EnumName(StaticName = "Black dot")]
        BlackDot
    }
}
