namespace YoutubeSubVideoManager
{
    public interface ILoadStoreable
    {
        void Load();
        void Store();
    }

    public abstract class LoadStoreable : ILoadStoreable
    {
        public virtual void Load()
        {
            if (Program.cmdLineArgs.OnlyCache)
            {
                LoadFromCache();
            }
            else
            {
                if (Program.cmdLineArgs.NoCache)
                {
                    LoadFromYoutube();
                }
                else
                {
                    if (!LoadFromCache())
                    {
                        LoadFromYoutube();
                    }
                }
            }
        }

        protected abstract bool LoadFromCache();

        protected abstract void LoadFromYoutube();

        public abstract void Store();
    }
}
