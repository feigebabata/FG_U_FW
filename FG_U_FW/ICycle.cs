
namespace FG_U_FW
{
    public interface ICycle
    {
        void OnCreate();
        void OnShow(object _data);
        void OnFocus();
        void OnUnFocus();
        void OnHide();
        void OnDestroy();
    }
}
