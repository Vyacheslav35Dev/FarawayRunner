public struct LoadingProgress 
{
    public float percents;
    public long bytesCount;
    public long totalBytesCount;

    public LoadingProgress(float percents, long bytesCount, long totalBytesCount)
    {
        this.percents = percents;
        this.bytesCount = bytesCount;
        this.totalBytesCount = totalBytesCount;
    }
}
