// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayerAnimatorManager.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking Demos
// </copyright>
// <summary>
//  Used in DemoAnimator to cdeal with the networked player Animator Component controls.
// </summary>
// <author>developer@exitgames.com</author>
// 中文注释：胡良云（CloudHu）
// --------------------------------------------------------------------------------------------------------------------


using UnityEngine;
using System.Collections;

namespace ExitGames.Demos.DemoAnimator
{
	public class PlayerAnimatorManager : Photon.MonoBehaviour 
	{
		#region PUBLIC PROPERTIES

		public float DirectionDampTime = 0.25f;

		#endregion

		#region PRIVATE PROPERTIES

		Animator animator;

		#endregion

		#region MONOBEHAVIOUR MESSAGES

		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity during initialization phase.
		/// </summary>
	    void Start () 
	    {
	        animator = GetComponent<Animator>();    //获取动画组件
        }
	        
		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity on every frame.
		/// </summary>
	    void Update () 
	    {

			// Prevent control is connected to Photon and represent the localPlayer
	        if( photonView.isMine == false && PhotonNetwork.connected == true )
	        {
	            return;
	        }

            //如果没有获取到则报错
            if (!animator)
	        {
				return;//如果没有获取到动画组件，则用return中断
            }

            // 处理跳跃
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

            // 只有当我们在奔跑的时候才能跳跃。
            if (stateInfo.IsName("Base Layer.Run"))
            {
                // 何时使用触发参数。
                if (Input.GetButtonDown("Fire2")) animator.SetTrigger("Jump"); 
			}
           
			// 处理移动
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");


            if( v < 0)//数值校正
            {
                v = 0;
            }

            //设置速度参数值
            animator.SetFloat( "Speed", h*h+v*v );
            animator.SetFloat( "Direction", h, DirectionDampTime, Time.deltaTime );
	    }

		#endregion

	}
}