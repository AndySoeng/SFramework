namespace SFramework
{
    public class UISubScreenBase
    {
        protected UISubCtrlBase mCtrlBase;

        public UISubCtrlBase CtrlBase
        {
            get { return mCtrlBase; }
        }

        public UISubScreenBase(UISubCtrlBase ctrlBase)
        {
            mCtrlBase = ctrlBase;
            Init();
        }

        virtual protected void Init()
        {

        }

        virtual public void Dispose()
        {

        }
    }
}