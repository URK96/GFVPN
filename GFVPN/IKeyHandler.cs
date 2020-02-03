using Java.Nio.Channels;

namespace GFVPN
{
    public interface IKeyHandler
    {
        void onKeyReady(SelectionKey key);
    }
}