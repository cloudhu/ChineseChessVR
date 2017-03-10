// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CameraWork.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking Demos
// </copyright>
// <summary>
//  Used in DemoAnimator to deal with the Camera work to follow the player
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;

namespace ExitGames.Demos.DemoAnimator
{
    /// <summary>
    /// 相机的工作。跟踪目标
    /// </summary>
    public class CameraWork : MonoBehaviour
	{

		#region Public Properties

		[Tooltip("在本地X-Z平面到目标的距离")]
	    public float distance = 7.0f;

		[Tooltip("我们希望相机高于目标的高度")]
	    public float height = 3.0f;

		[Tooltip("相机高度的平滑时间滞后.")]
	    public float heightSmoothLag = 0.3f;

		[Tooltip("让相机可以从目标抵消垂直，例如提供更多的视野且更少的地面。")]
		public Vector3 centerOffset = Vector3.zero;

		[Tooltip("如果预设的组件正在被Photon Networ实例化把这个属性设置为false，并在需要的时候手动调用OnStartFollowing()")]
		public bool followOnStart = false;

        #endregion

        #region Private Properties

        // 把目标的Transform缓存
        Transform cameraTransform;

        // 如果目标丢失或相机被切换，请在内部保持一个标志来重新连接
        bool isFollowing;

        // 表示当前的速度，这个值在每次你调用SmoothDamp()时被修改。
        private float heightVelocity = 0.0f;

        // 代表我们试图使用SmoothDamp()来达到的位置
        private float targetHeight = 100000.0f;

		#endregion

		#region MonoBehaviour Messages
		
		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity during initialization phase
		/// </summary>
		void Start()
		{
            // 如果想要的话开始跟随目标
            if (followOnStart)
			{
				OnStartFollowing();
			}

		}

        /// <summary>
        /// 在所有Update方法被调用后才调用的MonoBehaviour方法。这对于让脚本按顺序执行是有用的。例如一个跟踪相机应该总是在LateUpdate里实现，因为它的追踪对象可能已经在Update中移动了。
        /// </summary>
        void LateUpdate()
		{
            // 在关卡加载时目标Transform也许没有被破坏，因此，我们需要覆盖小概率事件，每一次我们加载一个新场主摄像机是不一样的，在发生时重连
    
            if (cameraTransform == null && isFollowing)
			{
				OnStartFollowing();
			}

            // 只在被明确声明时跟随
            if (isFollowing) {
				Apply ();
			}
		}

        #endregion

        #region Public Methods

        /// <summary>
        /// 引发开始跟随事件。
        /// 当你不知道在编辑什么时候跟随时使用这个，通常实例由photon网络管理。
        /// </summary>
        public void OnStartFollowing()
		{	      
			cameraTransform = Camera.main.transform;
			isFollowing = true;
            // 我们不平滑任何东西，我们直接找到正确的相机拍摄
            Cut();
		}

        #endregion

        #region Private Methods

        /// <summary>
        /// 平滑地跟踪目标
        /// </summary>
        void Apply()
	    {
			Vector3 targetCenter = transform.position + centerOffset;

            // 计算当前与目标旋转角度
            float originalTargetAngle = transform.eulerAngles.y;
	        float currentAngle = cameraTransform.eulerAngles.y;

            // 当摄像机被锁定的时候适应真正的目标角度
            float targetAngle = originalTargetAngle;

			currentAngle = targetAngle;

	        targetHeight = targetCenter.y + height;

            // 对高度进行平滑缓冲
            float currentHeight = cameraTransform.position.y;
	        currentHeight = Mathf.SmoothDamp( currentHeight, targetHeight, ref heightVelocity, heightSmoothLag );

            // 把角度转化成旋转，这样我们就可以重置像机的位置了
            Quaternion currentRotation = Quaternion.Euler( 0, currentAngle, 0 );

            // 把摄像机在x-z平面上的位置设置成：到目标的距离
            cameraTransform.position = targetCenter;
	        cameraTransform.position += currentRotation * Vector3.back * distance;

            // 设置摄像机的高度
            cameraTransform.position = new Vector3( cameraTransform.position.x, currentHeight, cameraTransform.position.z );

            // 总是看向目标
            SetUpRotation(targetCenter);
	    }


        /// <summary>
        /// 将摄像机直接定位到指定的目标和中心。
        /// </summary>
        void Cut( )
	    {
	        float oldHeightSmooth = heightSmoothLag;
	        heightSmoothLag = 0.001f;

	        Apply();

	        heightSmoothLag = oldHeightSmooth;
	    }

        /// <summary>
        /// 设置摄像机的旋转始终在目标后面
        /// </summary>
        /// <param name="centerPos">Center position.</param>
        void SetUpRotation( Vector3 centerPos )
	    {
	        Vector3 cameraPos = cameraTransform.position;
	        Vector3 offsetToCenter = centerPos - cameraPos;

            // 只围绕Y轴生成基础旋转
            Quaternion yRotation = Quaternion.LookRotation( new Vector3( offsetToCenter.x, 0, offsetToCenter.z ) );

	        Vector3 relativeOffset = Vector3.forward * distance + Vector3.down * height;
	        cameraTransform.rotation = yRotation * Quaternion.LookRotation( relativeOffset );

	    }

		#endregion
	}
}