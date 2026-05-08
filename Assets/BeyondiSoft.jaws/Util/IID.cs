namespace beyondi.Util
{
    public interface IID
    {
        int ID { get; set; }
    }

    public static class IDExtenstion
    {
        public static void AutoFillID(this IID[] iids)
        {
            for (var i = 0; i < iids.Length; i++)
                iids[i].ID = i + 1;
        }
    }
}
