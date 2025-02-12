namespace DuplicateHider.Cache
{
    interface IGeneratorCache<TItem, TArg>
    {
        TItem GetOrGenerate(TArg arg);
        void Clear();
    }
}
