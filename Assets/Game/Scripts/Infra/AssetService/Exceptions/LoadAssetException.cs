using System;

[Serializable]
public class LoadAssetEsception : Exception
{
    public LoadAssetEsception() { }

    public LoadAssetEsception(string message)
        : base(message) { }

    public LoadAssetEsception(string message, Exception inner)
        : base(message, inner) { }
}