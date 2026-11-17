// 悬停状态的宿主接口，供共享按钮把指针状态传给业务层。
namespace CZCIT.GlassUI
{
    /// <summary>Implemented by host menus that pause movement while a button is hovered.</summary>
    public interface IGlassHoverReceiver
    {

        void SetButtonHover(bool hover);
    }
}
