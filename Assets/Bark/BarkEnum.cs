namespace Bark
{
    public enum BarkLevel
    {
        active, //默认值，系统会立即亮屏显示通知
        timeSensitive, //时效性通知，可在专注状态下显示通知。
        passive, //仅将通知添加到通知列表，不会亮屏提醒。
    }
    
    
    public enum BarkSound
    {
        
    }
    
    
    public enum BarkArchive
    {
        NONE,
        Archive,
        UnArchive,
    }
}