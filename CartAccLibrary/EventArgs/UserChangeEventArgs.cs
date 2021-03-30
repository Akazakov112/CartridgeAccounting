namespace CartAccLibrary.EventArgs
{
    public class UserChangeEventArgs
    {
        public enum Props
        {
            Fullname,
            Osp,
            Access,
            Active
        }

        public Props Prop { get; }

        public string NewValue { get; }

        public UserChangeEventArgs(Props prop, string newValue)
        {
            Prop = prop;
            NewValue = newValue;
        }
    }
}
