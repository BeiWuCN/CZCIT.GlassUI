// 面板拖动的宿主接口，将通用抓取行为与业务菜单定位解耦。
using UnityEngine;

namespace CZCIT.GlassUI
{
    /// <summary>Host controls panel placement; the handle only translates input into drag requests.</summary>
    public interface IGlassDragTarget : IGlassHoverReceiver
    {
        Transform PanelTransform
        {
            get;
        }
        float FollowDistance
        {
            get;
        }

        void BeginHandleDrag();

        void EndHandleDrag();

        void PositionAroundViewer(Vector3 direction);
    }
}
