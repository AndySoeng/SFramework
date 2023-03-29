namespace Ex
{
    public static class ExFileSize
    {
        public static float BytesToMb(long bytes)
        {
            return bytes / 1024f / 1024f;
        }
    }
}
